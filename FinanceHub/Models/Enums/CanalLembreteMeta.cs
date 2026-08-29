using System.ComponentModel.DataAnnotations;

namespace FinanceHub.Models.Enums
{
    public enum CanalLembreteMeta
    {
        [Display(Name = "Dentro do FinanceHub")]
        SISTEMA = 1,

        [Display(Name = "E-mail")]
        EMAIL = 2
    }
}
