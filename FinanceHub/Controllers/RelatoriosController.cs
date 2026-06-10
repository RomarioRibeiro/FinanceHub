using FinanceHub.Models;
using FinanceHub.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
            if (relatorio.GeradoEm == default) relatorio.GeradoEm = DateTime.Now;
            await _relatorioService.InsertAsync(relatorio);
            return RedirectToAction(nameof(Index));
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
            catch (DbUpdateConcurrencyException)
            {
                throw new Exception();
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
            await _relatorioService.RemoveAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
