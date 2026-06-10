using FinanceHub.Models;
using FinanceHub.Repositories;

namespace FinanceHub.Services
{
    public class PerfilService
    {
        private readonly IRepository<Perfil> _repository;

        public PerfilService(IRepository<Perfil> repository)
        {
            _repository = repository;
        }

        public async Task<List<Perfil>> FindAllAsync()
        {
            return await _repository.FindAllAsync(obj => obj.Usuarios);
        }

        public async Task<Perfil> FindByIdAsync(int id)
        {
            return await _repository.FindByIdAsync(id, obj => obj.Usuarios);
        }

        public async Task InsertAsync(Perfil perfil)
        {
            await _repository.InsertAsync(perfil);
        }

        public async Task Update(Perfil perfil)
        {
            if (!await _repository.ExistsAsync(perfil.Id))
            {
                throw new Exception();
            }

            await _repository.UpdateAsync(perfil);
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
