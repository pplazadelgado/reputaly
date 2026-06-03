using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Reputaly.API.Infrastructure.Persistence;

namespace Reputaly.API.Features.Reviews;

[ApiController]
public class ReplyReviewController: ControllerBase
{
    private readonly AppDbContext _db;

    public ReplyReviewController(AppDbContext db) 
    {  
        _db = db; 
    }

    [Authorize]
    [HttpPost("/reviews/{id:guid}/reply")]
    public async Task<IActionResult> Handle(Guid id, [FromBody] ReplyRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Text))
            return BadRequest(new { error = "El texto de la respuesta no puede estar vacio" });

        var review = await _db.Reviews
            .FirstOrDefaultAsync(r => r.Id == id);

        if (review is null) return NotFound();

        review.FinalReply = request.Text;
        review.Status = "replied";
        review.RepliedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return Ok(new { message = "Repuesta publicada correctamente" });
    }
}

public record ReplyRequest (string Text);

