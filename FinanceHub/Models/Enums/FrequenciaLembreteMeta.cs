using System.ComponentModel.DataAnnotations;

namespace FinanceHub.Models.Enums
{
    public enum FrequenciaLembreteMeta
    {
        [Display(Name = "Diariamente")]
        DIARIA = 1,

        [Display(Name = "Mensalmente")]
        MENSAL = 2
    }
}
