using FinanceHub.Models;
using FinanceHub.Repositories;

namespace FinanceHub.Services
{
    public class CategoriaService
    {
        private readonly IRepository<Categoria> _repository;

        public CategoriaService(IRepository<Categoria> repository)
        {
            _repository = repository;
        }

        public async Task<List<Categoria>> FindAllAsync()
        {
            return await _repository.FindAllAsync();
        }

        public async Task<Categoria> FindByIdAsync(int id)
        {
            return await _repository.FindByIdAsync(id);
        }

        public async Task InsertAsync(Categoria categoria)
        {
            await _repository.InsertAsync(categoria);
        }

        public async Task Update(Categoria categoria)
        {
            if (!await _repository.ExistsAsync(categoria.Id))
            {
                throw new Exception();
            }

            await _repository.UpdateAsync(categoria);
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
