using Microsoft.AspNetCore.Mvc.TagHelpers.Cache;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Reputaly.API.Infrastructure.Persistence;
using Reputaly.API.Infrastructure.Services.Email;
using Stripe;

namespace Reputaly.API.Infrastructure.Services.Background
{
    public class WeeklySummaryService :BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<WeeklySummaryService> _logger;

        //Zona horaria española (ajusta verano/invierno automaticamente)
        private static readonly TimeZoneInfo SpainTz = ResolveSpainTimeZone();

        private static TimeZoneInfo ResolveSpainTimeZone()
        {
            //Windows usa "Romance Standard Time"; Linux (Railway/Render) usa 
            // "Europe/Madrid". Probamos ambos para funcionar en los dos entornos
            foreach(var id in new[] { "Europe/Madrid", "Romance Standard Time" })
            {
                try
                {
                    return TimeZoneInfo.FindSystemTimeZoneById(id);
                }
                catch (TimeZoneNotFoundException) { /* probamos el siguiente */}
            }
            // Ultimo recurso: UTC(no deberia llegar aqui nunca)
            return TimeZoneInfo.Utc;
        }

        public WeeklySummaryService(
            IServiceScopeFactory scopeFactory,
            ILogger<WeeklySummaryService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Bucle principal: espera hasta el proximo lunes 4:00, ejecuta y repite
            while (!stoppingToken.IsCancellationRequested)
            {
                var delay = TimeUntilNextMondayAt4Am();
                _logger.LogInformation("WeeklySummaryService: proxima ejecucion en {Hours:F1}h", delay.TotalHours);

                try
                {
                    await Task.Delay(delay, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    break; // la app se esta apagando
                }

                if (stoppingToken.IsCancellationRequested) break;

                try
                {
                    await RunWeeklySummariesAsync(stoppingToken);
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, "WeeklySummaryService: error en la ejecucion semanal");
                }
            }
        }

        //Crea un scope para resolver servicios scoped(AppDbContext, IEmailService)
        private async Task RunWeeklySummariesAsync(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var email = scope.ServiceProvider.GetRequiredService<IEmailService>();

            var since = DateTime.UtcNow.AddDays(-7);

            //Tenants con email configuraado y al menos una reseña en los ultimos 7 dias.
            var tenantIds = await db.TenantSettings
                .IgnoreQueryFilters()
                .Where(s => s.NotificationEmail != null && s.NotificationEmail != "")
                .Select(s => s.TenantId)
                .ToListAsync(ct);

            var tenantWithActivity = await db.Reviews
                .IgnoreQueryFilters()
                .Where(r => r.CreatedAt <= since && tenantIds.Contains(r.TenantId))
                .Select(r => r.TenantId)
                .ToListAsync(ct);

            _logger.LogInformation(
                "WeeklySummaryService: enviando resumen a {Count} tenants", tenantWithActivity);

            foreach(var tenantId in tenantWithActivity)
            {
                if (ct.IsCancellationRequested) break;
                // SendWeeklySummaryAsync ya es best-effort(captura sus errores)
                await email.SendWeeklySummaryAsync(tenantId);
            }
        }

        // Calcula cuanto falta hasta el proximo lunes a las 04:00 hora española
        // Calcula cuánto falta hasta el próximo lunes a las 4:00 hora española.
        private static TimeSpan TimeUntilNextMondayAt4Am()
        {
            // Hora actual en España.
            var nowSpain = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, SpainTz);

            // Días hasta el próximo lunes.
            int daysUntilMonday = ((int)DayOfWeek.Monday - (int)nowSpain.DayOfWeek + 7) % 7;

            // Objetivo: próximo lunes a las 4:00 (en hora española, sin info de zona aún).
            var nextRunSpain = nowSpain.Date.AddDays(daysUntilMonday).AddHours(4);

            // Si ya pasó (p.ej. hoy es lunes y son las 6:00), saltamos a la semana siguiente.
            if (nextRunSpain <= nowSpain)
                nextRunSpain = nextRunSpain.AddDays(7);

            // Convertimos ese instante (hora española) de vuelta a UTC.
            var nextRunUtc = TimeZoneInfo.ConvertTimeToUtc(nextRunSpain, SpainTz);

            return nextRunUtc - DateTime.UtcNow;
        }
    }
}
