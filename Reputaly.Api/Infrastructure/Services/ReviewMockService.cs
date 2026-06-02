using Microsoft.EntityFrameworkCore;
using Reputaly.API.Domain;
using Reputaly.API.Infrastructure.Persistence;

namespace Reputaly.API.Infrastructure.Services;

public interface IReviewIngestionService
{
    Task IngestPendingReviewsAsync(Guid tenantId, Guid locationId);
}

public class ReviewMockService : IReviewIngestionService
{
    private readonly AppDbContext _db;
    private readonly ILogger<ReviewMockService> _logger;
    private readonly IReviewDecisionEngine _decisionEngine;

    // Banco de reseñas realistas para simular distintos escenarios
    private static readonly List<(int Rating, string Author, string? Content)> MockReviews = new()
    {
        (5, "María García",    "Excelente servicio, muy profesionales y atentos. Totalmente recomendable."),
        (5, "Carlos Martínez", "Fantástica experiencia, repetiré sin duda. El equipo es muy amable."),
        (4, "Ana López",       "Muy buena atención. El precio es algo elevado pero la calidad lo justifica."),
        (4, "Pedro Sánchez",   "Buen servicio en general, aunque tuve que esperar más de lo previsto."),
        (3, "Laura Fernández", "Correcto pero sin más. Esperaba algo mejor por el precio."),
        (2, "David Rodríguez", "El servicio dejó bastante que desear. No volveré."),
        (1, "Sofía Jiménez",   "Pésima experiencia. Trato déspota y trabajo mal hecho. Vergonzoso."),
        (5, "Miguel Torres",   null), // reseña sin texto, solo estrellas
        (1, "Elena Moreno",    "Estafa total. Me cobraron de más y encima se pusieron a discutir."),
        (3, "Roberto Díaz",    "Normal, ni bien ni mal. Cumple pero no destaca."),
    };

    public ReviewMockService(
        AppDbContext db, 
        ILogger<ReviewMockService> logger,
        IReviewDecisionEngine decisionEngine)
    {
        _db = db;
        _logger = logger;
        _decisionEngine = decisionEngine;
    }

    public async Task IngestPendingReviewsAsync(Guid tenantId, Guid locationId)
    {
        // Cogemos las reseñas que ya existen para esa ubicación
        var existingIds = await _db.Reviews
            .IgnoreQueryFilters()
            .Where(r => r.LocationId == locationId)
            .Select(r => r.GoogleReviewId)
            .ToListAsync();

        var newReviews = new List<Review>();
        var random = new Random();

        // Generamos entre 1 y 3 reseñas nuevas por llamada
        var count = random.Next(1, 4);

        for (int i = 0; i < count; i++)
        {
            // GoogleReviewId único para evitar duplicados
            var googleReviewId = $"mock_{Guid.NewGuid():N}";

            // Nos aseguramos de no insertar un ID que ya existe
            if (existingIds.Contains(googleReviewId)) continue;

            var template = MockReviews[random.Next(MockReviews.Count)];

            newReviews.Add(new Review
            {
                TenantId = tenantId,
                LocationId = locationId,
                GoogleReviewId = googleReviewId,
                AuthorName = template.Author,
                Rating = template.Rating,
                Content = template.Content,
                PublishedAt = DateTime.UtcNow.AddHours(-random.Next(1, 72)),
                Status = "pending",
                CreatedAt = DateTime.UtcNow
            });
        }

        if (newReviews.Any())
        {
            _db.Reviews.AddRange(newReviews);
            await _db.SaveChangesAsync();

            // Procesamos cada reseña nueva con el motor de decisión
            foreach (var review in newReviews)
                await _decisionEngine.ProcessReviewAsync(review.Id);

            _logger.LogInformation(
                "Mock: {Count} reseñas insertadas y procesadas para location {LocationId}",
                newReviews.Count, locationId);
        }
    }

    
}
