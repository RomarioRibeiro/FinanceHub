namespace FinanceHub.Services
{
    public interface IEmailMetaSender
    {
        Task EnviarAsync(
            string destinatario,
            string assunto,
            string mensagem,
            CancellationToken cancellationToken = default);
    }
}
