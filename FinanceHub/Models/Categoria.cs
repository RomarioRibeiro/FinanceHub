using FinanceHub.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace FinanceHub.Models
{
    public class Categoria
    {
        public int Id { get; set; }

        [Required(ErrorMessage ="{0} é Obrigatorio")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "{0} Tamanho de caracteres e entre {2} a {1}")]
        public string Nome { get; set; }


        [Required(ErrorMessage = "{0} é Obrigatorio")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "{0} Tamanho de caracteres e entre {2} a {1}")]
        public string Descricao { get; set; }
        public TipoCategoria TipoCategoria { get; set; }

        public Categoria() { }

        public Categoria(int id, string nome, string descricao, TipoCategoria tipoCategoria)
        {
            Id = id;
            Nome = nome;
            Descricao = descricao;
            TipoCategoria = tipoCategoria;
        }
    }
}
