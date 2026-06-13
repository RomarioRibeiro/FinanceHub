using FinanceHub.Models;
using FinanceHub.Models.Exceptions;
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

        public async Task<Categoria?> FindByIdAsync(int id)
        {
            return await _repository.FindByIdAsync(id);
        }

        public async Task InsertAsync(Categoria categoria)
        {
            Validar(categoria);
            categoria.Nome = categoria.Nome.Trim();
            categoria.Descricao = categoria.Descricao.Trim();
            await _repository.InsertAsync(categoria);
        }

        public async Task Update(Categoria categoria)
        {
            Validar(categoria);
            var atual = await _repository.FindByIdAsync(categoria.Id);
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
            var entity = await _repository.FindByIdAsync(
                id,
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
