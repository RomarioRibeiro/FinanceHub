using FinanceHub.Models;
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

        public LancamentosRecorrentesController(
            LancamentoRecorrenteService service,
            CategoriaService categoriaService)
        {
            _service = service;
            _categoriaService = categoriaService;
        }

        public async Task<IActionResult> Index()
        {
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
            await CarregarCategoriasAsync();
            return View(new LancamentoRecorrente { DataInicial = DateTime.Now, Ativo = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LancamentoRecorrente lancamento)
        {
            if (!ModelState.IsValid)
            {
                await CarregarCategoriasAsync(lancamento.CategoriaId);
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
                await CarregarCategoriasAsync(lancamento.CategoriaId);
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

            await CarregarCategoriasAsync(item.CategoriaId);
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

            if (!ModelState.IsValid)
            {
                await CarregarCategoriasAsync(lancamento.CategoriaId);
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
                await CarregarCategoriasAsync(lancamento.CategoriaId);
                return View(lancamento);
            }
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

        private async Task CarregarCategoriasAsync(int? selecionada = null)
        {
            var categorias = await _categoriaService.FindAllAsync();
            ViewBag.Categorias = new SelectList(
                categorias.OrderBy(categoria => categoria.Nome),
                nameof(Categoria.Id),
                nameof(Categoria.Nome),
                selecionada);
        }
    }
}
