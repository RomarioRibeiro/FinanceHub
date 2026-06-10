using FinanceHub.Models;
using FinanceHub.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinanceHub.Controllers
{
    public class SaldosController : Controller
    {
        private readonly SaldoService _saldoService;

        public SaldosController(SaldoService saldoService)
        {
            _saldoService = saldoService;
        }

        public async Task<IActionResult> Index() => View(await _saldoService.FindAllAsync());

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var saldo = await _saldoService.FindByIdAsync(id.Value);
            if (saldo == null) return NotFound();
            return View(saldo);
        }

        public IActionResult Create() => View(new Saldo { Data = DateTime.Now });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Saldo saldo)
        {
            if (!ModelState.IsValid) return View(saldo);
            if (saldo.Data == default) saldo.Data = DateTime.Now;
            await _saldoService.InsertAsync(saldo);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var saldo = await _saldoService.FindByIdAsync(id.Value);
            if (saldo == null) return NotFound();
            return View(saldo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Saldo saldo)
        {
            if (id != saldo.Id) return NotFound();
            if (!ModelState.IsValid) return View(saldo);

            try
            {
                await _saldoService.Update(saldo);
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
            var saldo = await _saldoService.FindByIdAsync(id.Value);
            if (saldo == null) return NotFound();
            return View(saldo);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _saldoService.RemoveAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
