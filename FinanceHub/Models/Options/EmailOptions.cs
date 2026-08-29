namespace FinanceHub.Models.Options
{
    public class EmailOptions
    {
        public const string SectionName = "Email";

        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
        public bool UseSsl { get; set; } = true;
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FromAddress { get; set; } = string.Empty;
        public string FromName { get; set; } = "FinanceHub";

        public bool EstaConfigurado()
        {
            return !string.IsNullOrWhiteSpace(Host)
                && Port > 0
                && !string.IsNullOrWhiteSpace(FromAddress);
        }
    }
}
