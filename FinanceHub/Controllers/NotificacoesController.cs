using FinanceHub.Models.Exceptions;
using FinanceHub.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinanceHub.Controllers
{
    public class NotificacoesController : Controller
    {
        private readonly NotificacaoMetaService _service;

        public NotificacoesController(NotificacaoMetaService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index()
        {
            var notificacoes = await _service.FindAllAsync();
            return View(notificacoes.OrderByDescending(item => item.CriadaEm).ToList());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarcarComoLida(int id)
        {
            try
            {
                await _service.MarcarComoLidaAsync(id);
            }
            catch (RegraNegocioException ex)
            {
                TempData["Erro"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarcarTodasComoLidas()
        {
            await _service.MarcarTodasComoLidasAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
