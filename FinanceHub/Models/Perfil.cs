using System.Collections.Generic;

namespace FinanceHub.Models
{
    public class Perfil
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();

        public bool EstaNoPerfil(string perfilNome)
        {
            return string.Equals(Nome, perfilNome, StringComparison.OrdinalIgnoreCase);
        }
    }
}
