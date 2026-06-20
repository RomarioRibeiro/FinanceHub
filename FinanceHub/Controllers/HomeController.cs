using System.Diagnostics;
using FinanceHub.Models;
using FinanceHub.Models.Enums;
using FinanceHub.Models.ViewModels;
using FinanceHub.Repositories;
using FinanceHub.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinanceHub.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IRepository<Conta> _contaRepository;
        private readonly IRepository<Transacao> _transacaoRepository;
        private readonly IRepository<LancamentoRecorrente> _recorrenciaRepository;
        private readonly IRepository<Meta> _metaRepository;
        private readonly UsuarioAtualService _usuarioAtualService;
        private readonly RecorrenciaProcessorService _recorrenciaProcessorService;

        public HomeController(
            ILogger<HomeController> logger,
            IRepository<Conta> contaRepository,
            IRepository<Transacao> transacaoRepository,
            IRepository<LancamentoRecorrente> recorrenciaRepository,
            IRepository<Meta> metaRepository,
            UsuarioAtualService usuarioAtualService,
            RecorrenciaProcessorService recorrenciaProcessorService)
        {
            _logger = logger;
            _contaRepository = contaRepository;
            _transacaoRepository = transacaoRepository;
            _recorrenciaRepository = recorrenciaRepository;
            _metaRepository = metaRepository;
            _usuarioAtualService = usuarioAtualService;
            _recorrenciaProcessorService = recorrenciaProcessorService;
        }

        public async Task<IActionResult> Index()
        {
            var usuarioId = _usuarioAtualService.ObterUsuarioId();
            var hoje = DateTime.Today;
            var inicioMes = new DateTime(hoje.Year, hoje.Month, 1);

            await _recorrenciaProcessorService.GerarPendentesUsuarioAsync(usuarioId, DateTime.Now);

            var contas = await _contaRepository.FindAllAsync(conta => conta.UsuarioId == usuarioId);
            var transacoes = await _transacaoRepository.FindAllAsync(
                transacao => transacao.UsuarioId == usuarioId,
                transacao => transacao.Categoria!,
                transacao => transacao.Conta!);
            var recorrencias = await _recorrenciaRepository.FindAllAsync(
                item => item.UsuarioId == usuarioId && item.Ativo);
            var metas = await _metaRepository.FindAllAsync(meta => meta.UsuarioId == usuarioId);

            var transacoesMes = transacoes
                .Where(transacao => transacao.Data >= inicioMes && transacao.Data <= hoje.AddDays(1).AddTicks(-1))
                .ToList();

            var receitasMes = transacoesMes
                .Where(transacao => transacao.TipoCategoria == TipoCategoria.RECEITA)
                .Sum(transacao => transacao.Valor);

            var despesasMes = transacoesMes
                .Where(transacao => transacao.TipoCategoria == TipoCategoria.DESPESA)
                .Sum(transacao => transacao.Valor);

            var despesasDoAno = transacoes
                .Where(transacao =>
                    transacao.TipoCategoria == TipoCategoria.DESPESA
                    && transacao.Data.Year == hoje.Year)
                .ToList();

            var totalDespesasAno = despesasDoAno.Sum(transacao => transacao.Valor);
            var categoriasComCores = new[]
            {
                "#d95d39",
                "#f0a202",
                "#457b9d",
                "#6a994e",
                "#7b6d8d"
            };

            var despesasPorCategoria = despesasDoAno
                .GroupBy(transacao => transacao.Categoria?.Nome ?? "Sem categoria")
                .Select((grupo, index) => new HomeCategoryExpenseViewModel
                {
                    Categoria = grupo.Key,
                    Valor = grupo.Sum(item => item.Valor),
                    Percentual = totalDespesasAno <= 0 ? 0 : (grupo.Sum(item => item.Valor) / totalDespesasAno) * 100m,
                    Cor = categoriasComCores[index % categoriasComCores.Length]
                })
                .OrderByDescending(item => item.Valor)
                .Take(5)
                .ToList();

            var fluxoMensal = Enumerable.Range(0, 6)
                .Select(offset =>
                {
                    var referencia = inicioMes.AddMonths(-(5 - offset));
                    var fim = referencia.AddMonths(1);
                    var transacoesPeriodo = transacoes
                        .Where(transacao => transacao.Data >= referencia && transacao.Data < fim)
                        .ToList();

                    return new HomeMonthlyFlowViewModel
                    {
                        Mes = referencia.ToString("MMM/yy"),
                        Receitas = transacoesPeriodo
                            .Where(transacao => transacao.TipoCategoria == TipoCategoria.RECEITA)
                            .Sum(transacao => transacao.Valor),
                        Despesas = transacoesPeriodo
                            .Where(transacao => transacao.TipoCategoria == TipoCategoria.DESPESA)
                            .Sum(transacao => transacao.Valor)
                    };
                })
                .ToList();

            var maiorGasto = transacoesMes
                .Where(transacao => transacao.TipoCategoria == TipoCategoria.DESPESA)
                .OrderByDescending(transacao => transacao.Valor)
                .FirstOrDefault();

            var metasAtivas = metas.Where(meta => meta.Ativa).ToList();

            var viewModel = new HomeDashboardViewModel
            {
                SaldoTotal = contas.Where(conta => conta.Ativa).Sum(conta => conta.SaldoAtual),
                ReceitasMes = receitasMes,
                DespesasMes = despesasMes,
                ResultadoMes = receitasMes - despesasMes,
                GastoMedioMes = transacoesMes.Count == 0 ? 0 : despesasMes / Math.Max(1, transacoesMes.Count(transacao => transacao.TipoCategoria == TipoCategoria.DESPESA)),
                LancamentosMes = transacoesMes.Count,
                ContasAtivas = contas.Count(conta => conta.Ativa),
                RecorrenciasAtivas = recorrencias.Count,
                MetasAtivas = metasAtivas.Count,
                ProgressoMetasMedio = metasAtivas.Count == 0 ? 0 : metasAtivas.Average(meta => meta.CalcularProgresso()),
                PeriodoReferencia = inicioMes.ToString("MMMM yyyy"),
                MaiorGastoDescricao = maiorGasto?.Descricao ?? "Sem despesas no periodo",
                MaiorGastoValor = maiorGasto?.Valor ?? 0,
                FluxoMensal = fluxoMensal,
                DespesasPorCategoria = despesasPorCategoria,
                LancamentosRecentes = transacoes
                    .OrderByDescending(transacao => transacao.Data)
                    .Take(8)
                    .Select(transacao => new HomeRecentTransactionViewModel
                    {
                        Descricao = transacao.Descricao,
                        Categoria = transacao.Categoria?.Nome ?? "Sem categoria",
                        Conta = transacao.Conta?.Nome ?? "Sem conta",
                        Data = transacao.Data,
                        Valor = transacao.Valor,
                        EhReceita = transacao.TipoCategoria == TipoCategoria.RECEITA
                    })
                    .ToList()
            };

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
