using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reputaly.API.Infrastructure.Persistence;

namespace Reputaly.API.Features.Reviews;

[ApiController]
public class ApproveReviewController : ControllerBase
{
    private readonly AppDbContext _db;

    public ApproveReviewController(AppDbContext db)
    {
        _db = db;
    }

    [Authorize]
    [HttpPost("/reviews/{id:guid}/approved")]
    public async Task<IActionResult> Handle(Guid id)
    {
        var review = await _db.Reviews
            .FirstOrDefaultAsync(r => r.Id == id);

        if (review is null) return NotFound();

        //Solo se puede aprobar si hay una sugerencia de IA pendiente
        if (string.IsNullOrEmpty(review.AiSuggestedReply))
            return BadRequest(new { error = "Esta reseña no tiene respuesta sugerida por IA" });

        if (review.Status == "auto_replied" || review.Status == "replied")
            return BadRequest(new { error = "Esta reseña ya tiene respuesta publicada" });

        //Marcamos como respondida con la sugerencia de la IA
        // Aqui es donde en el futuro llamaresmo a la Google Api para publicar
        review.FinalReply = review.AiSuggestedReply;
        review.Status = "replied";
        review.RepliedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return Ok(new { message = " Respuesta aprobada correctamente" });
    }
}

