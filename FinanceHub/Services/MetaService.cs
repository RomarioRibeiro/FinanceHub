using FinanceHub.Models;
using FinanceHub.Repositories;

namespace FinanceHub.Services
{
    public class MetaService
    {
        private readonly IRepository<Meta> _repository;

        public MetaService(IRepository<Meta> repository)
        {
            _repository = repository;
        }

        public async Task<List<Meta>> FindAllAsync()
        {
            return await _repository.FindAllAsync(obj => obj.Usuario);
        }

        public async Task<Meta> FindByIdAsync(int id)
        {
            return await _repository.FindByIdAsync(id, obj => obj.Usuario);
        }

        public async Task InsertAsync(Meta meta)
        {
            await _repository.InsertAsync(meta);
        }

        public async Task Update(Meta meta)
        {
            if (!await _repository.ExistsAsync(meta.Id))
            {
                throw new Exception();
            }

            await _repository.UpdateAsync(meta);
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
