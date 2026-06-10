using FinanceHub.Models.Enums;
using System.Collections.Generic;

namespace FinanceHub.Models
{
    public class Conta
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public TipoConta Tipo { get; set; }
        public decimal SaldoInicial { get; set; }
        public decimal SaldoAtual { get; set; }
        public bool Ativa { get; set; }

        public Usuario Usuario { get; set; } = null!;
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

        public bool Validar()
        {
            return UsuarioId > 0
                && !string.IsNullOrWhiteSpace(Nome);
        }
    }
}
