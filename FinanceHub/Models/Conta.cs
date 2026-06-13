using FinanceHub.Models.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FinanceHub.Models
{
    public class Conta
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        [Required(ErrorMessage = "Informe o nome da conta.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 100 caracteres.")]
        public string Nome { get; set; } = string.Empty;
        public TipoConta Tipo { get; set; }
        public decimal SaldoInicial { get; set; }
        public decimal SaldoAtual { get; set; }
        public bool Ativa { get; set; }

        [ValidateNever]
        public Usuario? Usuario { get; set; } = null!;
        public ICollection<Saldo> Saldos { get; set; } = new List<Saldo>();
        public ICollection<Transacao> Transacoes { get; set; } = new List<Transacao>();

        public Conta()
        {
            Ativa = true;
        }

        public void AtualizarSaldo(decimal valor)
        {
            SaldoAtual += valor;
        }

        public void DefinirSaldoInicial(decimal saldoInicial)
        {
            SaldoInicial = saldoInicial;
            SaldoAtual = saldoInicial;
        }

        public bool Validar()
        {
            return UsuarioId > 0
                && !string.IsNullOrWhiteSpace(Nome);
        }
    }
}
