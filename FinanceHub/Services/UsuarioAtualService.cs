using System.Security.Claims;
using FinanceHub.Models.Exceptions;

namespace FinanceHub.Services
{
    public class UsuarioAtualService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UsuarioAtualService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int ObterUsuarioId()
        {
            var valor = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(valor, out var usuarioId) || usuarioId <= 0)
            {
                throw new RegraNegocioException("Nao foi possivel identificar o usuario autenticado.");
            }

            return usuarioId;
        }
    }
}
