using FinanceHub.Models;
using FinanceHub.Repositories;

namespace FinanceHub.Services
{
    public class SaldoService
    {
        private readonly IRepository<Saldo> _repository;
        private readonly UsuarioAtualService _usuarioAtualService;

        public SaldoService(IRepository<Saldo> repository, UsuarioAtualService usuarioAtualService)
        {
            _repository = repository;
            _usuarioAtualService = usuarioAtualService;
        }

        public Task<List<Saldo>> FindAllAsync()
        {
            var usuarioId = _usuarioAtualService.ObterUsuarioId();
            return _repository.FindAllAsync(
                saldo => saldo.Conta.UsuarioId == usuarioId,
                saldo => saldo.Conta);
        }

        public Task<Saldo?> FindByIdAsync(int id)
        {
            var usuarioId = _usuarioAtualService.ObterUsuarioId();
            return _repository.FindFirstAsync(
                saldo => saldo.Id == id && saldo.Conta.UsuarioId == usuarioId,
                saldo => saldo.Conta);
        }
    }
}
