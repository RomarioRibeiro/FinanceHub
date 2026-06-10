using FinanceHub.Models.Enums;
using TipoCategoriaEnum = FinanceHub.Models.Enums.TipoCategoria;

namespace FinanceHub.Models
{
    public class Transacao
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public int CategoriaId { get; set; }
        public int ContaId { get; set; }
        public TipoCategoria TipoCategoria { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public DateTime Data { get; set; }
        public string? Observacao { get; set; }

        public Usuario Usuario { get; set; } = null!;
        public Categoria Categoria { get; set; } = null!;
        public Conta Conta { get; set; } = null!;

        public Transacao()
        {
            Data = DateTime.Now;
        }

        public bool Validar()
        {
            return UsuarioId > 0
                && CategoriaId > 0
                && ContaId > 0
                && !string.IsNullOrWhiteSpace(Descricao)
                && Valor != 0
                && Data != default;
        }

        public bool EhReceita()
        {
            return TipoCategoria == TipoCategoriaEnum.RECEITA;
        }

        public bool EhDespesa()
        {
            return TipoCategoria == TipoCategoriaEnum.DESPESA;
        }
    }
}
