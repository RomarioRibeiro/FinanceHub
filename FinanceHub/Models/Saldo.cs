namespace FinanceHub.Models
{
    public class Saldo
    {
        public int Id { get; set; }
        public int ContaId { get; set; }
        public DateTime Data { get; set; }
        public decimal Valor { get; set; }

        public Conta Conta { get; set; } = null!;

        public Saldo()
        {
            Data = DateTime.Now;
        }
    }
}
