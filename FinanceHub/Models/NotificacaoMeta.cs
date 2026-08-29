using FinanceHub.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace FinanceHub.Models
{
    public class NotificacaoMeta
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public int MetaId { get; set; }
        public CanalLembreteMeta Canal { get; set; }

        [StringLength(500)]
        public string Mensagem { get; set; } = string.Empty;

        public DateTime DataAgendada { get; set; }
        public DateTime CriadaEm { get; set; }
        public DateTime? ProcessadaEm { get; set; }
        public DateTime? EnviadaEm { get; set; }
        public DateTime? LidaEm { get; set; }
        public int Tentativas { get; set; }

        [StringLength(500)]
        public string? UltimoErro { get; set; }

        public Usuario Usuario { get; set; } = null!;
        public Meta Meta { get; set; } = null!;

        public bool FoiEnviada() => EnviadaEm.HasValue;
        public bool FoiLida() => LidaEm.HasValue;
    }
}
