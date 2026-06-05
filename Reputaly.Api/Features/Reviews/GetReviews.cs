using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Reputaly.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Reputaly.API.Features.Reviews;

[ApiController]
public class GetReviewsController : ControllerBase
{
    private readonly AppDbContext _db;

    public GetReviewsController(AppDbContext db)
    {
        _db=db;
    }

    [Authorize]
    [HttpGet("/reviews")]
    public async Task<IActionResult> Handle(
        [FromQuery] string? status,
        [FromQuery] int? rating,
        [FromQuery] Guid? locationId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        // IQueryable construye la consulta SQL de forma incremental.
        // Cada .Where() añade un filtro sin ejecutar nada todavía.
        // La consulta real a la base de datos solo ocurre al llamar ToListAsync().
        var query = _db.Reviews.AsQueryable();

        if(!string.IsNullOrEmpty(status) ) 
            query = query.Where(r => r.Status == status);

        if(rating.HasValue)
            query = query.Where(r => r.Rating == rating.Value);

        if (locationId.HasValue)
            query = query.Where(r => r.LocationId == locationId.Value);

        if(from.HasValue)
            query = query.Where(r => r.PublishedAt >= from.Value);

        if (to.HasValue)
            query = query.Where(r => r.PublishedAt <= to.Value);

        var total = await query.CountAsync();

        var reviews = await query
            .OrderByDescending(r => r.PublishedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new ReviewDto(
                r.Id, r.AuthorName, r.Rating, r.Content,
                r.PublishedAt, r.Status, r.AiSuggestedReply,
                r.AiDecision, r.AiDecisionReason, r.FinalReply,
                r.RepliedAt, r.EscalatedAt, r.CreatedAt, r.LocationId,
                r.DetectedLanguage, r.SentimentScore, r.DetectedTopics, r.AutoReplied))
            .ToListAsync();

        return Ok(new
        {
            total,
            page,
            pageSize,
            items = reviews
        });
    }

}
