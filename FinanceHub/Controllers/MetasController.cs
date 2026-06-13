using FinanceHub.Models;
using FinanceHub.Models.Exceptions;
using FinanceHub.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinanceHub.Controllers
{
    public class MetasController : Controller
    {
        private readonly MetaService _metaService;

        public MetasController(MetaService metaService)
        {
            _metaService = metaService;
        }

        public async Task<IActionResult> Index() => View(await _metaService.FindAllAsync());

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var meta = await _metaService.FindByIdAsync(id.Value);
            if (meta == null) return NotFound();
            return View(meta);
        }

        public IActionResult Create() => View(new Meta { DataInicio = DateTime.Now, Ativa = true });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Meta meta)
        {
            if (!ModelState.IsValid) return View(meta);
            try
            {
                await _metaService.InsertAsync(meta);
                return RedirectToAction(nameof(Index));
            }
            catch (RegraNegocioException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(meta);
            }
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var meta = await _metaService.FindByIdAsync(id.Value);
            if (meta == null) return NotFound();
            return View(meta);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Meta meta)
        {
            if (id != meta.Id) return NotFound();
            if (!ModelState.IsValid) return View(meta);

            try
            {
                await _metaService.Update(meta);
                return RedirectToAction(nameof(Index));
            }
            catch (RegraNegocioException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(meta);
            }
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var meta = await _metaService.FindByIdAsync(id.Value);
            if (meta == null) return NotFound();
            return View(meta);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _metaService.RemoveAsync(id);
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
