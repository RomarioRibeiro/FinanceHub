using FinanceHub.Models;
using FinanceHub.Models.Exceptions;
using FinanceHub.Repositories;

namespace FinanceHub.Services
{
    public class CategoriaService
    {
        private readonly IRepository<Categoria> _repository;
        private readonly UsuarioAtualService _usuarioAtualService;

        public CategoriaService(
            IRepository<Categoria> repository,
            UsuarioAtualService usuarioAtualService)
        {
            _repository = repository;
            _usuarioAtualService = usuarioAtualService;
        }

        public Task<List<Categoria>> FindAllAsync()
        {
            var usuarioId = _usuarioAtualService.ObterUsuarioId();
            return _repository.FindAllAsync(categoria => categoria.UsuarioId == usuarioId);
        }

        public Task<Categoria?> FindByIdAsync(int id)
        {
            var usuarioId = _usuarioAtualService.ObterUsuarioId();
            return _repository.FindFirstAsync(
                categoria => categoria.Id == id && categoria.UsuarioId == usuarioId);
        }

        public async Task InsertAsync(Categoria categoria)
        {
            Validar(categoria);
            categoria.UsuarioId = _usuarioAtualService.ObterUsuarioId();
            categoria.Nome = categoria.Nome.Trim();
            categoria.Descricao = categoria.Descricao.Trim();
            await _repository.InsertAsync(categoria);
        }

        public async Task Update(Categoria categoria)
        {
            Validar(categoria);
            var usuarioId = _usuarioAtualService.ObterUsuarioId();
            var atual = await _repository.FindFirstAsync(
                item => item.Id == categoria.Id && item.UsuarioId == usuarioId);
            if (atual == null)
            {
                throw new RegraNegocioException("Categoria nao encontrada.");
            }

            atual.Nome = categoria.Nome.Trim();
            atual.Descricao = categoria.Descricao.Trim();
            atual.TipoCategoria = categoria.TipoCategoria;
            await _repository.UpdateAsync(atual);
        }

        public async Task RemoveAsync(int id)
        {
            var usuarioId = _usuarioAtualService.ObterUsuarioId();
            var entity = await _repository.FindFirstAsync(
                categoria => categoria.Id == id && categoria.UsuarioId == usuarioId,
                categoria => categoria.Transacoes,
                categoria => categoria.LancamentosRecorrentes);
            if (entity == null)
            {
                throw new RegraNegocioException("Categoria nao encontrada.");
            }

            if (entity.EstaAssociada())
            {
                throw new RegraNegocioException(
                    "A categoria possui movimentacoes associadas e nao pode ser excluida.");
            }

            await _repository.RemoveAsync(entity);
        }

        private static void Validar(Categoria categoria)
        {
            if (!categoria.Validar())
            {
                throw new RegraNegocioException(
                    "Nome e descricao devem ter entre 3 e 50 caracteres.");
            }
        }
    }
}
