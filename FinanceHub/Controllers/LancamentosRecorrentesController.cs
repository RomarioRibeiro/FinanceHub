using FinanceHub.Models;
using FinanceHub.Models.Enums;
using FinanceHub.Models.Exceptions;
using FinanceHub.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FinanceHub.Controllers
{
    public class LancamentosRecorrentesController : Controller
    {
        private readonly LancamentoRecorrenteService _service;
        private readonly CategoriaService _categoriaService;
        private readonly ContaService _contaService;
        private readonly RecorrenciaProcessorService _processorService;
        private readonly UsuarioAtualService _usuarioAtualService;

        public LancamentosRecorrentesController(
            LancamentoRecorrenteService service,
            CategoriaService categoriaService,
            ContaService contaService,
            RecorrenciaProcessorService processorService,
            UsuarioAtualService usuarioAtualService)
        {
            _service = service;
            _categoriaService = categoriaService;
            _contaService = contaService;
            _processorService = processorService;
            _usuarioAtualService = usuarioAtualService;
        }

        public async Task<IActionResult> Index()
        {
            await _processorService.GerarPendentesUsuarioAsync(
                _usuarioAtualService.ObterUsuarioId(),
                DateTime.Now);
            return View(await _service.FindAllAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _service.FindByIdAsync(id.Value);
            return item == null ? NotFound() : View(item);
        }

        public async Task<IActionResult> Create()
        {
            await CarregarOpcoesAsync();
            return View(new LancamentoRecorrente { DataInicial = DateTime.Now, Ativo = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LancamentoRecorrente lancamento)
        {
            await NormalizarLancamentoAsync(lancamento);

            if (!await ValidarPreRequisitosAsync())
            {
                await CarregarOpcoesAsync(lancamento.CategoriaId, lancamento.ContaId);
                return View(lancamento);
            }

            if (!ModelState.IsValid)
            {
                await CarregarOpcoesAsync(lancamento.CategoriaId, lancamento.ContaId);
                return View(lancamento);
            }

            try
            {
                await _service.InsertAsync(lancamento);
                return RedirectToAction(nameof(Index));
            }
            catch (RegraNegocioException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await CarregarOpcoesAsync(lancamento.CategoriaId, lancamento.ContaId);
                return View(lancamento);
            }
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _service.FindByIdAsync(id.Value);
            if (item == null)
            {
                return NotFound();
            }

            await CarregarOpcoesAsync(item.CategoriaId, item.ContaId);
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, LancamentoRecorrente lancamento)
        {
            if (id != lancamento.Id)
            {
                return NotFound();
            }

            await NormalizarLancamentoAsync(lancamento);

            if (!await ValidarPreRequisitosAsync())
            {
                await CarregarOpcoesAsync(lancamento.CategoriaId, lancamento.ContaId);
                return View(lancamento);
            }

            if (!ModelState.IsValid)
            {
                await CarregarOpcoesAsync(lancamento.CategoriaId, lancamento.ContaId);
                return View(lancamento);
            }

            try
            {
                await _service.Update(lancamento);
                return RedirectToAction(nameof(Index));
            }
            catch (RegraNegocioException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await CarregarOpcoesAsync(lancamento.CategoriaId, lancamento.ContaId);
                return View(lancamento);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GerarAgora(int id)
        {
            try
            {
                await _processorService.GerarAgoraAsync(
                    id,
                    _usuarioAtualService.ObterUsuarioId(),
                    DateTime.Now);
                TempData["Sucesso"] = "Recorrencia gerada com sucesso.";
            }
            catch (RegraNegocioException ex)
            {
                TempData["Erro"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _service.FindByIdAsync(id.Value);
            return item == null ? NotFound() : View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _service.RemoveAsync(id);
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
                    "Cadastre ao menos uma categoria antes de registrar uma recorrencia.");
            }

            if (!possuiContas)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Cadastre e ative ao menos uma conta antes de registrar uma recorrencia.");
            }

            return possuiCategorias && possuiContas;
        }

        private async Task NormalizarLancamentoAsync(LancamentoRecorrente lancamento)
        {
            if (lancamento.DataInicial == default)
            {
                lancamento.DataInicial = DateTime.Now;
                ModelState.Remove(nameof(LancamentoRecorrente.DataInicial));
            }

            if (lancamento.Frequencia == FrequenciaRecorrencia.DIARIA)
            {
                lancamento.DiaReferencia = 0;
                ModelState.Remove(nameof(LancamentoRecorrente.DiaReferencia));
            }

            if (string.IsNullOrWhiteSpace(lancamento.Descricao) && lancamento.CategoriaId > 0)
            {
                var categoria = await _categoriaService.FindByIdAsync(lancamento.CategoriaId);
                if (categoria != null)
                {
                    lancamento.Descricao = categoria.Nome;
                    ModelState.Remove(nameof(LancamentoRecorrente.Descricao));
                }
            }

            if ((!lancamento.ContaId.HasValue || lancamento.ContaId.Value <= 0))
            {
                var contas = await _contaService.FindAtivasAsync();
                if (contas.Count == 1)
                {
                    lancamento.ContaId = contas[0].Id;
                    ModelState.Remove(nameof(LancamentoRecorrente.ContaId));
                }
            }

            TryValidateModel(lancamento);
        }
    }
}
