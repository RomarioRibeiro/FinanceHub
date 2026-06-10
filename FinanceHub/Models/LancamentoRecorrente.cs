using FinanceHub.Models.Enums;

namespace FinanceHub.Models
{
    public class LancamentoRecorrente
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public int CategoriaId { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public TipoCategoria Tipo { get; set; }
        public DateTime DataInicial { get; set; }
        public FrequenciaRecorrencia Frequencia { get; set; }
        public int DiaReferencia { get; set; }
        public bool Ativo { get; set; }

        public Usuario Usuario { get; set; } = null!;
        public Categoria Categoria { get; set; } = null!;

        public LancamentoRecorrente()
        {
            DataInicial = DateTime.Now;
            Ativo = true;
        }

        public Transacao GerarLancamento()
        {
            return new Transacao
            {
                UsuarioId = UsuarioId,
                CategoriaId = CategoriaId,
                ContaId = 0,
                TipoCategoria = Tipo,
                Descricao = Descricao,
                Valor = Valor,
                Data = DateTime.Now,
                Observacao = $"Gerado a partir do lancamento recorrente {Id}"
            };
        }

        public bool EstaAtivo()
        {
            return Ativo;
        }
    }
}
