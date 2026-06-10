using FinanceHub.Models;
using FinanceHub.Repositories;

namespace FinanceHub.Services
{
    public class ContaService
    {
        private readonly IRepository<Conta> _repository;

        public ContaService(IRepository<Conta> repository)
        {
            _repository = repository;
        }

        public async Task<List<Conta>> FindAllAsync()
        {
            return await _repository.FindAllAsync(obj => obj.Usuario);
        }

        public async Task<Conta> FindByIdAsync(int id)
        {
            return await _repository.FindByIdAsync(id, obj => obj.Usuario);
        }

        public async Task InsertAsync(Conta conta)
        {
            await _repository.InsertAsync(conta);
        }

        public async Task Update(Conta conta)
        {
            if (!await _repository.ExistsAsync(conta.Id))
            {
                throw new Exception();
            }

            await _repository.UpdateAsync(conta);
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
