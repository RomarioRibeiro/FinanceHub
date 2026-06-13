using FinanceHub.Models;
using FinanceHub.Models.Enums;
using FinanceHub.Models.Exceptions;
using FinanceHub.Repositories;

namespace FinanceHub.Services
{
    public class LancamentoRecorrenteService
    {
        private readonly IRepository<LancamentoRecorrente> _repository;
        private readonly IRepository<Categoria> _categoriaRepository;
        private readonly UsuarioAtualService _usuarioAtualService;

        public LancamentoRecorrenteService(
            IRepository<LancamentoRecorrente> repository,
            IRepository<Categoria> categoriaRepository,
            UsuarioAtualService usuarioAtualService)
        {
            _repository = repository;
            _categoriaRepository = categoriaRepository;
            _usuarioAtualService = usuarioAtualService;
        }

        public Task<List<LancamentoRecorrente>> FindAllAsync()
        {
            var usuarioId = _usuarioAtualService.ObterUsuarioId();
            return _repository.FindAllAsync(
                item => item.UsuarioId == usuarioId,
                item => item.Categoria);
        }

        public Task<LancamentoRecorrente?> FindByIdAsync(int id)
        {
            var usuarioId = _usuarioAtualService.ObterUsuarioId();
            return _repository.FindFirstAsync(
                item => item.Id == id && item.UsuarioId == usuarioId,
                item => item.Categoria);
        }

        public async Task InsertAsync(LancamentoRecorrente lancamento)
        {
            var categoria = await ValidarAsync(lancamento);

            lancamento.UsuarioId = _usuarioAtualService.ObterUsuarioId();
            lancamento.Tipo = categoria.TipoCategoria;
            lancamento.Descricao = lancamento.Descricao.Trim();
            lancamento.Ativo = true;

            await _repository.InsertAsync(lancamento);
        }

        public async Task Update(LancamentoRecorrente lancamento)
        {
            var categoria = await ValidarAsync(lancamento);
            var atual = await FindByIdAsync(lancamento.Id);

            if (atual == null)
            {
                throw new RegraNegocioException("Lancamento recorrente nao encontrado.");
            }

            atual.CategoriaId = lancamento.CategoriaId;
            atual.Tipo = categoria.TipoCategoria;
            atual.Descricao = lancamento.Descricao.Trim();
            atual.Valor = lancamento.Valor;
            atual.DataInicial = lancamento.DataInicial;
            atual.Frequencia = lancamento.Frequencia;
            atual.DiaReferencia = lancamento.DiaReferencia;
            atual.Ativo = lancamento.Ativo;

            await _repository.UpdateAsync(atual);
        }

        public async Task RemoveAsync(int id)
        {
            var entity = await FindByIdAsync(id);
            if (entity == null)
            {
                throw new RegraNegocioException("Lancamento recorrente nao encontrado.");
            }

            await _repository.RemoveAsync(entity);
        }

        private async Task<Categoria> ValidarAsync(LancamentoRecorrente lancamento)
        {
            if (string.IsNullOrWhiteSpace(lancamento.Descricao)
                || lancamento.Descricao.Trim().Length is < 3 or > 200)
            {
                throw new RegraNegocioException("A descricao deve ter entre 3 e 200 caracteres.");
            }

            if (lancamento.Valor <= 0)
            {
                throw new RegraNegocioException("O valor deve ser maior que zero.");
            }

            if (lancamento.DataInicial == default)
            {
                throw new RegraNegocioException("Informe a data inicial.");
            }

            ValidarDiaReferencia(lancamento.Frequencia, lancamento.DiaReferencia);

            var categoria = await _categoriaRepository.FindByIdAsync(lancamento.CategoriaId);
            return categoria ?? throw new RegraNegocioException("Categoria nao encontrada.");
        }

        private static void ValidarDiaReferencia(
            FrequenciaRecorrencia frequencia,
            int diaReferencia)
        {
            var valido = frequencia switch
            {
                FrequenciaRecorrencia.DIARIA => diaReferencia == 0,
                FrequenciaRecorrencia.SEMANAL => diaReferencia is >= 1 and <= 7,
                FrequenciaRecorrencia.MENSAL => diaReferencia is >= 1 and <= 31,
                FrequenciaRecorrencia.ANUAL => diaReferencia is >= 1 and <= 366,
                _ => false
            };

            if (!valido)
            {
                throw new RegraNegocioException(
                    "Dia de referencia invalido para a frequencia selecionada.");
            }
        }
    }
}
