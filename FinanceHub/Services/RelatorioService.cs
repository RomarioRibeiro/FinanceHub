using FinanceHub.Models;
using FinanceHub.Repositories;

namespace FinanceHub.Services
{
    public class RelatorioService
    {
        private readonly IRepository<Relatorio> _repository;

        public RelatorioService(IRepository<Relatorio> repository)
        {
            _repository = repository;
        }

        public async Task<List<Relatorio>> FindAllAsync()
        {
            return await _repository.FindAllAsync(obj => obj.Usuario);
        }

        public async Task<Relatorio> FindByIdAsync(int id)
        {
            return await _repository.FindByIdAsync(id, obj => obj.Usuario);
        }

        public async Task InsertAsync(Relatorio relatorio)
        {
            await _repository.InsertAsync(relatorio);
        }

        public async Task Update(Relatorio relatorio)
        {
            if (!await _repository.ExistsAsync(relatorio.Id))
            {
                throw new Exception();
            }

            await _repository.UpdateAsync(relatorio);
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
