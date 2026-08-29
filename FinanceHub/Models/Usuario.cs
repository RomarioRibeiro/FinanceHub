using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace FinanceHub.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string SenhaHash { get; set; } = string.Empty;
        public DateTime DataCadastro { get; set; }
        public bool Ativo { get; set; }
        public ICollection<Perfil> Perfis { get; set; } = new List<Perfil>();
        public ICollection<Categoria> Categorias { get; set; } = new List<Categoria>();
        public ICollection<Conta> Contas { get; set; } = new List<Conta>();
        public ICollection<Transacao> Transacoes { get; set; } = new List<Transacao>();
        public ICollection<LancamentoRecorrente> LancamentosRecorrentes { get; set; } = new List<LancamentoRecorrente>();
        public ICollection<Meta> Metas { get; set; } = new List<Meta>();
        public ICollection<NotificacaoMeta> NotificacoesMeta { get; set; } = new List<NotificacaoMeta>();
        public ICollection<Relatorio> Relatorios { get; set; } = new List<Relatorio>();

        public Usuario()
        {
            DataCadastro = DateTime.Now;
            Ativo = true;
        }

        public bool Autenticar(string email, string senha)
        {
            return Ativo
                && string.Equals(Email, email, StringComparison.OrdinalIgnoreCase)
                && string.Equals(SenhaHash, GerarHash(senha), StringComparison.OrdinalIgnoreCase);
        }

        public bool AlterarSenha(string senhaAtual, string novaSenha)
        {
            if (!string.Equals(SenhaHash, GerarHash(senhaAtual), StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            SenhaHash = GerarHash(novaSenha);
            return true;
        }

        public void Desativar()
        {
            Ativo = false;
        }

        private static string GerarHash(string senha)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(senha);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToHexString(hash);
        }
    }
}
