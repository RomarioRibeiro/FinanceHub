using FinanceHub.Data;
using FinanceHub.Models;
using FinanceHub.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace FinanceHub.Services
{
    public class MetaLembreteProcessorService
    {
        private const int MaximoTentativasEmail = 3;
        private readonly FinanceHubContext _context;
        private readonly IEmailMetaSender _emailSender;
        private readonly ILogger<MetaLembreteProcessorService> _logger;

        public MetaLembreteProcessorService(
            FinanceHubContext context,
            IEmailMetaSender emailSender,
            ILogger<MetaLembreteProcessorService> logger)
        {
            _context = context;
            _emailSender = emailSender;
            _logger = logger;
        }

        public async Task<int> ProcessarPendentesAsync(
            CancellationToken cancellationToken = default)
        {
            var totalProcessado = await ProcessarEmailsPendentesAsync(cancellationToken);
            var agora = DateTime.Now;
            var metas = await _context.Meta
                .Include(meta => meta.Usuario)
                .Where(meta => meta.Ativa
                    && meta.LembreteAtivo
                    && meta.ProximoLembreteEm.HasValue
                    && meta.ProximoLembreteEm.Value <= agora)
                .ToListAsync(cancellationToken);

            foreach (var meta in metas)
            {
                if (meta.EstaConcluida() || meta.DataFim < agora)
                {
                    meta.LembreteAtivo = false;
                    meta.ProximoLembreteEm = null;
                    continue;
                }

                var dataAgendada = meta.ProximoLembreteEm!.Value;
                var existente = await _context.NotificacaoMeta.FirstOrDefaultAsync(
                    item => item.MetaId == meta.Id
                        && item.DataAgendada == dataAgendada,
                    cancellationToken);

                if (existente != null)
                {
                    if (existente.ProcessadaEm.HasValue)
                    {
                        AvancarAgendamento(meta, dataAgendada, agora);
                    }

                    continue;
                }

                var notificacao = new NotificacaoMeta
                {
                    UsuarioId = meta.UsuarioId,
                    MetaId = meta.Id,
                    Canal = meta.CanalLembrete!.Value,
                    Mensagem = meta.ObterMensagemLembrete(),
                    DataAgendada = dataAgendada,
                    CriadaEm = agora
                };

                if (notificacao.Canal == CanalLembreteMeta.SISTEMA)
                {
                    notificacao.Tentativas = 1;
                    notificacao.ProcessadaEm = agora;
                    notificacao.EnviadaEm = agora;
                    AvancarAgendamento(meta, dataAgendada, agora);
                    totalProcessado++;
                }

                _context.NotificacaoMeta.Add(notificacao);
                await _context.SaveChangesAsync(cancellationToken);
            }

            totalProcessado += await ProcessarEmailsPendentesAsync(cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return totalProcessado;
        }

        private async Task<int> ProcessarEmailsPendentesAsync(
            CancellationToken cancellationToken)
        {
            var pendentes = await _context.NotificacaoMeta
                .Include(item => item.Usuario)
                .Include(item => item.Meta)
                .Where(item => item.Canal == CanalLembreteMeta.EMAIL
                    && !item.ProcessadaEm.HasValue
                    && item.Tentativas < MaximoTentativasEmail)
                .ToListAsync(cancellationToken);
            var totalProcessado = 0;

            foreach (var notificacao in pendentes)
            {
                notificacao.Tentativas++;

                try
                {
                    await _emailSender.EnviarAsync(
                        notificacao.Usuario.Email,
                        $"FinanceHub - Meta {notificacao.Meta.Nome}",
                        notificacao.Mensagem,
                        cancellationToken);

                    var agora = DateTime.Now;
                    notificacao.EnviadaEm = agora;
                    notificacao.ProcessadaEm = agora;
                    notificacao.UltimoErro = null;
                    AvancarAgendamento(
                        notificacao.Meta,
                        notificacao.DataAgendada,
                        agora);
                    totalProcessado++;
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    notificacao.UltimoErro = LimitarErro(ex.Message);
                    _logger.LogWarning(
                        ex,
                        "Falha ao enviar lembrete da meta {MetaId}. Tentativa {Tentativa}.",
                        notificacao.MetaId,
                        notificacao.Tentativas);

                    if (notificacao.Tentativas >= MaximoTentativasEmail)
                    {
                        var agora = DateTime.Now;
                        notificacao.ProcessadaEm = agora;
                        AvancarAgendamento(
                            notificacao.Meta,
                            notificacao.DataAgendada,
                            agora);
                    }
                }

                await _context.SaveChangesAsync(cancellationToken);
            }

            return totalProcessado;
        }

        private static void AvancarAgendamento(
            Meta meta,
            DateTime dataAgendada,
            DateTime agora)
        {
            var proxima = meta.CalcularProximoLembrete(dataAgendada);
            while (proxima <= agora)
            {
                proxima = meta.CalcularProximoLembrete(proxima);
            }

            if (proxima > meta.DataFim)
            {
                meta.LembreteAtivo = false;
                meta.ProximoLembreteEm = null;
                return;
            }

            meta.ProximoLembreteEm = proxima;
        }

        private static string LimitarErro(string erro)
        {
            return erro.Length <= 500 ? erro : erro[..500];
        }
    }
}
