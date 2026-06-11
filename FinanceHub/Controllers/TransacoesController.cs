using FinanceHub.Models;
using FinanceHub.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FinanceHub.Controllers
{
    public class TransacoesController : Controller
    {
        private readonly TransacaoService _transacaoService;
        private readonly UsuarioService _usuarioService;
        private readonly CategoriaService _categoriaService;
        private readonly ContaService _contaService;

        public TransacoesController(
            TransacaoService transacaoService,
            UsuarioService usuarioService,
            CategoriaService categoriaService,
            ContaService contaService)
        {
            _transacaoService = transacaoService;
            _usuarioService = usuarioService;
            _categoriaService = categoriaService;
            _contaService = contaService;
        }

        public async Task<IActionResult> Index() => View(await _transacaoService.FindAllAsync());

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var transacao = await _transacaoService.FindByIdAsync(id.Value);
            if (transacao == null) return NotFound();
            return View(transacao);
        }

        public async Task<IActionResult> Create()
        {
            await CarregarOpcoesAsync();
            return View(new Transacao { Data = DateTime.Now });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Transacao transacao)
        {
            if (!ModelState.IsValid)
            {
                await CarregarOpcoesAsync(
                    transacao.UsuarioId,
                    transacao.CategoriaId,
                    transacao.ContaId);
                return View(transacao);
            }

            if (transacao.Data == default) transacao.Data = DateTime.Now;
            await _transacaoService.InsertAsync(transacao);
            return RedirectToAction(nameof(Index));
        }

        private async Task CarregarOpcoesAsync(
            int? usuarioSelecionado = null,
            int? categoriaSelecionada = null,
            int? contaSelecionada = null)
        {
            var usuarios = await _usuarioService.FindAllAsync();
            var categorias = await _categoriaService.FindAllAsync();
            var contasCadastradas = await _contaService.FindAllAsync();

            ViewBag.Usuarios = new SelectList(
                usuarios.OrderBy(usuario => usuario.Nome),
                nameof(Usuario.Id),
                nameof(Usuario.Nome),
                usuarioSelecionado);

            ViewBag.Categorias = new SelectList(
                categorias.OrderBy(categoria => categoria.Nome),
                nameof(Categoria.Id),
                nameof(Categoria.Nome),
                categoriaSelecionada);

            var contas = contasCadastradas
                .OrderBy(conta => conta.Nome)
                .Select(conta => new
                {
                    conta.Id,
                    Descricao = $"{conta.Nome} - {conta.Usuario?.Nome ?? "Usuario nao informado"}"
                });

            ViewBag.Contas = new SelectList(
                contas,
                nameof(Conta.Id),
                "Descricao",
                contaSelecionada);
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
