using System.ComponentModel.DataAnnotations;

namespace Reputaly.API.Domain
{
    public class AiConfig
    {
        public AiConfigEntry Default { get; set; } = new AiConfigEntry
        {
            Instructions = "Responde en tono profesional y cercano",
            Tone = "profesional",
            MaxLength = 300
        };

        public Dictionary<string, AiConfigEntry> ByRating { get; set; } = new()
        {
            ["1"] = new AiConfigEntry { Instructions = "Muestra empatía, ofrece solución privada.", Tone = "empathetic" },
            ["2"] = new AiConfigEntry { Instructions = "Reconoce el problema, invita a contacto.", Tone = "empathetic" },
            ["3"] = new AiConfigEntry { Instructions = "Agradece la valoración, destaca positivos.", Tone = "neutral" },
            ["4"] = new AiConfigEntry { Instructions = "Muestra agradecimiento genuino.", Tone = "warm" },
            ["5"] = new AiConfigEntry { Instructions = "Agradece con entusiasmo, refuerza marca.", Tone = "enthusiastic" }
        };
    }

    public class AiConfigEntry
    {
        public string Instructions { get; set; } = string.Empty;
        public string Tone { get; set; } = string.Empty;
        public int? MaxLength {  get; set; }
    }
}
