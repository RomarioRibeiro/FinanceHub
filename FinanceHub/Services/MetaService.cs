using FinanceHub.Models;
using FinanceHub.Models.Enums;
using FinanceHub.Models.Exceptions;
using FinanceHub.Models.Options;
using FinanceHub.Repositories;
using Microsoft.Extensions.Options;

namespace FinanceHub.Services
{
    public class MetaService
    {
        private readonly IRepository<Meta> _repository;
        private readonly UsuarioAtualService _usuarioAtualService;
        private readonly EmailOptions _emailOptions;

        public MetaService(
            IRepository<Meta> repository,
            UsuarioAtualService usuarioAtualService,
            IOptions<EmailOptions> emailOptions)
        {
            _repository = repository;
            _usuarioAtualService = usuarioAtualService;
            _emailOptions = emailOptions.Value;
        }

        public bool EmailEstaConfigurado() => _emailOptions.EstaConfigurado();

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
            meta.Ativa = true;
            Validar(meta);
            PrepararLembrete(meta);
            meta.UsuarioId = _usuarioAtualService.ObterUsuarioId();
            meta.Nome = meta.Nome.Trim();
            meta.ValorAtual = 0;
            await _repository.InsertAsync(meta);
        }

        public async Task Update(Meta meta)
        {
            Validar(meta);
            PrepararLembrete(meta);
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
            metaAtual.LembreteAtivo = meta.LembreteAtivo;
            metaAtual.CanalLembrete = meta.CanalLembrete;
            metaAtual.FrequenciaLembrete = meta.FrequenciaLembrete;
            metaAtual.ProximoLembreteEm = meta.ProximoLembreteEm;
            metaAtual.DiaReferenciaLembrete = meta.DiaReferenciaLembrete;
            metaAtual.MensagemLembrete = meta.MensagemLembrete;
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

            if (meta.MensagemLembrete?.Trim().Length > 300)
            {
                throw new RegraNegocioException(
                    "A mensagem do lembrete deve ter no maximo 300 caracteres.");
            }
        }

        private void PrepararLembrete(Meta meta)
        {
            meta.MensagemLembrete = string.IsNullOrWhiteSpace(meta.MensagemLembrete)
                ? null
                : meta.MensagemLembrete.Trim();

            if (!meta.Ativa || !meta.LembreteAtivo)
            {
                meta.LembreteAtivo = false;
                meta.ProximoLembreteEm = null;
                return;
            }

            if (!meta.CanalLembrete.HasValue
                || !Enum.IsDefined(meta.CanalLembrete.Value))
            {
                throw new RegraNegocioException("Informe como deseja receber o lembrete.");
            }

            if (!meta.FrequenciaLembrete.HasValue
                || !Enum.IsDefined(meta.FrequenciaLembrete.Value))
            {
                throw new RegraNegocioException("Informe a frequencia do lembrete.");
            }

            if (!meta.ProximoLembreteEm.HasValue)
            {
                throw new RegraNegocioException("Informe a data do primeiro lembrete.");
            }

            if (meta.ProximoLembreteEm.Value < meta.DataInicio
                || meta.ProximoLembreteEm.Value > meta.DataFim)
            {
                throw new RegraNegocioException(
                    "O primeiro lembrete deve estar dentro do periodo da meta.");
            }

            if (meta.CanalLembrete == CanalLembreteMeta.EMAIL
                && !_emailOptions.EstaConfigurado())
            {
                throw new RegraNegocioException(
                    "O envio por e-mail ainda nao foi configurado no servidor.");
            }

            meta.DiaReferenciaLembrete =
                meta.FrequenciaLembrete == FrequenciaLembreteMeta.MENSAL
                    ? meta.ProximoLembreteEm.Value.Day
                    : null;
        }
    }
}
