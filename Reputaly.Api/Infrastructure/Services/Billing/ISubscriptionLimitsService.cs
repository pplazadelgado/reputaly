namespace Reputaly.API.Infrastructure.Services.Billing;

public interface ISubscriptionLimitsService
{
    /// <summary>El plan del tenant permite auto-respuesta con IA.</summary>
    Task<bool> CanAutoReplyAsync(Guid tenantId);

    /// <summary>El tenant puede activar una location más según su plan.
    /// Cuenta locations activas (IsActive == true).</summary>
    Task<bool> CanAddLocationAsync(Guid tenantId);

    /// <summary>Respuestas IA que le quedan al tenant este mes.
    /// -1 = ilimitado (plan Pro). 0 = agotado o plan sin IA.</summary>
    Task<int> AiRepliesRemainingAsync(Guid tenantId);
}

