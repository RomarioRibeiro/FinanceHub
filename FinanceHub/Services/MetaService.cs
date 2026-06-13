using FinanceHub.Models;
using FinanceHub.Models.Exceptions;
using FinanceHub.Repositories;

namespace FinanceHub.Services
{
    public class MetaService
    {
        private readonly IRepository<Meta> _repository;
        private readonly UsuarioAtualService _usuarioAtualService;

        public MetaService(IRepository<Meta> repository, UsuarioAtualService usuarioAtualService)
        {
            _repository = repository;
            _usuarioAtualService = usuarioAtualService;
        }

        public Task<List<Meta>> FindAllAsync()
        {
            var usuarioId = _usuarioAtualService.ObterUsuarioId();
            return _repository.FindAllAsync(meta => meta.UsuarioId == usuarioId);
        }

        public Task<Meta?> FindByIdAsync(int id)
        {
            var usuarioId = _usuarioAtualService.ObterUsuarioId();
            return _repository.FindFirstAsync(meta => meta.Id == id && meta.UsuarioId == usuarioId);
        }

        public async Task InsertAsync(Meta meta)
        {
            Validar(meta);
            meta.UsuarioId = _usuarioAtualService.ObterUsuarioId();
            meta.Nome = meta.Nome.Trim();
            meta.ValorAtual = 0;
            meta.Ativa = true;
            await _repository.InsertAsync(meta);
        }

        public async Task Update(Meta meta)
        {
            Validar(meta);
            var metaAtual = await FindByIdAsync(meta.Id);
            if (metaAtual == null)
            {
                throw new RegraNegocioException("Meta nao encontrada.");
            }

            metaAtual.Nome = meta.Nome.Trim();
            metaAtual.ValorAlvo = meta.ValorAlvo;
            metaAtual.DataInicio = meta.DataInicio;
            metaAtual.DataFim = meta.DataFim;
            metaAtual.Ativa = meta.Ativa;
            metaAtual.AtualizarProgresso(meta.ValorAtual);
            await _repository.UpdateAsync(metaAtual);
        }

        public async Task RemoveAsync(int id)
        {
            var entity = await FindByIdAsync(id);
            if (entity == null)
            {
                throw new RegraNegocioException("Meta nao encontrada.");
            }

            await _repository.RemoveAsync(entity);
        }

        private static void Validar(Meta meta)
        {
            if (string.IsNullOrWhiteSpace(meta.Nome) || meta.Nome.Trim().Length is < 3 or > 100)
            {
                throw new RegraNegocioException("O nome da meta deve ter entre 3 e 100 caracteres.");
            }

            if (meta.ValorAlvo <= 0)
            {
                throw new RegraNegocioException("O valor alvo deve ser maior que zero.");
            }

            if (meta.ValorAtual < 0)
            {
                throw new RegraNegocioException("O valor atual nao pode ser negativo.");
            }

            if (meta.DataInicio == default || meta.DataFim == default || meta.DataFim < meta.DataInicio)
            {
                throw new RegraNegocioException("A data final deve ser igual ou posterior a data inicial.");
            }
        }
    }
}
