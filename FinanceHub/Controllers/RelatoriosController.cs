using FinanceHub.Models;
using FinanceHub.Models.Exceptions;
using FinanceHub.Models.ViewModels;
using FinanceHub.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinanceHub.Controllers
{
    public class RelatoriosController : Controller
    {
        private readonly RelatorioService _relatorioService;
        private readonly RelatorioArquivoService _relatorioArquivoService;

        public RelatoriosController(
            RelatorioService relatorioService,
            RelatorioArquivoService relatorioArquivoService)
        {
            _relatorioService = relatorioService;
            _relatorioArquivoService = relatorioArquivoService;
        }

        public async Task<IActionResult> Index()
        {
            return View(await CriarViewModelAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(6 * 1024 * 1024)]
        public async Task<IActionResult> Importar(
            IFormFile? arquivo,
            CancellationToken cancellationToken)
        {
            if (arquivo == null || arquivo.Length == 0)
            {
                ModelState.AddModelError("arquivo", "Selecione um arquivo CSV para importar.");
                return View(nameof(Index), await CriarViewModelAsync());
            }

            if (arquivo.Length > 5 * 1024 * 1024)
            {
                ModelState.AddModelError("arquivo", "O arquivo deve ter no maximo 5 MB.");
                return View(nameof(Index), await CriarViewModelAsync());
            }

            if (!Path.GetExtension(arquivo.FileName).Equals(".csv", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("arquivo", "Formato nao suportado. Envie um arquivo .csv.");
                return View(nameof(Index), await CriarViewModelAsync());
            }

            try
            {
                await using var stream = arquivo.OpenReadStream();
                var importacao = await _relatorioArquivoService.AnalisarCsvAsync(
                    stream,
                    Path.GetFileName(arquivo.FileName),
                    cancellationToken);

                return View(nameof(Index), await CriarViewModelAsync(importacao));
            }
            catch (RegraNegocioException ex)
            {
                ModelState.AddModelError("arquivo", ex.Message);
                return View(nameof(Index), await CriarViewModelAsync());
            }
            catch (IOException)
            {
                ModelState.AddModelError("arquivo", "Nao foi possivel ler o arquivo enviado.");
                return View(nameof(Index), await CriarViewModelAsync());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Exportar()
        {
            var conteudo = await _relatorioArquivoService.ExportarTransacoesAsync();
            var nomeArquivo = $"relatorio-financehub-{DateTime.Now:yyyyMMdd-HHmmss}.csv";
            return File(conteudo, "text/csv; charset=utf-8", nomeArquivo);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var relatorio = await _relatorioService.FindByIdAsync(id.Value);
            if (relatorio == null) return NotFound();
            return View(relatorio);
        }

        public IActionResult Create() => View(new Relatorio { GeradoEm = DateTime.Now });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Relatorio relatorio)
        {
            if (!ModelState.IsValid) return View(relatorio);
            try
            {
                await _relatorioService.InsertAsync(relatorio);
                return RedirectToAction(nameof(Index));
            }
            catch (RegraNegocioException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(relatorio);
            }
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var relatorio = await _relatorioService.FindByIdAsync(id.Value);
            if (relatorio == null) return NotFound();
            return View(relatorio);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Relatorio relatorio)
        {
            if (id != relatorio.Id) return NotFound();
            if (!ModelState.IsValid) return View(relatorio);

            try
            {
                await _relatorioService.Update(relatorio);
                return RedirectToAction(nameof(Index));
            }
            catch (RegraNegocioException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(relatorio);
            }
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var relatorio = await _relatorioService.FindByIdAsync(id.Value);
            if (relatorio == null) return NotFound();
            return View(relatorio);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _relatorioService.RemoveAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (RegraNegocioException ex)
            {
                TempData["Erro"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        private async Task<RelatoriosIndexViewModel> CriarViewModelAsync(
            RelatorioImportadoViewModel? importacao = null)
        {
            return new RelatoriosIndexViewModel
            {
                Relatorios = await _relatorioService.FindAllAsync(),
                Importacao = importacao
            };
        }
    }
}
