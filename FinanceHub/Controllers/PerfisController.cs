using FinanceHub.Models;
using FinanceHub.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinanceHub.Controllers
{
    [Authorize(Policy = "Administrador")]
    public class PerfisController : Controller
    {
        private readonly PerfilService _perfilService;

        public PerfisController(PerfilService perfilService)
        {
            _perfilService = perfilService;
        }

        public async Task<IActionResult> Index() => View(await _perfilService.FindAllAsync());

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var perfil = await _perfilService.FindByIdAsync(id.Value);
            if (perfil == null) return NotFound();
            return View(perfil);
        }

        public IActionResult Create() => View(new Perfil());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Perfil perfil)
        {
            if (!ModelState.IsValid) return View(perfil);
            await _perfilService.InsertAsync(perfil);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var perfil = await _perfilService.FindByIdAsync(id.Value);
            if (perfil == null) return NotFound();
            return View(perfil);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Perfil perfil)
        {
            if (id != perfil.Id) return NotFound();
            if (!ModelState.IsValid) return View(perfil);

            try
            {
                await _perfilService.Update(perfil);
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
            var perfil = await _perfilService.FindByIdAsync(id.Value);
            if (perfil == null) return NotFound();
            return View(perfil);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _perfilService.RemoveAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
