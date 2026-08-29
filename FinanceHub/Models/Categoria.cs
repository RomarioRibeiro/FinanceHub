using FinanceHub.Models.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace FinanceHub.Models
{
    public class Categoria
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }

        [Required(ErrorMessage = "{0} é Obrigatorio")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "{0} Tamanho de caracteres e entre {2} a {1}")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "{0} é Obrigatorio")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "{0} Tamanho de caracteres e entre {2} a {1}")]
        public string Descricao { get; set; } = string.Empty;

        public TipoCategoria TipoCategoria { get; set; }

        public Usuario Usuario { get; set; } = null!;
        public ICollection<Transacao> Transacoes { get; set; } = new List<Transacao>();
        public ICollection<LancamentoRecorrente> LancamentosRecorrentes { get; set; } = new List<LancamentoRecorrente>();

        public Categoria()
        {
        }

        public Categoria(int id, string nome, string descricao, TipoCategoria tipoCategoria)
        {
            Id = id;
            Nome = nome;
            Descricao = descricao;
            TipoCategoria = tipoCategoria;
        }

        public bool Validar()
        {
            return !string.IsNullOrWhiteSpace(Nome)
                && Nome.Length >= 3
                && Nome.Length <= 50
                && !string.IsNullOrWhiteSpace(Descricao)
                && Descricao.Length >= 3
                && Descricao.Length <= 50;
        }

        public bool EstaAssociada()
        {
            return Transacoes.Any() || LancamentosRecorrentes.Any();
        }
    }
}
