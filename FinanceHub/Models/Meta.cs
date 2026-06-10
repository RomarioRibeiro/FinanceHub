namespace FinanceHub.Models
{
    public class Meta
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public decimal ValorAlvo { get; set; }
        public decimal ValorAtual { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        public bool Ativa { get; set; }

        public Usuario Usuario { get; set; } = null!;

        public Meta()
        {
            DataInicio = DateTime.Now;
            Ativa = true;
        }

        public decimal CalcularProgresso()
        {
            if (ValorAlvo <= 0)
            {
                return 0;
            }

            var progresso = (ValorAtual / ValorAlvo) * 100m;
            return progresso > 100m ? 100m : progresso;
        }

        public bool EstaConcluida()
        {
            return ValorAlvo > 0 && ValorAtual >= ValorAlvo;
        }
    }
}
