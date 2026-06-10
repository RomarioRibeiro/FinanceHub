using FinanceHub.Models.Enums;
using System.Text;

namespace FinanceHub.Models
{
    public class Relatorio
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public TipoRelatorio Tipo { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        public DateTime GeradoEm { get; set; }

        public Usuario Usuario { get; set; } = null!;

        public Relatorio()
        {
            GeradoEm = DateTime.Now;
        }

        public byte[] Gerar()
        {
            var conteudo = $"Relatorio {Tipo} | Usuario {UsuarioId} | Periodo {DataInicio:yyyy-MM-dd} - {DataFim:yyyy-MM-dd}";
            GeradoEm = DateTime.Now;
            return Encoding.UTF8.GetBytes(conteudo);
        }
    }
}
