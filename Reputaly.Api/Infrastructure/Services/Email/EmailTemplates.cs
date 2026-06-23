using Reputaly.API.Domain;
using static Reputaly.API.Infrastructure.Services.Email.ResendEmailService;

namespace Reputaly.API.Infrastructure.Services.Email
{
    public static class EmailTemplates
    {
        // Email escalado. reviewUrl es el enlace al frontend (/revies/{id})
        public static string Escalation(
            Review review, string businessName, string reviewUrl, bool autoRepliedByTimeout)
        {
            var stars = new string('★', review.Rating) + new string('☆', 5 - review.Rating);
            var excerpt = string.IsNullOrWhiteSpace(review.Content)
                ? "(El cliente no dejó texto, solo valoración)"
                : Truncate(review.Content, 280);

            // Cabecera y mensaje varían según sea aviso normal o auto-respuesta por timeout.
            var (heading, intro, buttonLabel) = autoRepliedByTimeout
                ? ("La IA ha respondido automáticamente",
                   "Esta reseña llevaba un tiempo escalada sin respuesta, así que la IA ha publicado una respuesta automáticamente. Puedes revisarla:",
                   "Ver la respuesta")
                : ("Una reseña requiere tu atención",
                   "Se ha escalado una nueva reseña que necesita tu revisión:",
                   "Revisar reseña");

            return $$"""
                <!DOCTYPE html>
                <html lang="es">
                <body style="margin:0;padding:0;background:#f1f5f9;font-family:Arial,Helvetica,sans-serif;">
                  <table width="100%" cellpadding="0" cellspacing="0" style="background:#f1f5f9;padding:24px 0;">
                    <tr><td align="center">
                      <table width="560" cellpadding="0" cellspacing="0" style="background:#ffffff;border-radius:12px;overflow:hidden;border:1px solid #e2e8f0;">
                        <tr><td style="background:#0B2545;padding:20px 28px;">
                          <span style="color:#ffffff;font-size:18px;font-weight:bold;">Reputaly</span>
                        </td></tr>
                        <tr><td style="padding:28px;">
                          <h1 style="margin:0 0 8px;font-size:20px;color:#0F172A;">{{heading}}</h1>
                          <p style="margin:0 0 20px;font-size:14px;color:#475569;">{{businessName}}</p>
                          <p style="margin:0 0 16px;font-size:14px;color:#334155;">{{intro}}</p>
                          <table width="100%" cellpadding="0" cellspacing="0" style="background:#f8fafc;border-radius:8px;margin:0 0 20px;">
                            <tr><td style="padding:16px;">
                              <p style="margin:0 0 4px;font-size:14px;color:#0F172A;"><strong>{{review.AuthorName}}</strong></p>
                              <p style="margin:0 0 8px;font-size:16px;color:#f59e0b;">{{stars}}</p>
                              <p style="margin:0 0 8px;font-size:14px;color:#334155;line-height:1.5;">{{excerpt}}</p>
                              <p style="margin:0;font-size:13px;color:#64748b;"><em>Motivo: {{review.AiDecisionReason}}</em></p>
                            </td></tr>
                          </table>
                          <a href="{{reviewUrl}}" style="display:inline-block;background:#0B2545;color:#ffffff;text-decoration:none;padding:12px 24px;border-radius:8px;font-size:14px;font-weight:bold;">{{buttonLabel}}</a>
                        </td></tr>
                        <tr><td style="padding:16px 28px;border-top:1px solid #e2e8f0;">
                          <p style="margin:0;font-size:12px;color:#94a3b8;">Este es un email automático de Reputaly. Puedes ajustar las notificaciones en tu configuración.</p>
                        </td></tr>
                      </table>
                    </td></tr>
                  </table>
                </body>
                </html>
                """;
        }

        public static string WeeklySummary(string businessName, WeeklySummaryStats s)
        {
            return $$"""
        <!DOCTYPE html>
        <html lang="es">
        <body style="margin:0;padding:0;background:#f1f5f9;font-family:Arial,Helvetica,sans-serif;">
          <table width="100%" cellpadding="0" cellspacing="0" style="background:#f1f5f9;padding:24px 0;">
            <tr><td align="center">
              <table width="560" cellpadding="0" cellspacing="0" style="background:#ffffff;border-radius:12px;overflow:hidden;border:1px solid #e2e8f0;">
                <tr><td style="background:#0B2545;padding:20px 28px;">
                  <span style="color:#ffffff;font-size:18px;font-weight:bold;">Reputaly</span>
                </td></tr>
                <tr><td style="padding:28px;">
                  <h1 style="margin:0 0 8px;font-size:20px;color:#0F172A;">Resumen semanal</h1>
                  <p style="margin:0 0 20px;font-size:14px;color:#475569;">{{businessName}} · últimos 7 días</p>

                  <table width="100%" cellpadding="0" cellspacing="0">
                    <tr>
                      <td width="50%" style="padding:8px;">
                        <table width="100%" style="background:#f8fafc;border-radius:8px;"><tr><td style="padding:16px;text-align:center;">
                          <div style="font-size:28px;font-weight:bold;color:#0B2545;">{{s.Total}}</div>
                          <div style="font-size:13px;color:#64748b;">Reseñas esta semana</div>
                        </td></tr></table>
                      </td>
                      <td width="50%" style="padding:8px;">
                        <table width="100%" style="background:#f8fafc;border-radius:8px;"><tr><td style="padding:16px;text-align:center;">
                          <div style="font-size:28px;font-weight:bold;color:#f59e0b;">{{s.AvgRating}} ★</div>
                          <div style="font-size:13px;color:#64748b;">Valoración media</div>
                        </td></tr></table>
                      </td>
                    </tr>
                    <tr>
                      <td width="50%" style="padding:8px;">
                        <table width="100%" style="background:#f8fafc;border-radius:8px;"><tr><td style="padding:16px;text-align:center;">
                          <div style="font-size:28px;font-weight:bold;color:#16a34a;">{{s.AutoReplied}}</div>
                          <div style="font-size:13px;color:#64748b;">Respondidas por IA</div>
                        </td></tr></table>
                      </td>
                      <td width="50%" style="padding:8px;">
                        <table width="100%" style="background:#f8fafc;border-radius:8px;"><tr><td style="padding:16px;text-align:center;">
                          <div style="font-size:28px;font-weight:bold;color:#dc2626;">{{s.Escalated}}</div>
                          <div style="font-size:13px;color:#64748b;">Escaladas</div>
                        </td></tr></table>
                      </td>
                    </tr>
                  </table>

                  {{(s.Pending > 0
                        ? $"<p style=\"margin:20px 0 0;font-size:14px;color:#334155;\">Tienes <strong>{s.Pending}</strong> reseña(s) pendiente(s) sin respuesta.</p>"
                        : "<p style=\"margin:20px 0 0;font-size:14px;color:#16a34a;\">¡Buen trabajo! No tienes reseñas pendientes.</p>")}}
                </td></tr>
                <tr><td style="padding:16px 28px;border-top:1px solid #e2e8f0;">
                  <p style="margin:0;font-size:12px;color:#94a3b8;">Resumen automático de Reputaly. Puedes ajustar las notificaciones en tu configuración.</p>
                </td></tr>
              </table>
            </td></tr>
          </table>
        </body>
        </html>
        """;
        }

        private static string Truncate(string text, int max)
            => text.Length <= max ? text : text.Substring(0, max) + "_";
    }
}
