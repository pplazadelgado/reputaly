namespace Reputaly.API.Configuration;

/// <summary>
/// Definición de un plan de suscripción. Vive en configuración
/// para poder añadir/modificar planes sin recompilar.
/// </summary>
public class SubscriptionPlan
{
    /// <summary>Identificador interno del plan: "free", "starter", "pro".</summary>
    public string Plan { get; set; } = string.Empty;

    /// <summary>Price ID de Stripe (price_xxx). Vacío para el plan free.</summary>
    public string PriceId { get; set; } = string.Empty;

    /// <summary>Precio mostrado en UI (en euros). Informativo.</summary>
    public decimal MonthlyPrice { get; set; }

    /// <summary>Máximo de ubicaciones. -1 = ilimitado.</summary>
    public int MaxLocations { get; set; }
    
    /// <summary>Respuestas IA al mes. -1 = ilimitado, 0 = sin IA.</summary>
    public int MonthlyAiReplies { get; set; }

    /// <summary>Si el plan permite auto-respuesta.</summary>
    public bool CanAutoReply { get; set; }
}
