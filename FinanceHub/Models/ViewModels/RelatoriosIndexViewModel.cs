namespace FinanceHub.Models.ViewModels
{
    public class RelatoriosIndexViewModel
    {
        public IReadOnlyList<Relatorio> Relatorios { get; set; } = Array.Empty<Relatorio>();
        public RelatorioImportadoViewModel? Importacao { get; set; }
    }

    public class RelatorioImportadoViewModel
    {
        public string NomeArquivo { get; set; } = string.Empty;
        public int LinhasLidas { get; set; }
        public int LinhasSemValor { get; set; }
        public IReadOnlyList<LancamentoRelatorioImportadoViewModel> Lancamentos { get; set; }
            = Array.Empty<LancamentoRelatorioImportadoViewModel>();
        public IReadOnlyList<ResumoMensalImportadoViewModel> ResumosMensais { get; set; }
            = Array.Empty<ResumoMensalImportadoViewModel>();
        public IReadOnlyList<CustoImportadoViewModel> MaioresCustos { get; set; }
            = Array.Empty<CustoImportadoViewModel>();

        public decimal TotalReceitas => Lancamentos
            .Where(item => item.Tipo == "Receita")
            .Sum(item => item.Valor);

        public decimal TotalDespesas => Lancamentos
            .Where(item => item.Tipo == "Despesa")
            .Sum(item => item.Valor);

        public decimal Resultado => TotalReceitas - TotalDespesas;
    }

    public class LancamentoRelatorioImportadoViewModel
    {
        public DateTime Mes { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public decimal Valor { get; set; }
    }

    public class ResumoMensalImportadoViewModel
    {
        public DateTime Mes { get; set; }
        public decimal Receitas { get; set; }
        public decimal Despesas { get; set; }
        public decimal Resultado => Receitas - Despesas;
    }

    public class CustoImportadoViewModel
    {
        public string Descricao { get; set; } = string.Empty;
        public decimal Valor { get; set; }
    }
}
