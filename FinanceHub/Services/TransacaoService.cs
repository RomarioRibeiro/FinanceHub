using FinanceHub.Models;
using FinanceHub.Repositories;

namespace FinanceHub.Services
{
    public class TransacaoService
    {
        private readonly IRepository<Transacao> _repository;

        public TransacaoService(IRepository<Transacao> repository)
        {
            _repository = repository;
        }

        public async Task<List<Transacao>> FindAllAsync()
        {
            return await _repository.FindAllAsync(obj => obj.Usuario, obj => obj.Categoria, obj => obj.Conta);
        }

        public async Task<Transacao> FindByIdAsync(int id)
        {
            return await _repository.FindByIdAsync(id, obj => obj.Usuario, obj => obj.Categoria, obj => obj.Conta);
        }

        public async Task InsertAsync(Transacao transacao)
        {
            await _repository.InsertAsync(transacao);
        }

        public async Task Update(Transacao transacao)
        {
            if (!await _repository.ExistsAsync(transacao.Id))
            {
                throw new Exception();
            }

            await _repository.UpdateAsync(transacao);
        }

        public async Task RemoveAsync(int id)
        {
            var entity = await _repository.FindByIdAsync(id);
            if (entity == null)
            {
                throw new Exception();
            }

            await _repository.RemoveAsync(entity);
        }
    }
}
