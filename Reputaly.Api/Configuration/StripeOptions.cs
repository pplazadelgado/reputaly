namespace Reputaly.API.Configuration;

public class StripeOptions
{
    public const string SectionName = "Stripe";

    /// <summary>Clave secreta de Stripe (sk_xxx). SECRETO.</summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>Secreto de verificación del webhook (whsec_xxx). SECRETO.</summary>
    public string WebhookSecret { get; set; } = string.Empty;

    /// <summary>Lista de planes disponibles. Incluye el free.</summary>
    public List<SubscriptionPlan> Plans { get; set; } = new();
}
