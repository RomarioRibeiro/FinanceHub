using FinanceHub.Models;
using FinanceHub.Models.Exceptions;
using FinanceHub.Repositories;

namespace FinanceHub.Services
{
    public class RelatorioService
    {
        private readonly IRepository<Relatorio> _repository;
        private readonly UsuarioAtualService _usuarioAtualService;

        public RelatorioService(IRepository<Relatorio> repository, UsuarioAtualService usuarioAtualService)
        {
            _repository = repository;
            _usuarioAtualService = usuarioAtualService;
        }

        public Task<List<Relatorio>> FindAllAsync()
        {
            var usuarioId = _usuarioAtualService.ObterUsuarioId();
            return _repository.FindAllAsync(relatorio => relatorio.UsuarioId == usuarioId);
        }

        public Task<Relatorio?> FindByIdAsync(int id)
        {
            var usuarioId = _usuarioAtualService.ObterUsuarioId();
            return _repository.FindFirstAsync(
                relatorio => relatorio.Id == id && relatorio.UsuarioId == usuarioId);
        }

        public async Task InsertAsync(Relatorio relatorio)
        {
            ValidarPeriodo(relatorio);
            relatorio.UsuarioId = _usuarioAtualService.ObterUsuarioId();
            relatorio.GeradoEm = DateTime.Now;
            await _repository.InsertAsync(relatorio);
        }

        public async Task Update(Relatorio relatorio)
        {
            ValidarPeriodo(relatorio);
            var relatorioAtual = await FindByIdAsync(relatorio.Id);
            if (relatorioAtual == null)
            {
                throw new RegraNegocioException("Relatorio nao encontrado.");
            }

            relatorioAtual.Tipo = relatorio.Tipo;
            relatorioAtual.DataInicio = relatorio.DataInicio;
            relatorioAtual.DataFim = relatorio.DataFim;
            await _repository.UpdateAsync(relatorioAtual);
        }

        public async Task RemoveAsync(int id)
        {
            var entity = await FindByIdAsync(id);
            if (entity == null)
            {
                throw new RegraNegocioException("Relatorio nao encontrado.");
            }

            await _repository.RemoveAsync(entity);
        }

        private static void ValidarPeriodo(Relatorio relatorio)
        {
            if (relatorio.DataInicio == default
                || relatorio.DataFim == default
                || relatorio.DataFim < relatorio.DataInicio)
            {
                throw new RegraNegocioException("A data final deve ser igual ou posterior a data inicial.");
            }
        }
    }
}
