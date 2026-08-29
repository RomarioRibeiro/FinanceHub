using FinanceHub.Models.Options;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace FinanceHub.Services
{
    public class SmtpEmailMetaSender : IEmailMetaSender
    {
        private readonly EmailOptions _options;

        public SmtpEmailMetaSender(IOptions<EmailOptions> options)
        {
            _options = options.Value;
        }

        public async Task EnviarAsync(
            string destinatario,
            string assunto,
            string mensagem,
            CancellationToken cancellationToken = default)
        {
            if (!_options.EstaConfigurado())
            {
                throw new InvalidOperationException(
                    "O envio de e-mail nao esta configurado no servidor.");
            }

            using var email = new MailMessage
            {
                From = new MailAddress(_options.FromAddress, _options.FromName),
                Subject = assunto,
                Body = mensagem,
                IsBodyHtml = false
            };
            email.To.Add(destinatario);

            using var smtp = new SmtpClient(_options.Host, _options.Port)
            {
                EnableSsl = _options.UseSsl,
                UseDefaultCredentials = false
            };

            if (!string.IsNullOrWhiteSpace(_options.UserName))
            {
                smtp.Credentials = new NetworkCredential(
                    _options.UserName,
                    _options.Password);
            }

            await smtp.SendMailAsync(email, cancellationToken);
        }
    }
}
