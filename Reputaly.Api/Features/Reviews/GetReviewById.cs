using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reputaly.API.Infrastructure.Persistence;
using System.Security.Cryptography.X509Certificates;

namespace Reputaly.API.Features.Reviews;

[ApiController]
public class GetReviewByIdController : ControllerBase
{
    private readonly AppDbContext _db;

    public GetReviewByIdController(AppDbContext db)
    {
        _db = db;   
    }

    [Authorize]
    [HttpGet("/reviews/{id:guid}")]
    public async Task<IActionResult> Handle(Guid id)
    {
        var review = await _db.Reviews
            .Where(r => r.Id == id)
            .Select(r => new ReviewDto(
                r.Id, r.AuthorName, r.Rating, r.Content,
                r.PublishedAt, r.Status, r.AiSuggestedReply,
                r.AiDecision, r.AiDecisionReason, r.FinalReply,
                r.RepliedAt, r.EscalatedAt, r.CreatedAt, r.LocationId,
                r.DetectedLanguage, r.SentimentScore, r.DetectedTopics, r.AutoReplied))
            .FirstOrDefaultAsync();

        if (review is null) return NotFound();

        return Ok(review);
    }
}