using FinanceHub.Models;
using FinanceHub.Models.Exceptions;
using FinanceHub.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FinanceHub.Controllers
{
    public class TransacoesController : Controller
    {
        private readonly TransacaoService _transacaoService;
        private readonly CategoriaService _categoriaService;
        private readonly ContaService _contaService;
        private readonly RecorrenciaProcessorService _recorrenciaProcessorService;
        private readonly UsuarioAtualService _usuarioAtualService;

        public TransacoesController(
            TransacaoService transacaoService,
            CategoriaService categoriaService,
            ContaService contaService,
            RecorrenciaProcessorService recorrenciaProcessorService,
            UsuarioAtualService usuarioAtualService)
        {
            _transacaoService = transacaoService;
            _categoriaService = categoriaService;
            _contaService = contaService;
            _recorrenciaProcessorService = recorrenciaProcessorService;
            _usuarioAtualService = usuarioAtualService;
        }

        public async Task<IActionResult> Index()
        {
            await _recorrenciaProcessorService.GerarPendentesUsuarioAsync(
                _usuarioAtualService.ObterUsuarioId(),
                DateTime.Now);
            return View(await _transacaoService.FindAllAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transacao = await _transacaoService.FindByIdAsync(id.Value);
            return transacao == null ? NotFound() : View(transacao);
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
            if (!await ValidarPreRequisitosAsync())
            {
                await CarregarOpcoesAsync(transacao.CategoriaId, transacao.ContaId);
                return View(transacao);
            }

            if (!ModelState.IsValid)
            {
                await CarregarOpcoesAsync(transacao.CategoriaId, transacao.ContaId);
                return View(transacao);
            }

            try
            {
                await _transacaoService.InsertAsync(transacao);
                return RedirectToAction(nameof(Index));
            }
            catch (RegraNegocioException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await CarregarOpcoesAsync(transacao.CategoriaId, transacao.ContaId);
                return View(transacao);
            }
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transacao = await _transacaoService.FindByIdAsync(id.Value);
            if (transacao == null)
            {
                return NotFound();
            }

            await CarregarOpcoesAsync(transacao.CategoriaId, transacao.ContaId);
            return View(transacao);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Transacao transacao)
        {
            if (id != transacao.Id)
            {
                return NotFound();
            }

            if (!await ValidarPreRequisitosAsync())
            {
                await CarregarOpcoesAsync(transacao.CategoriaId, transacao.ContaId);
                return View(transacao);
            }

            if (!ModelState.IsValid)
            {
                await CarregarOpcoesAsync(transacao.CategoriaId, transacao.ContaId);
                return View(transacao);
            }

            try
            {
                await _transacaoService.Update(transacao);
                return RedirectToAction(nameof(Index));
            }
            catch (RegraNegocioException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await CarregarOpcoesAsync(transacao.CategoriaId, transacao.ContaId);
                return View(transacao);
            }
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transacao = await _transacaoService.FindByIdAsync(id.Value);
            return transacao == null ? NotFound() : View(transacao);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _transacaoService.RemoveAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (RegraNegocioException ex)
            {
                TempData["Erro"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        private async Task CarregarOpcoesAsync(
            int? categoriaSelecionada = null,
            int? contaSelecionada = null)
        {
            var categorias = await _categoriaService.FindAllAsync();
            var contas = await _contaService.FindAtivasAsync();

            ViewBag.PossuiCategorias = categorias.Count > 0;
            ViewBag.PossuiContas = contas.Count > 0;
            ViewBag.PodeSalvar = categorias.Count > 0 && contas.Count > 0;

            ViewBag.Categorias = new SelectList(
                categorias.OrderBy(categoria => categoria.Nome),
                nameof(Categoria.Id),
                nameof(Categoria.Nome),
                categoriaSelecionada);

            ViewBag.Contas = new SelectList(
                contas.OrderBy(conta => conta.Nome),
                nameof(Conta.Id),
                nameof(Conta.Nome),
                contaSelecionada);
        }

        private async Task<bool> ValidarPreRequisitosAsync()
        {
            var possuiCategorias = (await _categoriaService.FindAllAsync()).Count > 0;
            var possuiContas = (await _contaService.FindAtivasAsync()).Count > 0;

            if (!possuiCategorias)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Cadastre ao menos uma categoria antes de lancar uma transacao.");
            }

            if (!possuiContas)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Cadastre e ative ao menos uma conta antes de lancar uma transacao.");
            }

            return possuiCategorias && possuiContas;
        }
    }
}
