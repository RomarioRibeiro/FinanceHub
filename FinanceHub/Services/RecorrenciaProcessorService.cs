using FinanceHub.Data;
using FinanceHub.Models;
using FinanceHub.Models.Enums;
using FinanceHub.Models.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace FinanceHub.Services
{
    public class RecorrenciaProcessorService
    {
        private readonly FinanceHubContext _context;
        private readonly ILogger<RecorrenciaProcessorService> _logger;

        public RecorrenciaProcessorService(
            FinanceHubContext context,
            ILogger<RecorrenciaProcessorService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public Task<int> GerarPendentesAsync(CancellationToken cancellationToken = default)
        {
            return GerarPendentesAsync(DateTime.Now, cancellationToken);
        }

        public async Task<int> GerarPendentesAsync(
            DateTime referencia,
            CancellationToken cancellationToken = default)
        {
            var recorrencias = await _context.LancamentoRecorrente
                .Include(item => item.Categoria)
                .Include(item => item.Conta)
                .Where(item => item.Ativo)
                .ToListAsync(cancellationToken);

            return await ProcessarRecorrenciasAsync(recorrencias, referencia, cancellationToken);
        }

        public async Task<int> GerarPendentesUsuarioAsync(
            int usuarioId,
            DateTime referencia,
            CancellationToken cancellationToken = default)
        {
            var recorrencias = await _context.LancamentoRecorrente
                .Include(item => item.Categoria)
                .Include(item => item.Conta)
                .Where(item => item.Ativo && item.UsuarioId == usuarioId)
                .ToListAsync(cancellationToken);

            return await ProcessarRecorrenciasAsync(recorrencias, referencia, cancellationToken);
        }

        public async Task GerarAgoraAsync(
            int recorrenciaId,
            int usuarioId,
            DateTime referencia,
            CancellationToken cancellationToken = default)
        {
            var recorrencia = await _context.LancamentoRecorrente
                .Include(item => item.Categoria)
                .Include(item => item.Conta)
                .FirstOrDefaultAsync(
                    item => item.Id == recorrenciaId && item.UsuarioId == usuarioId,
                    cancellationToken);

            if (recorrencia == null)
            {
                throw new RegraNegocioException("Lancamento recorrente nao encontrado.");
            }

            if (!recorrencia.Ativo)
            {
                throw new RegraNegocioException("Nao e permitido gerar uma recorrencia inativa.");
            }

            ValidarRecorrenciaProcessavel(recorrencia);

            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var novaTransacao = recorrencia.GerarLancamento(recorrencia.ContaId!.Value, referencia);
                recorrencia.Conta!.AtualizarSaldo(novaTransacao.ObterImpactoNoSaldo());
                recorrencia.UltimaGeracaoEm = referencia;

                _context.Transacao.Add(novaTransacao);
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        private async Task<int> ProcessarRecorrenciasAsync(
            List<LancamentoRecorrente> recorrencias,
            DateTime referencia,
            CancellationToken cancellationToken)
        {
            if (recorrencias.Count == 0)
            {
                return 0;
            }

            var totalGerado = 0;

            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                foreach (var recorrencia in recorrencias)
                {
                    if (!PodeProcessar(recorrencia))
                    {
                        continue;
                    }

                    foreach (var dataGeracao in ObterGeracoesPendentes(recorrencia, referencia))
                    {
                        var novaTransacao = recorrencia.GerarLancamento(recorrencia.ContaId!.Value, dataGeracao);
                        recorrencia.Conta!.AtualizarSaldo(novaTransacao.ObterImpactoNoSaldo());
                        recorrencia.UltimaGeracaoEm = dataGeracao;
                        _context.Transacao.Add(novaTransacao);
                        totalGerado++;
                    }
                }

                if (totalGerado > 0)
                {
                    await _context.SaveChangesAsync(cancellationToken);
                }

                await transaction.CommitAsync(cancellationToken);
                return totalGerado;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        private bool PodeProcessar(LancamentoRecorrente recorrencia)
        {
            try
            {
                ValidarRecorrenciaProcessavel(recorrencia);
                return true;
            }
            catch (RegraNegocioException ex)
            {
                _logger.LogWarning(
                    "Recorrencia {RecorrenciaId} ignorada durante o processamento: {Mensagem}",
                    recorrencia.Id,
                    ex.Message);
                return false;
            }
        }

        private static void ValidarRecorrenciaProcessavel(LancamentoRecorrente recorrencia)
        {
            if (recorrencia.Conta == null)
            {
                throw new RegraNegocioException("A recorrencia nao possui conta vinculada.");
            }

            if (!recorrencia.ContaId.HasValue || recorrencia.ContaId.Value <= 0)
            {
                throw new RegraNegocioException("A recorrencia nao possui conta vinculada.");
            }

            if (!recorrencia.Conta.Ativa)
            {
                throw new RegraNegocioException("A conta vinculada a recorrencia esta inativa.");
            }

            if (recorrencia.Categoria == null)
            {
                throw new RegraNegocioException("A recorrencia nao possui categoria vinculada.");
            }

            if (recorrencia.Categoria.UsuarioId != recorrencia.UsuarioId)
            {
                throw new RegraNegocioException(
                    "A categoria vinculada a recorrencia pertence a outro usuario.");
            }
        }

        private static List<DateTime> ObterGeracoesPendentes(
            LancamentoRecorrente recorrencia,
            DateTime referencia)
        {
            var geracoes = new List<DateTime>();
            var proximaGeracao = recorrencia.UltimaGeracaoEm.HasValue
                ? CalcularProximaOcorrencia(recorrencia, recorrencia.UltimaGeracaoEm.Value)
                : CalcularPrimeiraOcorrencia(recorrencia);

            while (proximaGeracao <= referencia)
            {
                geracoes.Add(proximaGeracao);
                proximaGeracao = CalcularProximaOcorrencia(recorrencia, proximaGeracao);
            }

            return geracoes;
        }

        private static DateTime CalcularPrimeiraOcorrencia(LancamentoRecorrente recorrencia)
        {
            return recorrencia.Frequencia switch
            {
                FrequenciaRecorrencia.DIARIA => recorrencia.DataInicial,
                FrequenciaRecorrencia.SEMANAL => CalcularPrimeiraOcorrenciaSemanal(recorrencia),
                FrequenciaRecorrencia.MENSAL => CalcularPrimeiraOcorrenciaMensal(recorrencia),
                FrequenciaRecorrencia.ANUAL => CalcularPrimeiraOcorrenciaAnual(recorrencia),
                _ => recorrencia.DataInicial
            };
        }

        private static DateTime CalcularProximaOcorrencia(
            LancamentoRecorrente recorrencia,
            DateTime ultimaGeracao)
        {
            return recorrencia.Frequencia switch
            {
                FrequenciaRecorrencia.DIARIA => ultimaGeracao.AddDays(1),
                FrequenciaRecorrencia.SEMANAL => ultimaGeracao.AddDays(7),
                FrequenciaRecorrencia.MENSAL => CriarOcorrenciaMensal(
                    ultimaGeracao.Year,
                    ultimaGeracao.Month,
                    recorrencia.DiaReferencia,
                    recorrencia.DataInicial.TimeOfDay).AddMonths(1),
                FrequenciaRecorrencia.ANUAL => CriarOcorrenciaAnual(
                    ultimaGeracao.Year + 1,
                    recorrencia.DiaReferencia,
                    recorrencia.DataInicial.TimeOfDay),
                _ => ultimaGeracao.AddDays(1)
            };
        }

        private static DateTime CalcularPrimeiraOcorrenciaSemanal(LancamentoRecorrente recorrencia)
        {
            var dataBase = recorrencia.DataInicial;
            var diaSemanaDesejado = recorrencia.DiaReferencia % 7;
            var diasAteOcorrencia = (diaSemanaDesejado - (int)dataBase.DayOfWeek + 7) % 7;
            var proximaData = dataBase.Date.AddDays(diasAteOcorrencia).Add(dataBase.TimeOfDay);

            return proximaData < dataBase ? proximaData.AddDays(7) : proximaData;
        }

        private static DateTime CalcularPrimeiraOcorrenciaMensal(LancamentoRecorrente recorrencia)
        {
            var dataBase = recorrencia.DataInicial;
            var primeira = CriarOcorrenciaMensal(
                dataBase.Year,
                dataBase.Month,
                recorrencia.DiaReferencia,
                dataBase.TimeOfDay);

            return primeira < dataBase ? primeira.AddMonths(1) : primeira;
        }

        private static DateTime CalcularPrimeiraOcorrenciaAnual(LancamentoRecorrente recorrencia)
        {
            var dataBase = recorrencia.DataInicial;
            var primeira = CriarOcorrenciaAnual(
                dataBase.Year,
                recorrencia.DiaReferencia,
                dataBase.TimeOfDay);

            return primeira < dataBase
                ? CriarOcorrenciaAnual(dataBase.Year + 1, recorrencia.DiaReferencia, dataBase.TimeOfDay)
                : primeira;
        }

        private static DateTime CriarOcorrenciaMensal(
            int ano,
            int mes,
            int diaReferencia,
            TimeSpan horario)
        {
            var ultimoDiaMes = DateTime.DaysInMonth(ano, mes);
            var dia = Math.Min(diaReferencia, ultimoDiaMes);
            return new DateTime(ano, mes, dia).Add(horario);
        }

        private static DateTime CriarOcorrenciaAnual(
            int ano,
            int diaReferencia,
            TimeSpan horario)
        {
            var ultimoDiaAno = DateTime.IsLeapYear(ano) ? 366 : 365;
            var diaDoAno = Math.Min(diaReferencia, ultimoDiaAno);
            return new DateTime(ano, 1, 1).AddDays(diaDoAno - 1).Add(horario);
        }
    }
}
