namespace Reputaly.API.Infrastructure.Services.Email
{
    public interface IEmailService
    {
        /// <summary>Avisa de una reseña escalada que requiere atención humana.
        /// Si autoRepliedByTimeout es true, el email indica que la IA respondió
        /// automáticamente por timeout (lo usará el Bloque 5).</summary>
        Task SendEscalationEmailAsync(Guid tenantId, Guid ReviewId, bool autoRepliedByTimeout = false);

        /// <summary>Envía el resumen semanal de reseñas al tenant.</summary>
        Task SendWeeklySummaryAsync(Guid tenantId);
    }
}
