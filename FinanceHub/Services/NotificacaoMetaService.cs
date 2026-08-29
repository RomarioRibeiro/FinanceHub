using FinanceHub.Models;
using FinanceHub.Models.Enums;
using FinanceHub.Models.Exceptions;
using FinanceHub.Repositories;

namespace FinanceHub.Services
{
    public class NotificacaoMetaService
    {
        private readonly IRepository<NotificacaoMeta> _repository;
        private readonly UsuarioAtualService _usuarioAtualService;

        public NotificacaoMetaService(
            IRepository<NotificacaoMeta> repository,
            UsuarioAtualService usuarioAtualService)
        {
            _repository = repository;
            _usuarioAtualService = usuarioAtualService;
        }

        public Task<List<NotificacaoMeta>> FindAllAsync()
        {
            var usuarioId = _usuarioAtualService.ObterUsuarioId();
            return _repository.FindAllAsync(
                item => item.UsuarioId == usuarioId
                    && item.Canal == CanalLembreteMeta.SISTEMA,
                item => item.Meta);
        }

        public async Task<int> ContarNaoLidasAsync()
        {
            var notificacoes = await FindAllAsync();
            return notificacoes.Count(item => !item.LidaEm.HasValue);
        }

        public async Task MarcarComoLidaAsync(int id)
        {
            var usuarioId = _usuarioAtualService.ObterUsuarioId();
            var notificacao = await _repository.FindFirstAsync(
                item => item.Id == id
                    && item.UsuarioId == usuarioId
                    && item.Canal == CanalLembreteMeta.SISTEMA);

            if (notificacao == null)
            {
                throw new RegraNegocioException("Notificacao nao encontrada.");
            }

            notificacao.LidaEm ??= DateTime.Now;
            await _repository.UpdateAsync(notificacao);
        }

        public async Task MarcarTodasComoLidasAsync()
        {
            var notificacoes = await FindAllAsync();
            var agora = DateTime.Now;

            foreach (var notificacao in notificacoes.Where(item => !item.LidaEm.HasValue))
            {
                notificacao.LidaEm = agora;
                await _repository.UpdateAsync(notificacao);
            }
        }
    }
}
