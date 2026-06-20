using FinanceHub.Models;
using FinanceHub.Models.ViewModels;
using FinanceHub.Repositories;

namespace FinanceHub.Services
{
    public class SaldoService
    {
        private readonly IRepository<Conta> _contaRepository;
        private readonly UsuarioAtualService _usuarioAtualService;

        public SaldoService(IRepository<Conta> contaRepository, UsuarioAtualService usuarioAtualService)
        {
            _contaRepository = contaRepository;
            _usuarioAtualService = usuarioAtualService;
        }

        public async Task<List<SaldoContaViewModel>> FindAllAsync()
        {
            var usuarioId = _usuarioAtualService.ObterUsuarioId();
            var contas = await _contaRepository.FindAllAsync(
                conta => conta.UsuarioId == usuarioId);

            return contas
                .OrderBy(conta => conta.Nome)
                .Select(conta => new SaldoContaViewModel
                {
                    ContaId = conta.Id,
                    ContaNome = conta.Nome,
                    TipoConta = conta.Tipo,
                    SaldoInicial = conta.SaldoInicial,
                    SaldoAtual = conta.SaldoAtual,
                    Ativa = conta.Ativa
                })
                .ToList();
        }

        public async Task<SaldoContaViewModel?> FindByIdAsync(int id)
        {
            var usuarioId = _usuarioAtualService.ObterUsuarioId();
            var conta = await _contaRepository.FindFirstAsync(
                item => item.Id == id && item.UsuarioId == usuarioId);

            if (conta == null)
            {
                return null;
            }

            return new SaldoContaViewModel
            {
                ContaId = conta.Id,
                ContaNome = conta.Nome,
                TipoConta = conta.Tipo,
                SaldoInicial = conta.SaldoInicial,
                SaldoAtual = conta.SaldoAtual,
                Ativa = conta.Ativa
            };
        }
    }
}
