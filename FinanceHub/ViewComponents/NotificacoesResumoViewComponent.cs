using FinanceHub.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinanceHub.ViewComponents
{
    public class NotificacoesResumoViewComponent : ViewComponent
    {
        private readonly NotificacaoMetaService _service;

        public NotificacoesResumoViewComponent(NotificacaoMetaService service)
        {
            _service = service;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            return View(await _service.ContarNaoLidasAsync());
        }
    }
}
