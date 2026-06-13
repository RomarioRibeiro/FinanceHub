using FinanceHub.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinanceHub.Controllers
{
    public class SaldosController : Controller
    {
        private readonly SaldoService _saldoService;

        public SaldosController(SaldoService saldoService)
        {
            _saldoService = saldoService;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _saldoService.FindAllAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var saldo = await _saldoService.FindByIdAsync(id.Value);
            return saldo == null ? NotFound() : View(saldo);
        }
    }
}
