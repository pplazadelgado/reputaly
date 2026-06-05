namespace Reputaly.API.Infrastructure.Services;

    // Configuración de sistema por sector (vertical).
    // NO se guarda en BD ni es editable por el tenant: son reglas de negocio fijas.
    public record VerticalTemplate(
        string[] HardConstraints,
        string[] SuggestedEscalateKeywords);


public static class VerticalTemplates
{
    private static readonly Dictionary<string, VerticalTemplate> Templates = new()
    {
        ["clinic"] = new VerticalTemplate(
        HardConstraints: new[]
        {
                "NUNCA confirmes que el autor de esta reseña es o fue paciente.",
                "NUNCA menciones tratamientos, medicamentos o diagnósticos.",
                "NUNCA reveles si la persona acudió o no al centro.",
                "Si la reseña menciona datos de salud, responde de forma genérica."
        },
        SuggestedEscalateKeywords: new[]
        {
                "negligencia", "denuncia", "abogado", "muerte", "urgencias", "mala praxis"
        }),

        ["garage"] = new VerticalTemplate(
        HardConstraints: Array.Empty<string>(),
        SuggestedEscalateKeywords: new[]
        {
                "accidente", "seguro", "estafa", "ITV", "frenos", "avería grave"
        }),

        ["franchise"] = new VerticalTemplate(
        HardConstraints: Array.Empty<string>(),
        SuggestedEscalateKeywords: new[]
        {
                "franquicia", "central", "marca", "demanda", "contrato"
        }),
    };

    // Devuelve el template del vertical, o null si no existe o es null
    public static VerticalTemplate? Get(string? vertical)
    {
        if (string.IsNullOrEmpty(vertical)) return null;
        return Templates.TryGetValue(vertical, out var template) ? template : null;
    }
}

