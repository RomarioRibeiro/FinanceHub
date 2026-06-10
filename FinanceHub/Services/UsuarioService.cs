using FinanceHub.Models;
using FinanceHub.Repositories;

namespace FinanceHub.Services
{
    public class UsuarioService
    {
        private readonly IRepository<Usuario> _repository;

        public UsuarioService(IRepository<Usuario> repository)
        {
            _repository = repository;
        }

        public async Task<List<Usuario>> FindAllAsync()
        {
            return await _repository.FindAllAsync(obj => obj.Perfis);
        }

        public async Task<Usuario> FindByIdAsync(int id)
        {
            return await _repository.FindByIdAsync(id, obj => obj.Perfis);
        }

        public async Task InsertAsync(Usuario usuario)
        {
            await _repository.InsertAsync(usuario);
        }

        public async Task Update(Usuario usuario)
        {
            if (!await _repository.ExistsAsync(usuario.Id))
            {
                throw new Exception();
            }

            await _repository.UpdateAsync(usuario);
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
