using Microsoft.EntityFrameworkCore;
using Reputaly.API.Domain;
using Reputaly.API.Infrastructure.Persistence;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Reputaly.API.Infrastructure.Services
{
    public interface IReviewDecisionEngine
    {
        Task ProcessReviewAsync(Guid reviewId);
    }

    public class ReviewDecisionEngine : IReviewDecisionEngine
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ReviewDecisionEngine> _logger;
        private readonly Billing.ISubscriptionLimitsService _limits;

        public ReviewDecisionEngine(
            AppDbContext db,
            IConfiguration config,
            IHttpClientFactory httpClientFactory,
            ILogger<ReviewDecisionEngine> logger,
            Billing.ISubscriptionLimitsService limits)
        {
            _db = db;
            _config = config;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _limits = limits;
        }

        public async Task ProcessReviewAsync(Guid reviewId)
        {
            // Cargamos la reseña con su tenant y settings
            var review = await _db.Reviews
                .IgnoreQueryFilters()
                .Include(r => r.Location)
                .Include(r => r.Tenant)
                .FirstOrDefaultAsync(r => r.Id == reviewId);

            if(review == null)
            {
                _logger.LogWarning("Review {ReviewId} no encontrada", reviewId);
                return;
            }

            var settings = await _db.TenantSettings
                .FirstOrDefaultAsync(s => s.TenantId == review.TenantId);

            if(settings is null)
            {
                _logger.LogWarning("TenantSettings no encontrado para tenant {TenantId}", review.TenantId);
                return;
            }

            // -------------------------------------------------------
            // PASO 1: Reglas duras (sin IA)
            // -------------------------------------------------------
            var hardRuleEscalation = CheckHardRules(review, settings);

            if(hardRuleEscalation is not null)
            {
                review.AiDecision = "escalate";
                review.AiDecisionReason = hardRuleEscalation;
                review.Status = "escalated";
                review.EscalatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();

                _logger.LogInformation(
                    "Review {ReviewId} escalada por regla dura: {Reason}",
                    reviewId, hardRuleEscalation);
                return;
            }

            // -------------------------------------------------------
            // PASO 1.5: Control de límites de suscripción
            // Antes de gastar quota de Claude, comprobamos que el plan
            // del tenant permite IA y que le queda cuota este mes.
            // -------------------------------------------------------
            var canAutoReply = await _limits.CanAutoReplyAsync(review.TenantId);
            if (!canAutoReply)
            {
                review.AiDecision = "escalate";
                review.AiDecisionReason = "El plan actual no incluye respuestas con IA";
                review.Status = "escalated";
                review.EscalatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();

                _logger.LogInformation(
                    "Review {ReviewId} escalada: plan sin IA (tenant {TenantId}",
                    review, review.TenantId);
                return;
            }

            var repliesRemaining = await _limits.AiRepliesRemainingAsync(review.TenantId);
            // - 1 = ilimitado (Pro). 0 = cuota agotada.
            if(repliesRemaining == 0)
            {
                review.AiDecision = "escalate";
                review.AiDecisionReason = "Limite mensual de respuestas IA alcanzado";
                review.Status = "escalated";
                review.EscalatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();

                _logger.LogInformation(
                    "Review {ReviewId} escalada: cuota IA agotada (tenant {TenantId}",
                    reviewId, review.TenantId);
                return;
            }

            // -------------------------------------------------------
            // PASO 2: Llamada a Claude API
            // -------------------------------------------------------
            var aiResult = await CallClaudeAsync(review, settings, review.Tenant);

            if (aiResult is null)
            {
                _logger.LogError("Claude no devolvio respuesta para review {ReviewId}", reviewId);
                return;
            }

            review.AiSuggestedReply = aiResult.SuggestedReply;
            review.AiDecision = aiResult.Decision;
            review.AiDecisionReason = aiResult.Reason;
            review.DetectedLanguage = aiResult.DetectedLanguage;
            review.SentimentScore = aiResult.SentimentScore;
            review.DetectedTopics = aiResult.Topics;
            review.AiAnalyzedAt = DateTime.UtcNow;

            if (aiResult.Decision == "auto_reply")
            {
                review.FinalReply = aiResult.SuggestedReply;
                review.Status = "auto_replied";
                review.RepliedAt = DateTime.UtcNow;
                review.AutoReplied = true;

                _logger.LogInformation(
                    "Review {ReviewId} respondida automaticamente por IA", reviewId);
            }
            else
            {
                review.Status = "escalated";
                review.EscalatedAt = DateTime.UtcNow;

                _logger.LogInformation(
                    "Review {ReviewId} escalada por IA: {Reason}", reviewId, aiResult.Reason);
            }

            await _db.SaveChangesAsync();
        }

        // -------------------------------------------------------
        // Reglas duras: devuelve el motivo si hay que escalar,
        // null si la reseña pasa los filtros
        // -------------------------------------------------------
        private static string? CheckHardRules(Review review, TenantSettings settings)
        {
            if (review.Rating < settings.AutoReplyMinRating)
                return $"Rating {review.Rating} por debajo el minimo {settings.AutoReplyMinRating}";

            if(review.Content is not null && settings.EscalateOnKeywords is not null)
            {
                var contentLower = review.Content.ToLowerInvariant();
                var matchedKeyword = settings.EscalateOnKeywords
                    .FirstOrDefault(k => contentLower.Contains(k.ToLowerInvariant()));

                if (matchedKeyword is not null)
                    return $"Keyword detectada: '{matchedKeyword}'";
            }

            return null;
        }

        // -------------------------------------------------------
        // Llamada a Claude API
        // -------------------------------------------------------
        private async Task<AiDecisionResult?> CallClaudeAsync(
            Review review, TenantSettings settings, Tenant tenant)
        {
            var apiKey = _config["Anthropic:ApiKey"]!;
            var model = _config["Anthropic:Model"] ?? "claude-sonnet-4-5";

            var systemPrompt = BuildSystemPrompt(review, settings, tenant);
            var userPrompt = BuildUserPrompt(review);

            var requestBody = new
            {
                model,
                max_tokens = 1024,
                system = systemPrompt,
                messages = new[]
                {
                    new { role = "user", content = userPrompt }
                }
            };

            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
            httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(
                 "https://api.anthropic.com/v1/messages", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error de Claude API [{StatusCode}]: {Error}",
                    (int)response.StatusCode, error);
                return null;
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            return ParseClaudeResponse(responseJson);
        }

        // -------------------------------------------------------
        // Prompt estructurado para Claude
        // -------------------------------------------------------
        // -------------------------------------------------------
        // -------------------------------------------------------
        // System prompt: rol, restricciones del vertical, config por rating, idioma
        // -------------------------------------------------------
        private string BuildSystemPrompt(Review review, TenantSettings settings, Tenant tenant)
        {
            var sb = new StringBuilder();

            sb.AppendLine("Eres el asistente de gestión de reseñas de Google de un negocio.");
            sb.AppendLine();

            // --- Modificación 4: hardConstraints del vertical (lo primero, no negociable) ---
            var vertical = VerticalTemplates.Get(tenant.Vertical);
            if (vertical is not null && vertical.HardConstraints.Length > 0)
            {
                sb.AppendLine("RESTRICCIONES OBLIGATORIAS (no negociables, tienen prioridad absoluta):");
                foreach (var constraint in vertical.HardConstraints)
                    sb.AppendLine($"- {constraint}");
                sb.AppendLine();
            }

            // --- Modificación 2: config general + específica por rating ---
            sb.AppendLine("INFORMACIÓN DEL NEGOCIO:");
            sb.AppendLine($"Instrucciones generales: {settings.AiConfig.Default.Instructions}");
            sb.AppendLine($"Tono general: {settings.AiConfig.Default.Tone}");

            if (settings.AiConfig.ByRating.TryGetValue(review.Rating.ToString(), out var ratingConfig))
            {
                sb.AppendLine($"Instrucciones para {review.Rating} estrellas: {ratingConfig.Instructions}");
                sb.AppendLine($"Tono para {review.Rating} estrellas: {ratingConfig.Tone}");
            }

            var maxLength = settings.AiConfig.Default.MaxLength;
            if (maxLength.HasValue)
                sb.AppendLine($"Longitud máxima de la respuesta: {maxLength} caracteres.");
            sb.AppendLine();

            // --- Modificación 3: idioma ---
            if (settings.AutoDetectLanguage)
            {
                sb.AppendLine("IDIOMA: Responde en el mismo idioma en que está escrita la reseña.");
            }
            else
            {
                // ResponseLanguage de la ubicación tiene prioridad sobre el del tenant
                var lang = review.Location.ResponseLanguage ?? settings.DefaultResponseLanguage;
                sb.AppendLine($"IDIOMA: Responde siempre en el idioma con código BCP-47 '{lang}'.");
            }
            sb.AppendLine();

            sb.AppendLine("""
            INSTRUCCIONES DE DECISIÓN:
            Analiza la reseña y decide si responder automáticamente o escalarla a un humano.
            Escala si detectas: situaciones legales, quejas graves que requieren investigación
            interna, o lenguaje que requiere tacto especial más allá de una respuesta estándar.

            Responde ÚNICAMENTE con este JSON, sin texto adicional ni markdown:
            {
              "decision": "auto_reply" | "escalate",
              "suggestedReply": "texto de la respuesta (siempre, incluso si escalas)",
              "reason": "explicación breve de la decisión",
              "detectedLanguage": "código BCP-47 del idioma de la reseña (ej. es, en)",
              "sentimentScore": número entre -1.0 y 1.0,
              "topics": ["tema1", "tema2"]
            }
            """);

            return sb.ToString();
        }

        // -------------------------------------------------------
        // User prompt: solo los datos concretos de la reseña
        // -------------------------------------------------------
        private string BuildUserPrompt(Review review)
        {
            var reviewText = string.IsNullOrWhiteSpace(review.Content)
                ? "(El cliente no dejó texto, solo valoración con estrellas)"
                : review.Content;

            return $"""
            RESEÑA A ANALIZAR:
            - Autor: {review.AuthorName}
            - Valoración: {review.Rating}/5 estrellas
            - Texto: {reviewText}
            """;
        }

        // -------------------------------------------------------
        // Parsea la respuesta de Claude y extrae el JSON
        // -------------------------------------------------------
        private AiDecisionResult? ParseClaudeResponse(string responseJson)
        {
            try
            {
                var doc = JsonDocument.Parse(responseJson);
                var text = doc.RootElement
                    .GetProperty("content")[0]
                    .GetProperty("text")
                    .GetString();

                if (string.IsNullOrEmpty(text)) return null;

                text = text.Trim();
                if (text.StartsWith("```"))
                {
                    text = text.Replace("```json", "").Replace("```", "").Trim();
                }

                var result = JsonSerializer.Deserialize<AiDecisionResult>(text,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error parseando respuesta de Claude: {Error}", ex.Message);
                return null;
            }
        }

    }

    // Record interno para deserializar la respuesta de Claude
    internal record AiDecisionResult(
        string Decision,
        string SuggestedReply,
        string Reason,
        string? DetectedLanguage,
        decimal? SentimentScore,
        string[]? Topics);
}
