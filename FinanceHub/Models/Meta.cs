using System.ComponentModel.DataAnnotations;

namespace FinanceHub.Models
{
    public class Meta
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        [Required(ErrorMessage = "Informe o nome da meta.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 100 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        [Range(typeof(decimal), "0.01", "9999999999999999", ErrorMessage = "O valor alvo deve ser maior que zero.")]
        public decimal ValorAlvo { get; set; }

        [Range(typeof(decimal), "0", "9999999999999999", ErrorMessage = "O valor atual nao pode ser negativo.")]
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

        public void AtualizarProgresso(decimal valorAtual)
        {
            ValorAtual = valorAtual;
            if (EstaConcluida())
            {
                Ativa = false;
            }
        }
    }
}
