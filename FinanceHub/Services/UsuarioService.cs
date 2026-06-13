using FinanceHub.Models;
using FinanceHub.Repositories;
using Microsoft.AspNetCore.Identity;

namespace FinanceHub.Services
{
    public class UsuarioService
    {
        private readonly IRepository<Usuario> _repository;
        private readonly PasswordHasher<Usuario> _passwordHasher = new();

        public UsuarioService(IRepository<Usuario> repository)
        {
            _repository = repository;
        }

        public async Task<List<Usuario>> FindAllAsync()
        {
            return await _repository.FindAllAsync(obj => obj.Perfis);
        }

        public async Task<Usuario?> FindByIdAsync(int id)
        {
            return await _repository.FindByIdAsync(id, obj => obj.Perfis);
        }

        public async Task InsertAsync(Usuario usuario)
        {
            usuario.SenhaHash = _passwordHasher.HashPassword(usuario, usuario.SenhaHash);
            await _repository.InsertAsync(usuario);
        }

        public async Task Update(Usuario usuario)
        {
            var usuarioAtual = await _repository.FindByIdAsync(usuario.Id, item => item.Perfis);
            if (usuarioAtual == null)
            {
                throw new Exception();
            }

            usuarioAtual.Nome = usuario.Nome;
            usuarioAtual.Email = usuario.Email;
            usuarioAtual.Ativo = usuario.Ativo;

            if (!string.IsNullOrWhiteSpace(usuario.SenhaHash))
            {
                usuarioAtual.SenhaHash = _passwordHasher.HashPassword(usuarioAtual, usuario.SenhaHash);
            }

            await _repository.UpdateAsync(usuarioAtual);
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
