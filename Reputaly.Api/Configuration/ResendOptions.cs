namespace Reputaly.API.Configuration
{
    public class ResendOptions
    {
        public const string SectionName = "Resend";

        /// <summary>Api key de resend (re_xxx). SECRETO, va en user-sercrets</summary>
        public string ApiKey { get; set; } = string.Empty;

        ///<summary>Email remitente. Desarrollo: onboarding@resend.dev
        ///Produccion: notificaciones@reputaly.com</summary>
        public string FromEmail { get; set; } = string.Empty;

        ///<summary>Nombre visible del remitante: Reputaly</summary>
        public string FromName { get; set; } = "Reputaly";
    }
}
