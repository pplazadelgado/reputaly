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

        public ReviewDecisionEngine(
            AppDbContext db,
            IConfiguration config,
            IHttpClientFactory httpClientFactory,
            ILogger<ReviewDecisionEngine> logger)
        {
            _db = db;
            _config = config;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task ProcessReviewAsync(Guid reviewId)
        {
            // Cargamos la reseña con su tenant y settings
            var review = await _db.Reviews
                .IgnoreQueryFilters()
                .Include(r => r.Location)
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
            }

            // -------------------------------------------------------
            // PASO 1: Reglas duras (sin IA)
            // -------------------------------------------------------
            var hardRuleEscalation = CheckHardRules(review, settings);

            if(hardRuleEscalation is not null)
            {
                review.AiDecision = "escalate";
                review.AiDecisionReason = hardRuleEscalation;
                review.Status = "escalate";
                review.EscalatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();

                _logger.LogInformation(
                    "Review {ReviewId} escalada por regla dura: {Reason}",
                    reviewId, hardRuleEscalation);
                return;
            }

            // -------------------------------------------------------
            // PASO 2: Llamada a Claude API
            // -------------------------------------------------------
            var aiResult = await CallClaudeAsync(review, settings);

            if(aiResult is null)
            {
                _logger.LogError("Claude no devolvio respuesta para review {ReviewId}", reviewId);
                return;
            }

            review.AiSuggestedReply = aiResult.SuggestedReply;
            review.AiDecision = aiResult.Decision;
            review.AiDecisionReason = aiResult.Reason;

            if(aiResult.Decision == "auto_reply")
            {
                // En Fase 2 guardamos la respuesta sugerida
                // La publicacion real en Google se implementa cuando llegue la aprobacion de la API
                review.FinalReply = aiResult.SuggestedReply;
                review.Status = "auto_replied";
                review.RepliedAt = DateTime.UtcNow;

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
            Review review, TenantSettings settings)
        {
            var apiKey = _config["Anthropic:ApiKey"]!;
            var model = _config["Anthropic:Model"] ?? "claude-sonnet-4-20250514";

            var prompt = BuildPrompt(review, settings);

            var requesBody = new
            {
                model,
                max_tokens = 1024,
                messages = new[]
                {
                    new{role = "user", content = prompt}
                }
            };

            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
            httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

            var json = JsonSerializer.Serialize(requesBody);
            var content = new StringContent(json,Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(
                 "https://api.anthropic.com/v1/messages", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error de Claude API: {Error}", error);
                return null;
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            return ParseClaudeResponse(responseJson);
        }

        // -------------------------------------------------------
        // Prompt estructurado para Claude
        // -------------------------------------------------------
        // -------------------------------------------------------
        private string BuildPrompt(Review review, TenantSettings settings)
        {
            var reviewText = string.IsNullOrWhiteSpace(review.Content)
                ? "(El cliente no dejo texto, solo valoracion de estrellas"
                :review.Content;

            return $$"""
            Eres el asistente de gestión de reseñas de Google de un negocio.

            INFORMACIÓN DEL NEGOCIO:
            {{settings.AiPersonality}}

            RESEÑA A ANALIZAR:
            - Autor: {{review.AuthorName}}
            - Valoración: {{review.Rating}}/5 estrellas
            - Texto: {{reviewText}}

            INSTRUCCIONES:
            Analiza esta reseña y decide si responder automáticamente o escalarla
            a un humano. Escala si detectas: situaciones legales, quejas graves
            que requieren investigación interna, o lenguaje que requiere tacto
            especial más allá de una respuesta estándar.

            Responde ÚNICAMENTE con este JSON, sin texto adicional:
            {
              "decision": "auto_reply" | "escalate",
              "suggestedReply": "texto de la respuesta (siempre, incluso si escalas)",
              "reason": "explicación breve de la decisión"
            }
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
        string Reason);
}
