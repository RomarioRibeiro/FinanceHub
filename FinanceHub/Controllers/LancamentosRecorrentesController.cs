using FinanceHub.Models;
using FinanceHub.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinanceHub.Controllers
{
    public class LancamentosRecorrentesController : Controller
    {
        private readonly LancamentoRecorrenteService _service;

        public LancamentosRecorrentesController(LancamentoRecorrenteService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index() => View(await _service.FindAllAsync());

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var item = await _service.FindByIdAsync(id.Value);
            if (item == null) return NotFound();
            return View(item);
        }

        public IActionResult Create() => View(new LancamentoRecorrente { DataInicial = DateTime.Now, Ativo = true });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LancamentoRecorrente lancamentoRecorrente)
        {
            if (!ModelState.IsValid) return View(lancamentoRecorrente);
            if (lancamentoRecorrente.DataInicial == default) lancamentoRecorrente.DataInicial = DateTime.Now;
            await _service.InsertAsync(lancamentoRecorrente);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var item = await _service.FindByIdAsync(id.Value);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, LancamentoRecorrente lancamentoRecorrente)
        {
            if (id != lancamentoRecorrente.Id) return NotFound();
            if (!ModelState.IsValid) return View(lancamentoRecorrente);

            try
            {
                await _service.Update(lancamentoRecorrente);
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
            var item = await _service.FindByIdAsync(id.Value);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _service.RemoveAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
