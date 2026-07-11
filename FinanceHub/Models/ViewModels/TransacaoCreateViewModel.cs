using System.ComponentModel.DataAnnotations;

namespace FinanceHub.Models.ViewModels
{
    public class TransacaoCreateViewModel
    {
        [Required(ErrorMessage = "Informe uma categoria.")]
        public int CategoriaId { get; set; }

        [Required(ErrorMessage = "Informe uma conta.")]
        public int ContaId { get; set; }

        [Required(ErrorMessage = "Informe a descricao.")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "A descricao deve ter entre 3 e 200 caracteres.")]
        public string Descricao { get; set; } = string.Empty;

        [Range(typeof(decimal), "0.01", "9999999999999999",
            ParseLimitsInInvariantCulture = true,
            ConvertValueInInvariantCulture = true,
            ErrorMessage = "O valor deve ser maior que zero.")]
        public decimal Valor { get; set; }

        [Required(ErrorMessage = "Informe a data.")]
        public DateTime Data { get; set; } = DateTime.Now;

        [StringLength(500, ErrorMessage = "A observacao deve ter no maximo 500 caracteres.")]
        public string? Observacao { get; set; }

        [Required(ErrorMessage = "Informe a forma de pagamento.")]
        public string FormaPagamento { get; set; } = "AVISTA";

        [Range(1, 999, ErrorMessage = "A quantidade de parcelas deve estar entre 1 e 999.")]
        public int? QuantidadeParcelas { get; set; }
    }
}
