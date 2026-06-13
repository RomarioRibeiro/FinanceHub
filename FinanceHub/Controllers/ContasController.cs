using FinanceHub.Models;
using FinanceHub.Models.Exceptions;
using FinanceHub.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinanceHub.Controllers
{
    public class ContasController : Controller
    {
        private readonly ContaService _contaService;

        public ContasController(ContaService contaService)
        {
            _contaService = contaService;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _contaService.FindAllAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var conta = await _contaService.FindByIdAsync(id.Value);
            return conta == null ? NotFound() : View(conta);
        }

        public IActionResult Create()
        {
            return View(new Conta { Ativa = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Conta conta)
        {
            if (!ModelState.IsValid)
            {
                return View(conta);
            }

            try
            {
                await _contaService.InsertAsync(conta);
                return RedirectToAction(nameof(Index));
            }
            catch (RegraNegocioException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(conta);
            }
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var conta = await _contaService.FindByIdAsync(id.Value);
            return conta == null ? NotFound() : View(conta);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Conta conta)
        {
            if (id != conta.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(conta);
            }

            try
            {
                await _contaService.Update(conta);
                return RedirectToAction(nameof(Index));
            }
            catch (RegraNegocioException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(conta);
            }
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var conta = await _contaService.FindByIdAsync(id.Value);
            return conta == null ? NotFound() : View(conta);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _contaService.RemoveAsync(id);
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
