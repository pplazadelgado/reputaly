namespace Reputaly.API.Domain;

public class Review
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    public Guid LocationId { get; set; }
    public TenantLocation Location { get; set; } = null!;

    // ID único de la reseña en Google — evita duplicados en el polling
    public string GoogleReviewId { get; set; } = string.Empty;

    public string AuthorName { get; set; } = string.Empty;

    // 1 a 5 estrellas
    public int Rating { get; set; }

    // Nullable: hay reseñas de Google que solo tienen estrellas, sin texto
    public string? Content { get; set; }

    public DateTime PublishedAt { get; set; }

    public string? DetectedLanguage {  get; set; }

    // Analisis de IA
    public decimal? SentimentScore {  get; set; }  // -1.0 a 1.0
                                                   
    public string[]? DetectedTopics {  get; set; }  // ["precio","trato","limpieza"]
    public DateTime? AiAnalyzedAt {  get; set; }



    // pending | auto_replied | escalated | replied
    public string Status { get; set; } = "pending";

    // Lo que la IA sugiere responder
    public string? AiSuggestedReply { get; set; }

    // auto_reply | escalate
    public string? AiDecision { get; set; }

    // Por qué la IA tomó esa decisión (útil para debugging y transparencia)
    public string? AiDecisionReason { get; set; }

    // La respuesta final publicada en Google (puede ser la de la IA o una manual)
    public string? FinalReply { get; set; }

    public DateTime? RepliedAt { get; set; }
    public DateTime? EscalatedAt { get; set; }

    // True si la respuesta se publicó automáticamente sin intervención humana
    public bool AutoReplied { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
