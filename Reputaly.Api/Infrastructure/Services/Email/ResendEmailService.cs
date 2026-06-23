using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Reputaly.API.Configuration;
using Reputaly.API.Infrastructure.Persistence;
using System;
using System.Text;
using System.Text.Json;

namespace Reputaly.API.Infrastructure.Services.Email
{
    public class ResendEmailService :IEmailService
    {
        private readonly AppDbContext _db;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;
        private readonly ResendOptions _options;
        private readonly ILogger<ResendEmailService> _logger;

        public ResendEmailService(
            AppDbContext db,
            IHttpClientFactory httpClientFactory,
            IConfiguration config,
            IOptions<ResendOptions> options,
            ILogger<ResendEmailService> logger)
        {
            _db = db;
            _httpClientFactory = httpClientFactory;
            _config = config;
            _options = options.Value;
            _logger = logger;
        }

        public async Task SendEscalationEmailAsync(
            Guid tenantId, Guid reviewId, bool autoRepliedByTimeout = false)
        {
            try
            {
                var tenant = await _db.Tenants
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(t => t.Id == tenantId);

                var settings = await _db.TenantSettings
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(s => s.TenantId == tenantId);

                var review = await _db.Reviews
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(r => r.Id == reviewId && r.TenantId == tenantId);

                // Si falta algo o no hay email configurado, no enviamos (best-effort)
                if(tenant is null || settings is null || review is null
                    || string.IsNullOrWhiteSpace(settings.NotificationEmail))
                {
                    return;
                }

                var baseUrl = _config["Frontend:BaseUrl"];
                var reviewUrl = $"{baseUrl}/reviews?review={review.Id}";

                var subject = autoRepliedByTimeout
                    ? $"La Ia ha respondido una reseña - {tenant.Name}"
                    : $"Nueva reseña requiere tu atencion - {tenant.Name}";

                var html = EmailTemplates.Escalation(
                    review, tenant.Name, reviewUrl, autoRepliedByTimeout);

                await SendAsync(settings.NotificationEmail, subject, html);
            }
            catch(Exception ex)
            {
                // Best - effort: un fallo de email nunca rompe el flujo que nos llamó.
                _logger.LogError(ex,
                   "Fallo enviando email de escalado (tenant {TenantId}, review {ReviewId}). DETALLE: {Message} | INNER: {Inner}",
                   tenantId, reviewId, ex.Message, ex.InnerException?.Message);
            }
        }

        public async Task SendWeeklySummaryAsync(Guid tenantId)
        {
            try
            {
                var tenant = await _db.Tenants
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(t => t.Id == tenantId);

                var settings = await _db.TenantSettings
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(s => s.TenantId == tenantId);

                if (tenant is null || settings is null
                    || string.IsNullOrWhiteSpace(settings.NotificationEmail))
                {
                    return;
                }

                // Ventana: ultimos 7 dias.
                var since = DateTime.UtcNow.AddDays(-7);

                // Traemos las reseñas de la semana una vez y cualculamos sobre ellas.
                var weekReviews = await _db.Reviews
                     .IgnoreQueryFilters()
                     .Where(r => r.TenantId == tenantId && r.CreatedAt >= since)
                     .ToListAsync();

                // Si no hubo actividad, no enviamos(El hosted service ya filtra esto,
                // pero lo comprobamos tambien aqui por seguridad).
                if (weekReviews.Count == 0) return;

                var total = weekReviews.Count;
                var avgRating = Math.Round(weekReviews.Average(r => r.Rating), 1);
                var autoReplied = weekReviews.Count(r => r.Status == "auto_replied");
                var escalated = weekReviews.Count(r => r.Status == "escalated");
                var pending = weekReviews.Count(r => r.Status == "pending");

                var stats = new WeeklySummaryStats(total, avgRating, autoReplied, escalated, pending);

                var html = EmailTemplates.WeeklySummary(tenant.Name, stats);
                var subject = $"Resumen semanal de reseñas - {tenant.Name}";

                await SendAsync(settings.NotificationEmail, subject, html);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fallo enviando el resumen semanal (tenan) {TenantId}", tenantId);
            }
        }

        // -------------------------------------------------------
        // Envío crudo a la API de Resend
        // -------------------------------------------------------
        private async Task SendAsync(string to, string subject, string html)
        {
            var body = new
            {
                from = $"{_options.FromName}<{_options.FromEmail}>",
                to = new[] { to },
                subject,
                html
            };

            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_options.ApiKey}");

            var response = await httpClient.PostAsync("https://api.resend.com/emails", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error de Resend API [{StatusCode}]: {Error}",
                    (int)response.StatusCode, error);
            }
            else
            {
                _logger.LogInformation("Email enviado a {To}: {Subject}", to, subject);
            }
        }

        // Datos calculados del resumen semanal.
        public record WeeklySummaryStats(
            int Total,
            double AvgRating,
            int AutoReplied,
            int Escalated,
            int Pending);

    }
}
