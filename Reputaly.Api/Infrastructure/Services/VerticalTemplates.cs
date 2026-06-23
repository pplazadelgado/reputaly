namespace Reputaly.API.Infrastructure.Services;

public record VerticalTemplate(
    string[] HardConstraints,
    string[] SuggestedEscalateKeywords);

public static class VerticalTemplates
{
    // ───────────────────────────────────────────────
    // Plantillas por categoría regulatoria
    // Varios sectores del selector comparten la misma
    // ───────────────────────────────────────────────

    private static readonly VerticalTemplate Health = new(
        HardConstraints: new[]
        {
            "NUNCA confirmes que el autor de esta reseña es o fue paciente.",
            "NUNCA menciones tratamientos, medicamentos o diagnósticos.",
            "NUNCA reveles si la persona acudió o no al centro.",
            "Si la reseña menciona datos de salud, responde de forma genérica sin confirmar ni desmentir nada."
        },
        SuggestedEscalateKeywords: new[]
        {
            "negligencia", "denuncia", "abogado", "muerte",
            "urgencias", "mala praxis", "demanda", "inspección sanitaria"
        });

    private static readonly VerticalTemplate Legal = new(
        HardConstraints: new[]
        {
            "NUNCA confirmes que el autor de esta reseña es o fue cliente del despacho.",
            "NUNCA reveles detalles de ningún caso o expediente.",
            "NUNCA admitas responsabilidad profesional públicamente.",
            "NUNCA comentes el resultado de un caso concreto."
        },
        SuggestedEscalateKeywords: new[]
        {
            "negligencia", "denuncia", "colegio de abogados",
            "mala praxis", "estafa", "secreto profesional"
        });

    private static readonly VerticalTemplate Financial = new(
        HardConstraints: new[]
        {
            "NUNCA prometas rentabilidades ni resultados económicos concretos.",
            "NUNCA asesores sobre productos financieros en respuestas públicas.",
            "NUNCA reveles datos económicos de clientes.",
            "NUNCA comentes condiciones concretas de pólizas, préstamos o contratos."
        },
        SuggestedEscalateKeywords: new[]
        {
            "estafa", "denuncia", "CNMV", "banco de españa",
            "reclamación", "pérdidas", "fraude"
        });

    private static readonly VerticalTemplate Education = new(
        HardConstraints: new[]
        {
            "NUNCA confirmes que un menor está matriculado o asiste al centro.",
            "NUNCA menciones el nombre de ningún menor.",
            "NUNCA reveles información académica o conductual.",
            "Si la reseña menciona menores, responde sin referirte a ellos directamente."
        },
        SuggestedEscalateKeywords: new[]
        {
            "acoso", "bullying", "denuncia", "menor",
            "negligencia", "inspección educativa"
        });

    // ───────────────────────────────────────────────
    // Mapeo sector → plantilla
    // Muchos selectores, pocas plantillas por detrás
    // Los sectores no regulados devuelven null (sin HardConstraints)
    // ───────────────────────────────────────────────

    private static readonly Dictionary<string, VerticalTemplate> SectorMap = new()
    {
        // Salud (RGPD art. 9, Ley 41/2002, LOPD-GDD)
        ["clinic"] = Health,
        ["dental"] = Health,
        ["aesthetics"] = Health,
        ["veterinary"] = Health,
        ["physio"] = Health,
        ["psychology"] = Health,
        ["pharmacy"] = Health,

        // Legal (Estatuto de la Abogacía, secreto profesional)
        ["legal"] = Legal,

        // Financiero (MiFID II, DGSFP)
        ["finance"] = Financial,

        // Educación con menores (RGPD art. 8, LOPIVI)
        ["school"] = Education,
    };

    /// <summary>
    /// Devuelve la plantilla del sector, o null si no es regulado / no existe.
    /// Null = comportamiento genérico sin restricciones especiales.
    /// </summary>
    public static VerticalTemplate? Get(string? vertical)
    {
        if (string.IsNullOrEmpty(vertical)) return null;
        return SectorMap.TryGetValue(vertical, out var template) ? template : null;
    }
}
