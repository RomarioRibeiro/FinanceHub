using FinanceHub.Models;
using FinanceHub.Models.Exceptions;
using FinanceHub.Repositories;

namespace FinanceHub.Services
{
    public class ContaService
    {
        private readonly IRepository<Conta> _repository;
        private readonly UsuarioAtualService _usuarioAtualService;

        public ContaService(
            IRepository<Conta> repository,
            UsuarioAtualService usuarioAtualService)
        {
            _repository = repository;
            _usuarioAtualService = usuarioAtualService;
        }

        public Task<List<Conta>> FindAllAsync()
        {
            var usuarioId = _usuarioAtualService.ObterUsuarioId();
            return _repository.FindAllAsync(conta => conta.UsuarioId == usuarioId);
        }

        public Task<List<Conta>> FindAtivasAsync()
        {
            var usuarioId = _usuarioAtualService.ObterUsuarioId();
            return _repository.FindAllAsync(
                conta => conta.UsuarioId == usuarioId && conta.Ativa);
        }

        public Task<Conta?> FindByIdAsync(int id)
        {
            var usuarioId = _usuarioAtualService.ObterUsuarioId();
            return _repository.FindFirstAsync(
                conta => conta.Id == id && conta.UsuarioId == usuarioId);
        }

        public async Task InsertAsync(Conta conta)
        {
            ValidarConta(conta);

            conta.UsuarioId = _usuarioAtualService.ObterUsuarioId();
            conta.Nome = conta.Nome.Trim();
            conta.Ativa = true;
            conta.DefinirSaldoInicial(conta.SaldoInicial);

            await _repository.InsertAsync(conta);
        }

        public async Task Update(Conta conta)
        {
            ValidarConta(conta);

            var contaAtual = await FindByIdAsync(conta.Id);
            if (contaAtual == null)
            {
                throw new RegraNegocioException("Conta nao encontrada.");
            }

            contaAtual.Nome = conta.Nome.Trim();
            contaAtual.Tipo = conta.Tipo;
            contaAtual.Ativa = conta.Ativa;

            await _repository.UpdateAsync(contaAtual);
        }

        public async Task RemoveAsync(int id)
        {
            var usuarioId = _usuarioAtualService.ObterUsuarioId();
            var conta = await _repository.FindFirstAsync(
                item => item.Id == id && item.UsuarioId == usuarioId,
                item => item.Transacoes);

            if (conta == null)
            {
                throw new RegraNegocioException("Conta nao encontrada.");
            }

            if (conta.Transacoes.Count > 0)
            {
                throw new RegraNegocioException(
                    "A conta possui transacoes e nao pode ser excluida. Desative a conta para preservar o historico.");
            }

            await _repository.RemoveAsync(conta);
        }

        private static void ValidarConta(Conta conta)
        {
            if (string.IsNullOrWhiteSpace(conta.Nome))
            {
                throw new RegraNegocioException("Informe o nome da conta.");
            }

            if (conta.Nome.Trim().Length is < 2 or > 100)
            {
                throw new RegraNegocioException("O nome da conta deve ter entre 2 e 100 caracteres.");
            }
        }
    }
}
