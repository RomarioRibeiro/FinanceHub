using FinanceHub.Models;
using FinanceHub.Repositories;

namespace FinanceHub.Services
{
    public class SaldoService
    {
        private readonly IRepository<Saldo> _repository;

        public SaldoService(IRepository<Saldo> repository)
        {
            _repository = repository;
        }

        public async Task<List<Saldo>> FindAllAsync()
        {
            return await _repository.FindAllAsync(obj => obj.Conta);
        }

        public async Task<Saldo> FindByIdAsync(int id)
        {
            return await _repository.FindByIdAsync(id, obj => obj.Conta);
        }

        public async Task InsertAsync(Saldo saldo)
        {
            await _repository.InsertAsync(saldo);
        }

        public async Task Update(Saldo saldo)
        {
            if (!await _repository.ExistsAsync(saldo.Id))
            {
                throw new Exception();
            }

            await _repository.UpdateAsync(saldo);
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
