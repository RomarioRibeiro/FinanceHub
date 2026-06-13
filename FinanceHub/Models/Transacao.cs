using FinanceHub.Models.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
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
        [Required(ErrorMessage = "Informe a descricao.")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "A descricao deve ter entre 3 e 200 caracteres.")]
        public string Descricao { get; set; } = string.Empty;

        [Range(typeof(decimal), "0.01", "9999999999999999", ErrorMessage = "O valor deve ser maior que zero.")]
        public decimal Valor { get; set; }

        [Required(ErrorMessage = "Informe a data.")]
        public DateTime Data { get; set; }

        [StringLength(500, ErrorMessage = "A observacao deve ter no maximo 500 caracteres.")]
        public string? Observacao { get; set; }

        [ValidateNever]
        public Usuario? Usuario { get; set; } = null!;
        [ValidateNever]
        public Categoria? Categoria { get; set; } = null!;
        [ValidateNever]
        public Conta? Conta { get; set; } = null!;

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

        public decimal ObterImpactoNoSaldo()
        {
            return EhReceita() ? Valor : -Valor;
        }
    }
}
