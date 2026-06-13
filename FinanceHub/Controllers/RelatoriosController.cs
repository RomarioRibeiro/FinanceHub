using FinanceHub.Models;
using FinanceHub.Models.Exceptions;
using FinanceHub.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinanceHub.Controllers
{
    public class RelatoriosController : Controller
    {
        private readonly RelatorioService _relatorioService;

        public RelatoriosController(RelatorioService relatorioService)
        {
            _relatorioService = relatorioService;
        }

        public async Task<IActionResult> Index() => View(await _relatorioService.FindAllAsync());

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
    }
}
