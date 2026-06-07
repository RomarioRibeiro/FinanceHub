using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FinanceHub.Models.Enums
{
    public enum TipoCategoria : int
    {
        [Display(Name = "Receita")]
        RECEITA = 0,
        [Display(Name = "Despesa")]
        DESPESA = 1
    }
}
