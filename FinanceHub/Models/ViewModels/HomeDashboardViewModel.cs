namespace FinanceHub.Models.ViewModels
{
    public class HomeDashboardViewModel
    {
        public decimal SaldoTotal { get; set; }
        public decimal ReceitasMes { get; set; }
        public decimal DespesasMes { get; set; }
        public decimal ResultadoMes { get; set; }
        public decimal GastoMedioMes { get; set; }
        public int LancamentosMes { get; set; }
        public int ContasAtivas { get; set; }
        public int RecorrenciasAtivas { get; set; }
        public int MetasAtivas { get; set; }
        public decimal ProgressoMetasMedio { get; set; }
        public string PeriodoReferencia { get; set; } = string.Empty;
        public string MaiorGastoDescricao { get; set; } = "Sem despesas no periodo";
        public decimal MaiorGastoValor { get; set; }
        public List<HomeMonthlyFlowViewModel> FluxoMensal { get; set; } = new();
        public List<HomeCategoryExpenseViewModel> DespesasPorCategoria { get; set; } = new();
        public List<HomeRecentTransactionViewModel> LancamentosRecentes { get; set; } = new();
    }

    public class HomeMonthlyFlowViewModel
    {
        public string Mes { get; set; } = string.Empty;
        public decimal Receitas { get; set; }
        public decimal Despesas { get; set; }
        public decimal Resultado => Receitas - Despesas;
    }

    public class HomeCategoryExpenseViewModel
    {
        public string Categoria { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public decimal Percentual { get; set; }
        public string Cor { get; set; } = "#4a7a68";
    }

    public class HomeRecentTransactionViewModel
    {
        public string Descricao { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public string Conta { get; set; } = string.Empty;
        public DateTime Data { get; set; }
        public decimal Valor { get; set; }
        public bool EhReceita { get; set; }
    }
}
