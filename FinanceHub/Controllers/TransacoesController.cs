using FinanceHub.Models;
using FinanceHub.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinanceHub.Controllers
{
    public class TransacoesController : Controller
    {
        private readonly TransacaoService _transacaoService;

        public TransacoesController(TransacaoService transacaoService)
        {
            _transacaoService = transacaoService;
        }

        public async Task<IActionResult> Index() => View(await _transacaoService.FindAllAsync());

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var transacao = await _transacaoService.FindByIdAsync(id.Value);
            if (transacao == null) return NotFound();
            return View(transacao);
        }

        public IActionResult Create() => View(new Transacao { Data = DateTime.Now });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Transacao transacao)
        {
            if (!ModelState.IsValid) return View(transacao);
            if (transacao.Data == default) transacao.Data = DateTime.Now;
            await _transacaoService.InsertAsync(transacao);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var transacao = await _transacaoService.FindByIdAsync(id.Value);
            if (transacao == null) return NotFound();
            return View(transacao);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Transacao transacao)
        {
            if (id != transacao.Id) return NotFound();
            if (!ModelState.IsValid) return View(transacao);

            try
            {
                await _transacaoService.Update(transacao);
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
            var transacao = await _transacaoService.FindByIdAsync(id.Value);
            if (transacao == null) return NotFound();
            return View(transacao);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _transacaoService.RemoveAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
