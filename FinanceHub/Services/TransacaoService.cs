using FinanceHub.Models;
using FinanceHub.Models.Exceptions;
using FinanceHub.Repositories;

namespace FinanceHub.Services
{
    public class TransacaoService
    {
        private readonly IRepository<Transacao> _transacaoRepository;
        private readonly IRepository<Conta> _contaRepository;
        private readonly IRepository<Categoria> _categoriaRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UsuarioAtualService _usuarioAtualService;

        public TransacaoService(
            IRepository<Transacao> transacaoRepository,
            IRepository<Conta> contaRepository,
            IRepository<Categoria> categoriaRepository,
            IUnitOfWork unitOfWork,
            UsuarioAtualService usuarioAtualService)
        {
            _transacaoRepository = transacaoRepository;
            _contaRepository = contaRepository;
            _categoriaRepository = categoriaRepository;
            _unitOfWork = unitOfWork;
            _usuarioAtualService = usuarioAtualService;
        }

        public Task<List<Transacao>> FindAllAsync()
        {
            var usuarioId = _usuarioAtualService.ObterUsuarioId();
            return _transacaoRepository.FindAllAsync(
                transacao => transacao.UsuarioId == usuarioId,
                transacao => transacao.Categoria!,
                transacao => transacao.Conta!);
        }

        public Task<Transacao?> FindByIdAsync(int id)
        {
            var usuarioId = _usuarioAtualService.ObterUsuarioId();
            return _transacaoRepository.FindFirstAsync(
                transacao => transacao.Id == id && transacao.UsuarioId == usuarioId,
                transacao => transacao.Categoria!,
                transacao => transacao.Conta!);
        }

        public async Task InsertAsync(Transacao transacao)
        {
            var usuarioId = _usuarioAtualService.ObterUsuarioId();
            var (conta, categoria) = await ValidarRelacionamentosAsync(transacao, usuarioId);

            PrepararTransacao(transacao, usuarioId, categoria);

            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                conta.AtualizarSaldo(transacao.ObterImpactoNoSaldo());
                await _transacaoRepository.InsertAsync(transacao);
                await _contaRepository.UpdateAsync(conta);
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task Update(Transacao transacao)
        {
            var usuarioId = _usuarioAtualService.ObterUsuarioId();
            var transacaoAtual = await _transacaoRepository.FindFirstAsync(
                item => item.Id == transacao.Id && item.UsuarioId == usuarioId);

            if (transacaoAtual == null)
            {
                throw new RegraNegocioException("Transacao nao encontrada.");
            }

            var contaAnterior = await _contaRepository.FindFirstAsync(
                conta => conta.Id == transacaoAtual.ContaId && conta.UsuarioId == usuarioId);

            if (contaAnterior == null)
            {
                throw new RegraNegocioException("A conta original da transacao nao foi encontrada.");
            }

            var (novaConta, categoria) = await ValidarRelacionamentosAsync(transacao, usuarioId);

            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                contaAnterior.AtualizarSaldo(-transacaoAtual.ObterImpactoNoSaldo());

                transacaoAtual.CategoriaId = transacao.CategoriaId;
                transacaoAtual.ContaId = transacao.ContaId;
                transacaoAtual.TipoCategoria = categoria.TipoCategoria;
                transacaoAtual.Descricao = transacao.Descricao.Trim();
                transacaoAtual.Valor = transacao.Valor;
                transacaoAtual.Data = transacao.Data;
                transacaoAtual.Observacao = NormalizarObservacao(transacao.Observacao);

                novaConta.AtualizarSaldo(transacaoAtual.ObterImpactoNoSaldo());

                await _transacaoRepository.UpdateAsync(transacaoAtual);
                await _contaRepository.UpdateAsync(contaAnterior);

                if (novaConta.Id != contaAnterior.Id)
                {
                    await _contaRepository.UpdateAsync(novaConta);
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task RemoveAsync(int id)
        {
            var usuarioId = _usuarioAtualService.ObterUsuarioId();
            var transacao = await _transacaoRepository.FindFirstAsync(
                item => item.Id == id && item.UsuarioId == usuarioId);

            if (transacao == null)
            {
                throw new RegraNegocioException("Transacao nao encontrada.");
            }

            var conta = await _contaRepository.FindFirstAsync(
                item => item.Id == transacao.ContaId && item.UsuarioId == usuarioId);

            if (conta == null)
            {
                throw new RegraNegocioException("A conta da transacao nao foi encontrada.");
            }

            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                conta.AtualizarSaldo(-transacao.ObterImpactoNoSaldo());
                await _transacaoRepository.RemoveAsync(transacao);
                await _contaRepository.UpdateAsync(conta);
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task<(Conta Conta, Categoria Categoria)> ValidarRelacionamentosAsync(
            Transacao transacao,
            int usuarioId)
        {
            ValidarDados(transacao);

            var conta = await _contaRepository.FindFirstAsync(
                item => item.Id == transacao.ContaId && item.UsuarioId == usuarioId);

            if (conta == null)
            {
                throw new RegraNegocioException("A conta informada nao pertence ao usuario autenticado.");
            }

            if (!conta.Ativa)
            {
                throw new RegraNegocioException("Nao e permitido movimentar uma conta inativa.");
            }

            var categoria = await _categoriaRepository.FindFirstAsync(
                item => item.Id == transacao.CategoriaId && item.UsuarioId == usuarioId);
            if (categoria == null)
            {
                throw new RegraNegocioException(
                    "A categoria informada nao pertence ao usuario autenticado.");
            }

            return (conta, categoria);
        }

        private static void PrepararTransacao(
            Transacao transacao,
            int usuarioId,
            Categoria categoria)
        {
            transacao.UsuarioId = usuarioId;
            transacao.TipoCategoria = categoria.TipoCategoria;
            transacao.Descricao = transacao.Descricao.Trim();
            transacao.Observacao = NormalizarObservacao(transacao.Observacao);
        }

        private static void ValidarDados(Transacao transacao)
        {
            if (transacao.ContaId <= 0)
            {
                throw new RegraNegocioException("Informe uma conta.");
            }

            if (transacao.CategoriaId <= 0)
            {
                throw new RegraNegocioException("Informe uma categoria.");
            }

            if (string.IsNullOrWhiteSpace(transacao.Descricao)
                || transacao.Descricao.Trim().Length is < 3 or > 200)
            {
                throw new RegraNegocioException("A descricao deve ter entre 3 e 200 caracteres.");
            }

            if (transacao.Valor <= 0)
            {
                throw new RegraNegocioException("O valor da transacao deve ser maior que zero.");
            }

            if (transacao.Data == default)
            {
                throw new RegraNegocioException("Informe a data da transacao.");
            }

            if (transacao.Observacao?.Length > 500)
            {
                throw new RegraNegocioException("A observacao deve ter no maximo 500 caracteres.");
            }
        }

        private static string? NormalizarObservacao(string? observacao)
        {
            return string.IsNullOrWhiteSpace(observacao) ? null : observacao.Trim();
        }
    }
}
