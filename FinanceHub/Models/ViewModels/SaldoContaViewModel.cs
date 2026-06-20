using FinanceHub.Models.Enums;

namespace FinanceHub.Models.ViewModels
{
    public class SaldoContaViewModel
    {
        public int ContaId { get; set; }
        public string ContaNome { get; set; } = string.Empty;
        public TipoConta TipoConta { get; set; }
        public decimal SaldoInicial { get; set; }
        public decimal SaldoAtual { get; set; }
        public bool Ativa { get; set; }
    }
}
