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
        private readonly IRepository<Conta> _contaRepository;
        private readonly UsuarioAtualService _usuarioAtualService;

        public LancamentoRecorrenteService(
            IRepository<LancamentoRecorrente> repository,
            IRepository<Categoria> categoriaRepository,
            IRepository<Conta> contaRepository,
            UsuarioAtualService usuarioAtualService)
        {
            _repository = repository;
            _categoriaRepository = categoriaRepository;
            _contaRepository = contaRepository;
            _usuarioAtualService = usuarioAtualService;
        }

        public Task<List<LancamentoRecorrente>> FindAllAsync()
        {
            var usuarioId = _usuarioAtualService.ObterUsuarioId();
            return _repository.FindAllAsync(
                item => item.UsuarioId == usuarioId,
                item => item.Categoria!,
                item => item.Conta!);
        }

        public Task<LancamentoRecorrente?> FindByIdAsync(int id)
        {
            var usuarioId = _usuarioAtualService.ObterUsuarioId();
            return _repository.FindFirstAsync(
                item => item.Id == id && item.UsuarioId == usuarioId,
                item => item.Categoria!,
                item => item.Conta!);
        }

        public async Task InsertAsync(LancamentoRecorrente lancamento)
        {
            var usuarioId = _usuarioAtualService.ObterUsuarioId();
            var categoria = await ValidarAsync(lancamento, usuarioId);

            lancamento.UsuarioId = usuarioId;
            lancamento.Tipo = categoria.TipoCategoria;
            lancamento.Descricao = lancamento.Descricao.Trim();
            lancamento.Ativo = true;

            await _repository.InsertAsync(lancamento);
        }

        public async Task Update(LancamentoRecorrente lancamento)
        {
            var usuarioId = _usuarioAtualService.ObterUsuarioId();
            var categoria = await ValidarAsync(lancamento, usuarioId);
            var atual = await FindByIdAsync(lancamento.Id);

            if (atual == null)
            {
                throw new RegraNegocioException("Lancamento recorrente nao encontrado.");
            }

            atual.CategoriaId = lancamento.CategoriaId;
            atual.ContaId = lancamento.ContaId;
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

        private async Task<Categoria> ValidarAsync(LancamentoRecorrente lancamento, int usuarioId)
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
            if (categoria == null)
            {
                throw new RegraNegocioException("Categoria nao encontrada.");
            }

            if (!lancamento.ContaId.HasValue || lancamento.ContaId.Value <= 0)
            {
                throw new RegraNegocioException("Informe uma conta para a recorrencia.");
            }

            var conta = await _contaRepository.FindFirstAsync(
                item => item.Id == lancamento.ContaId.Value && item.UsuarioId == usuarioId);

            if (conta == null)
            {
                throw new RegraNegocioException("A conta informada nao pertence ao usuario autenticado.");
            }

            if (!conta.Ativa)
            {
                throw new RegraNegocioException("Nao e permitido vincular recorrencia a uma conta inativa.");
            }

            return categoria;
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
