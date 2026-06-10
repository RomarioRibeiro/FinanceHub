using FinanceHub.Models;
using FinanceHub.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinanceHub.Controllers
{
    public class ContasController : Controller
    {
        private readonly ContaService _contaService;

        public ContasController(ContaService contaService)
        {
            _contaService = contaService;
        }

        public async Task<IActionResult> Index() => View(await _contaService.FindAllAsync());

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var conta = await _contaService.FindByIdAsync(id.Value);
            if (conta == null) return NotFound();
            return View(conta);
        }

        public IActionResult Create() => View(new Conta { Ativa = true });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Conta conta)
        {
            if (!ModelState.IsValid) return View(conta);
            conta.Ativa = true;
            await _contaService.InsertAsync(conta);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var conta = await _contaService.FindByIdAsync(id.Value);
            if (conta == null) return NotFound();
            return View(conta);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Conta conta)
        {
            if (id != conta.Id) return NotFound();
            if (!ModelState.IsValid) return View(conta);

            try
            {
                await _contaService.Update(conta);
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
            var conta = await _contaService.FindByIdAsync(id.Value);
            if (conta == null) return NotFound();
            return View(conta);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _contaService.RemoveAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
