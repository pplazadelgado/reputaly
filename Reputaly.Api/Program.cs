using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Reputaly.API.Infrastructure.Multitenancy;
using Reputaly.API.Infrastructure.Persistence;
using Reputaly.API.Infrastructure.Services;
using Reputaly.API.Configuration;
using Stripe;
using Microsoft.AspNetCore.DataProtection;
using System.Threading.RateLimiting;
using Reputaly.API.Infrastructure.Services.Stripe;
using Reputaly.API.Infrastructure.Services.Stripe.Billing;
using Reputaly.API.Infrastructure.Services.Billing;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Incluir propiedades null en la respuesta JSON en lugar de omitirlas
        options.JsonSerializerOptions.DefaultIgnoreCondition =
            System.Text.Json.Serialization.JsonIgnoreCondition.Never;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Introduce el token JWT sin el prefijo Bearer"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// EF Core con PostgreSQL (Neon)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// TenantContext
builder.Services.AddScoped<ClerkTenantContext>();
builder.Services.AddScoped<ITenantContext>(sp => sp.GetRequiredService<ClerkTenantContext>());
builder.Services.AddScoped<IReviewIngestionService, ReviewMockService>();
builder.Services.AddScoped<IReviewDecisionEngine, ReviewDecisionEngine>();
builder.Services.AddScoped<ISubscriptionLimitsService, SubscriptionLimitsService>();

// Autenticaci�n JWT con Clerk
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://allowed-turkey-45.clerk.accounts.dev";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false
        };
    });

builder.Services.AddAuthorization();

// Data Protection (cifrado de tokens OAuth)
builder.Services.AddDataProtection();
builder.Services.AddScoped<ITokenEncryptionService, TokenEncryptionService>();
builder.Services.AddScoped<IStripeService, StripeService>();
builder.Services.AddScoped<IPlanResolver, PlanResolver>();

//OAuth state (CSRF protection, en memoria con TTL)
builder.Services.AddSingleton<IOAuthStateService, OAuthStateService>();

builder.Services.AddHttpClient();

// CORS � debe registrarse ANTES de builder.Build()
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendDev", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Configuración tipada de Stripe (Options pattern)
builder.Services.Configure<StripeOptions>(
    builder.Configuration.GetSection(StripeOptions.SectionName));

// Inicializa la API key global de Stripe.net una sola vez al arrancar.
StripeConfiguration.ApiKey =
    builder.Configuration["Stripe:SecretKey"];

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("FrontendDev");
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<TenantMiddleware>();
app.MapControllers();

app.Run();