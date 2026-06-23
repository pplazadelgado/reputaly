namespace Reputaly.API.Infrastructure.Services.Background;

public interface IEscalationTimeoutProcessor
{
    /// <summary>Revisa las reseñas escaladas vencidas y auto-responde las que
    /// proceda. Devuelve cuántas auto-respondió.</summary>
    Task<int> ProcessTimeoutsAsync(CancellationToken ct = default);
}