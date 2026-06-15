using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Reputaly.API.Configuration;
using Reputaly.API.Infrastructure.Persistence;
using Reputaly.API.Infrastructure.Services.Stripe.Billing;
using Stripe;

namespace Reputaly.API.Features.Billing;

[ApiController]
public class StripeWebhookController: ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IPlanResolver _plans;
    private readonly StripeOptions _options;
    private readonly ILogger<StripeWebhookController> _logger;

    public StripeWebhookController(
        AppDbContext db,
        IPlanResolver plans,
        IOptions<StripeOptions> options,
        ILogger<StripeWebhookController> logger)
    {
        _db = db;
        _plans = plans;
        _options = options.Value;
        _logger = logger;
    }

    [HttpPost("webhooks/stripe")]
    public async Task<IActionResult> Handle()
    {
        // 1. Leer el cuerpo CRUDO (Stripe firma el body sin parsear).
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

        Event stripeEvent;
        try
        {
            // 2. Verificar la firma.
            stripeEvent = EventUtility.ConstructEvent(
                json,
                Request.Headers["Stripe-Signature"],
                _options.WebhookSecret);
        }
        catch (StripeException ex)
        {
            _logger.LogWarning(ex, "Firma de webhook de Stripe inválida.");
            return BadRequest();
        }

        // 3. Procesar por tipo de evento.
        switch (stripeEvent.Type)
        {
            case "customer.subscription.created":
            case "customer.subscription.updated":
                await HandleSubscriptionChange(
                    (Subscription)stripeEvent.Data.Object);
                break;

            case "customer.subscription.deleted":
                await HandleSubscriptionDeleted(
                    (Subscription)stripeEvent.Data.Object);
                break;

            case "invoice.payment_failed":
                _logger.LogWarning("Pago fallido para evento {Id}", stripeEvent.Id);
                // Fase 4: notificar por email.
                break;
        }

        // Siempre 200 si la firma era válida: evita reintentos infinitos de Stripe.
        return Ok();
    }

    private async Task HandleSubscriptionChange(Subscription sub)
    {
        var priceId = sub.Items.Data.FirstOrDefault()?.Price?.Id;


        if (priceId is null) return;

        var plan = _plans.ResolvePlanFromPriceId(priceId);
        if (plan is null)
        {
            _logger.LogWarning("PriceId {PriceId} sin plan mapeado.", priceId);
            return;
        }

        await UpdateTenantPlan(sub.CustomerId, plan);
    }

    private async Task HandleSubscriptionDeleted(Subscription sub)
        => await UpdateTenantPlan(sub.CustomerId, "free");

    private async Task UpdateTenantPlan(string customerId, string plan)
    {
        // OJO: sin filtro de tenant aquí; el webhook no tiene ITenantContext.
        // Por eso buscamos Tenants directamente por StripeCustomerId.
        var tenant = await _db.Tenants
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.StripeCustomerId == customerId);

        if (tenant is null)
        {
            // 200 igualmente: puede ser un customer de otra app o de prueba.
            _logger.LogWarning("No hay tenant con StripeCustomerId {Id}", customerId);
            return;
        }

        tenant.SubscriptionPlan = plan; // idempotente: reescribir el mismo valor es inocuo.
        await _db.SaveChangesAsync();
    }
}



