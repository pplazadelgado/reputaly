using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Reputaly.API.Infrastructure;
using Reputaly.API.Infrastructure.Multitenancy;
using Reputaly.API.Infrastructure.Services;

namespace Reputaly.API.Features.Reviews;

[ApiController]
public class TrigguerMockIngestionController: ControllerBase
{
    private readonly IReviewIngestionService _ingestion;
    public readonly ITenantContext _tenant;

    public TrigguerMockIngestionController(
        IReviewIngestionService ingestion,
        ITenantContext tenant)
    {
        _ingestion = ingestion;
        _tenant = tenant;
    }

    // Solo disponible en Development - no queremos est en produccion
    [Authorize]
    [DevelopmentOnly]
    [HttpPost("reviews/mock/ingest/{locationId:guid}")]
    public async Task<IActionResult> Handle(Guid locationId)
    {
        await _ingestion.IngestPendingReviewsAsync(_tenant.TenantId, locationId);
        return Ok(new { message = "Mock ingestion completada" });
    }
}