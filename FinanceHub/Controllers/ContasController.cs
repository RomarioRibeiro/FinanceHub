using FinanceHub.Models;
using FinanceHub.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FinanceHub.Controllers
{
    public class ContasController : Controller
    {
        private readonly ContaService _contaService;
        private readonly UsuarioService _usuarioService;

        public ContasController(ContaService contaService, UsuarioService usuarioService)
        {
            _contaService = contaService;
            _usuarioService = usuarioService;
        }

        public async Task<IActionResult> Index() => View(await _contaService.FindAllAsync());

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var conta = await _contaService.FindByIdAsync(id.Value);
            if (conta == null) return NotFound();
            return View(conta);
        }

        public async Task<IActionResult> Create()
        {
            await CarregarUsuariosAsync();
            return View(new Conta { Ativa = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Conta conta)
        {
            if (!ModelState.IsValid)
            {
                await CarregarUsuariosAsync(conta.UsuarioId);
                return View(conta);
            }

            conta.Ativa = true;
            await _contaService.InsertAsync(conta);
            return RedirectToAction(nameof(Index));
        }

        private async Task CarregarUsuariosAsync(int? usuarioSelecionado = null)
        {
            var usuarios = await _usuarioService.FindAllAsync();
            ViewBag.Usuarios = new SelectList(
                usuarios.OrderBy(usuario => usuario.Nome),
                nameof(Usuario.Id),
                nameof(Usuario.Nome),
                usuarioSelecionado);
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
