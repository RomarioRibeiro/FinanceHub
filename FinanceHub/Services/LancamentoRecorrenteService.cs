using FinanceHub.Models;
using FinanceHub.Repositories;

namespace FinanceHub.Services
{
    public class LancamentoRecorrenteService
    {
        private readonly IRepository<LancamentoRecorrente> _repository;

        public LancamentoRecorrenteService(IRepository<LancamentoRecorrente> repository)
        {
            _repository = repository;
        }

        public async Task<List<LancamentoRecorrente>> FindAllAsync()
        {
            return await _repository.FindAllAsync(obj => obj.Usuario, obj => obj.Categoria);
        }

        public async Task<LancamentoRecorrente> FindByIdAsync(int id)
        {
            return await _repository.FindByIdAsync(id, obj => obj.Usuario, obj => obj.Categoria);
        }

        public async Task InsertAsync(LancamentoRecorrente lancamentoRecorrente)
        {
            await _repository.InsertAsync(lancamentoRecorrente);
        }

        public async Task Update(LancamentoRecorrente lancamentoRecorrente)
        {
            if (!await _repository.ExistsAsync(lancamentoRecorrente.Id))
            {
                throw new Exception();
            }

            await _repository.UpdateAsync(lancamentoRecorrente);
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
