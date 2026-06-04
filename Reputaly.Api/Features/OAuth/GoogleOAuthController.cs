using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reputaly.API.Domain;
using Reputaly.API.Infrastructure.Multitenancy;
using Reputaly.API.Infrastructure.Persistence;
using Reputaly.API.Infrastructure.Services;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Reputaly.API.Features.OAuth;

[ApiController]
public class GoogleOAuthController : ControllerBase
{
    // Scopes que pedimos a Google - acceso a Google Business Profile
    private const string Scopes = "https://www.googleapis.com/auth/business.manage";

    private readonly IConfiguration _config;
    private readonly IOAuthStateService _stateService;
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenant;
    private readonly ITokenEncryptionService _encryption;
    private readonly IHttpClientFactory _httpClientFactory;

    public GoogleOAuthController(
        IConfiguration config,
        IOAuthStateService stateservice,
        AppDbContext db,
        ITenantContext tenant,
        ITokenEncryptionService encryption,
        IHttpClientFactory httpClientFactory)
    {
        _config = config;
        _stateService = stateservice;
        _db = db;
        _tenant = tenant;
        _encryption = encryption;
        _httpClientFactory = httpClientFactory;
    }

    // ---------------------------------------------------------------
    // GET /oauth/google/initiate/{locationId}
    // El frontend llama aquí. Redirige al usuario a Google.
    // ---------------------------------------------------------------
    [Authorize]
    [HttpGet("/oauth/google/initiate/{locationId:guid}")]
    public IActionResult Initiate(Guid locationId)
    {
        var clientId = _config["GoogleOAuth:ClientId"]!;
        var redirectUri = _config["GoogleOAuth:RedirectUri"]!;

        //Generamos el state con TenantId + LocationId parqa recuperarlo en el callback
        var state = _stateService.GenerateState(_tenant.TenantId, locationId);

        //Construimos la URL de autorizacion de Google
        var authUrl = "https://accounts.google.com/o/oauth2/v2/auth" +
            $"?client_id={Uri.EscapeDataString(clientId)}" +
            $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
            $"&response_type=code" +
            $"&scope={Uri.EscapeDataString(Scopes)}" +
            $"&state={Uri.EscapeDataString(state)}" +
            $"&access_type=offline" +   // necesario para obtener RefreshToken
            $"&prompt=consent";         // fuerza que Google entregue RefreshToken siempre

        //Redirigimos el navegador del usuario a Google
        return Redirect(authUrl);
    }


    // ---------------------------------------------------------------
    // GET /oauth/google/callback
    // Google redirige aquí tras el consentimiento del usuario.
    // Este endpoint NO lleva [Authorize] — Google lo llama sin JWT.
    // ---------------------------------------------------------------
    [HttpGet("/oauth/google/callback")]
    public async Task<IActionResult> Callback(
        [FromQuery] string code,
        [FromQuery] string state,
        [FromQuery] string? error)
    {
        // El usuario denego el acceso en Google
        if(!string.IsNullOrEmpty(error))
            return Redirect($"{GetFrontendUrl()}/settings?google=denied");

        // Validamos el state - si falla, es un ataque CSRF o el state espiro
        var stateData = _stateService.ValidateAndConsume(state);
        if(stateData is null)
            return Redirect($"{GetFrontendUrl()}/settings?google=invalid_state");

        var (tenantId, locationId) = stateData.Value;

        // Verificamos que la ubicacion existe y pertenece al tenant correcot
        var location = await _db.TenantLocations
            .IgnoreQueryFilters()  // necesario porque el TenantContext no está disponible aquí
            .FirstOrDefaultAsync(l => l.Id == locationId && l.TenantId == tenantId);

        if (location is null)
            return Redirect($"{GetFrontendUrl()}/settings?google=not_found");

        // Intercambiamos el código por tokens con Google
        var tokenResponse = await ExchangeCodeForTokensAsync(code);
        if (tokenResponse is null)
            return Redirect($"{GetFrontendUrl()}/settings?google=token_error");

        // Guardamos los tokens cifrados en la ubicación
        location.GoogleAccessToken = _encryption.Encrypt(tokenResponse.AccessToken);
        location.GoogleRefreshToken = _encryption.Encrypt(tokenResponse.RefreshToken);
        location.GoogleTokenExpiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
        location.GoogleAccountEmail = tokenResponse.Email ?? string.Empty;

        await _db.SaveChangesAsync();

        return Redirect($"{GetFrontendUrl()}/settings?google=connected");
    }

    // ---------------------------------------------------------------
    // Llama a Google para intercambiar el code por tokens
    // ---------------------------------------------------------------
    private async Task<GoogleTokenResponse?> ExchangeCodeForTokensAsync(string code)
    {
        var clientId = _config["GoogleOAuth:ClientId"]!;
        var clientSecret = _config["GoogleOAuth:ClientSecret"]!;
        var redirectUri = _config["GoogleOAuth:RedirectUri"]!;

        var httpClient = _httpClientFactory.CreateClient();

        //Google espera los parametros como form_urlencoded
        var formData = new FormUrlEncodedContent(new[]
        {
             new KeyValuePair<string, string>("code", code),
            new KeyValuePair<string, string>("client_id", clientId),
            new KeyValuePair<string, string>("client_secret", clientSecret),
            new KeyValuePair<string, string>("redirect_uri", redirectUri),
            new KeyValuePair<string, string>("grant_type", "authorization_code"),
        });

        var response = await httpClient.PostAsync(
            "https://oauth2.googleapis.com/token", formData);

        if (!response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<JsonElement>(json);

        // Pedimos el email de la cuenta conectada
        var email = await GetGoogleAccountEmailAsync(
            httpClient, data.GetProperty("access_token").GetString()!);

        return new GoogleTokenResponse(
            AccessToken: data.GetProperty("access_token").GetString()!,
            RefreshToken: data.GetProperty("refresh_token").GetString()!,
            ExpiresIn: data.GetProperty("expires_in").GetInt32(),
            Email: email);
    }

    // ---------------------------------------------------------------
    // Obtiene el email de la cuenta Google que acaba de autorizar
    // ---------------------------------------------------------------
    private async Task<string> GetGoogleAccountEmailAsync(
        HttpClient httpClient, string accessToken)
    {
        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await httpClient.GetAsync(
             "https://www.googleapis.com/oauth2/v3/userinfo");

        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<JsonElement>(json);
        return data.TryGetProperty("email", out var email) ? email.GetString() : null;
    }

    private string GetFrontendUrl() =>
        _config["Frontend:BaseUrl"] ?? "http://localhost:5173";
}

// Recod interno para deserializar la respuesta de Google
internal record GoogleTokenResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    string? Email);

