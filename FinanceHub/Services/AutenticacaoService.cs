using FinanceHub.Models;
using FinanceHub.Repositories;
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;
using System.Text;

namespace FinanceHub.Services
{
    public class AutenticacaoService
    {
        private readonly IRepository<Usuario> _repository;
        private readonly PasswordHasher<Usuario> _passwordHasher = new();

        public AutenticacaoService(IRepository<Usuario> repository)
        {
            _repository = repository;
        }

        public async Task<Usuario?> AutenticarAsync(string email, string senha)
        {
            var emailNormalizado = email.Trim();
            var usuario = await _repository.FindFirstAsync(
                item => item.Email == emailNormalizado,
                item => item.Perfis);

            if (usuario == null || !usuario.Ativo)
            {
                return null;
            }

            var resultado = VerificarSenha(usuario, senha);
            if (resultado == PasswordVerificationResult.Failed)
            {
                return null;
            }

            if (resultado == PasswordVerificationResult.SuccessRehashNeeded)
            {
                usuario.SenhaHash = _passwordHasher.HashPassword(usuario, senha);
                await _repository.UpdateAsync(usuario);
            }

            return usuario;
        }

        private PasswordVerificationResult VerificarSenha(Usuario usuario, string senha)
        {
            try
            {
                var resultado = _passwordHasher.VerifyHashedPassword(usuario, usuario.SenhaHash, senha);
                if (resultado != PasswordVerificationResult.Failed)
                {
                    return resultado;
                }
            }
            catch (FormatException)
            {
                // Credenciais antigas podem nao estar no formato do PasswordHasher.
            }

            if (CompararTextoSeguro(usuario.SenhaHash, GerarSha256(senha))
                || CompararTextoSeguro(usuario.SenhaHash, senha))
            {
                return PasswordVerificationResult.SuccessRehashNeeded;
            }

            return PasswordVerificationResult.Failed;
        }

        private static bool CompararTextoSeguro(string valorAtual, string valorEsperado)
        {
            var atual = Encoding.UTF8.GetBytes(valorAtual);
            var esperado = Encoding.UTF8.GetBytes(valorEsperado);

            return atual.Length == esperado.Length
                && CryptographicOperations.FixedTimeEquals(atual, esperado);
        }

        private static string GerarSha256(string senha)
        {
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(senha));
            return Convert.ToHexString(hash);
        }
    }
}
