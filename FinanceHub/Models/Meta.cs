using FinanceHub.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace FinanceHub.Models
{
    public class Meta
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        [Required(ErrorMessage = "Informe o nome da meta.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 100 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        [Range(
            typeof(decimal),
            "0.01",
            "9999999999999999",
            ParseLimitsInInvariantCulture = true,
            ConvertValueInInvariantCulture = true,
            ErrorMessage = "O valor alvo deve ser maior que zero.")]
        public decimal ValorAlvo { get; set; }

        [Range(
            typeof(decimal),
            "0",
            "9999999999999999",
            ParseLimitsInInvariantCulture = true,
            ConvertValueInInvariantCulture = true,
            ErrorMessage = "O valor atual nao pode ser negativo.")]
        public decimal ValorAtual { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        public bool Ativa { get; set; }
        public bool LembreteAtivo { get; set; }
        public CanalLembreteMeta? CanalLembrete { get; set; }
        public FrequenciaLembreteMeta? FrequenciaLembrete { get; set; }
        public DateTime? ProximoLembreteEm { get; set; }
        public int? DiaReferenciaLembrete { get; set; }

        [StringLength(300, ErrorMessage = "A mensagem deve ter no maximo 300 caracteres.")]
        public string? MensagemLembrete { get; set; }

        [ValidateNever]
        public Usuario Usuario { get; set; } = null!;
        [ValidateNever]
        public ICollection<NotificacaoMeta> Notificacoes { get; set; } = new List<NotificacaoMeta>();

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
                LembreteAtivo = false;
                ProximoLembreteEm = null;
            }
        }

        public DateTime CalcularProximoLembrete(DateTime referencia)
        {
            if (!FrequenciaLembrete.HasValue)
            {
                throw new InvalidOperationException("A frequencia do lembrete nao foi informada.");
            }

            return FrequenciaLembrete.Value switch
            {
                FrequenciaLembreteMeta.DIARIA => referencia.AddDays(1),
                FrequenciaLembreteMeta.MENSAL => AdicionarMesPreservandoDia(
                    referencia,
                    DiaReferenciaLembrete ?? referencia.Day),
                _ => throw new InvalidOperationException("Frequencia de lembrete invalida.")
            };
        }

        public string ObterMensagemLembrete()
        {
            if (!string.IsNullOrWhiteSpace(MensagemLembrete))
            {
                return MensagemLembrete.Trim();
            }

            var valorRestante = Math.Max(0, ValorAlvo - ValorAtual);
            var valorFormatado = valorRestante.ToString(
                "C",
                CultureInfo.GetCultureInfo("pt-BR"));
            return $"Continue sua meta {Nome}. Voce ja alcancou {CalcularProgresso():0.0}% e faltam {valorFormatado}.";
        }

        private static DateTime AdicionarMesPreservandoDia(
            DateTime referencia,
            int diaReferencia)
        {
            var primeiroProximoMes = new DateTime(
                referencia.Year,
                referencia.Month,
                1,
                referencia.Hour,
                referencia.Minute,
                referencia.Second,
                referencia.Kind).AddMonths(1);
            var dia = Math.Min(diaReferencia, DateTime.DaysInMonth(
                primeiroProximoMes.Year,
                primeiroProximoMes.Month));

            return new DateTime(
                primeiroProximoMes.Year,
                primeiroProximoMes.Month,
                dia,
                referencia.Hour,
                referencia.Minute,
                referencia.Second,
                referencia.Kind);
        }
    }
}
