using FinanceHub.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace FinanceHub.Models
{
    public class LancamentoRecorrente
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public int CategoriaId { get; set; }
        [Required(ErrorMessage = "Informe a descricao.")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "A descricao deve ter entre 3 e 200 caracteres.")]
        public string Descricao { get; set; } = string.Empty;

        [Range(typeof(decimal), "0.01", "9999999999999999", ErrorMessage = "O valor deve ser maior que zero.")]
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

        public Transacao GerarLancamento(int contaId, DateTime data)
        {
            if (contaId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(contaId));
            }

            return new Transacao
            {
                UsuarioId = UsuarioId,
                CategoriaId = CategoriaId,
                ContaId = contaId,
                TipoCategoria = Tipo,
                Descricao = Descricao,
                Valor = Valor,
                Data = data,
                Observacao = $"Gerado a partir do lancamento recorrente {Id}"
            };
        }

        public bool EstaAtivo()
        {
            return Ativo;
        }
    }
}
