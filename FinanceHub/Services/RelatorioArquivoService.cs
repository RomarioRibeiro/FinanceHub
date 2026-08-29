using FinanceHub.Models;
using FinanceHub.Models.Exceptions;
using FinanceHub.Models.ViewModels;
using FinanceHub.Repositories;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace FinanceHub.Services
{
    public partial class RelatorioArquivoService
    {
        private const int MaximoColunasAnalisadas = 6;
        private static readonly CultureInfo CulturaBrasileira = new("pt-BR");
        private readonly IRepository<Transacao> _transacaoRepository;
        private readonly UsuarioAtualService _usuarioAtualService;

        public RelatorioArquivoService(
            IRepository<Transacao> transacaoRepository,
            UsuarioAtualService usuarioAtualService)
        {
            _transacaoRepository = transacaoRepository;
            _usuarioAtualService = usuarioAtualService;
        }

        public async Task<RelatorioImportadoViewModel> AnalisarCsvAsync(
            Stream arquivo,
            string nomeArquivo,
            CancellationToken cancellationToken = default)
        {
            var linhas = await LerCsvAsync(arquivo, cancellationToken);
            var cabecalhos = LocalizarMeses(linhas);

            if (cabecalhos.Count == 0)
            {
                throw new RegraNegocioException(
                    "Nao foi possivel identificar os meses da planilha. Use o modelo mensal com secoes A RECEBER e A PAGAR.");
            }

            PreencherAnos(cabecalhos);
            var lancamentos = new List<LancamentoRelatorioImportadoViewModel>();
            var linhasSemValor = 0;

            for (var indiceMes = 0; indiceMes < cabecalhos.Count; indiceMes++)
            {
                var cabecalho = cabecalhos[indiceMes];
                var limite = indiceMes + 1 < cabecalhos.Count
                    ? cabecalhos[indiceMes + 1].IndiceLinha
                    : linhas.Count;
                string? tipoAtual = null;

                for (var indiceLinha = cabecalho.IndiceLinha + 1; indiceLinha < limite; indiceLinha++)
                {
                    var campos = linhas[indiceLinha];
                    var secao = IdentificarSecao(campos);
                    if (secao != null)
                    {
                        tipoAtual = secao;
                        continue;
                    }

                    if (tipoAtual == null)
                    {
                        continue;
                    }

                    var quantidadeCampos = Math.Min(campos.Count, MaximoColunasAnalisadas);
                    for (var coluna = 0; coluna + 1 < quantidadeCampos; coluna += 2)
                    {
                        var descricao = campos[coluna].Trim();
                        var valorTexto = campos[coluna + 1].Trim();

                        if (!EhDescricaoDeLancamento(descricao))
                        {
                            continue;
                        }

                        if (!TentarConverterValor(valorTexto, out var valor) || valor <= 0)
                        {
                            if (valorTexto.Length > 0 || valorTexto.Equals("PGO", StringComparison.OrdinalIgnoreCase))
                            {
                                linhasSemValor++;
                            }

                            continue;
                        }

                        lancamentos.Add(new LancamentoRelatorioImportadoViewModel
                        {
                            Mes = new DateTime(cabecalho.Ano, cabecalho.Mes, 1),
                            Tipo = tipoAtual,
                            Descricao = descricao,
                            Valor = valor
                        });
                    }
                }
            }

            if (lancamentos.Count == 0)
            {
                throw new RegraNegocioException(
                    "A planilha foi lida, mas nao possui valores monetarios maiores que zero para analisar.");
            }

            var lancamentosOrdenados = lancamentos
                .OrderBy(item => item.Mes)
                .ThenBy(item => item.Tipo)
                .ThenBy(item => item.Descricao)
                .ToList();

            return new RelatorioImportadoViewModel
            {
                NomeArquivo = nomeArquivo,
                LinhasLidas = linhas.Count,
                LinhasSemValor = linhasSemValor,
                Lancamentos = lancamentosOrdenados,
                ResumosMensais = lancamentosOrdenados
                    .GroupBy(item => item.Mes)
                    .Select(grupo => new ResumoMensalImportadoViewModel
                    {
                        Mes = grupo.Key,
                        Receitas = grupo.Where(item => item.Tipo == "Receita").Sum(item => item.Valor),
                        Despesas = grupo.Where(item => item.Tipo == "Despesa").Sum(item => item.Valor)
                    })
                    .OrderBy(item => item.Mes)
                    .ToList(),
                MaioresCustos = lancamentosOrdenados
                    .Where(item => item.Tipo == "Despesa")
                    .GroupBy(item => item.Descricao.Trim(), StringComparer.OrdinalIgnoreCase)
                    .Select(grupo => new CustoImportadoViewModel
                    {
                        Descricao = grupo.First().Descricao,
                        Valor = grupo.Sum(item => item.Valor)
                    })
                    .OrderByDescending(item => item.Valor)
                    .Take(10)
                    .ToList()
            };
        }

        public async Task<byte[]> ExportarTransacoesAsync()
        {
            var usuarioId = _usuarioAtualService.ObterUsuarioId();
            var transacoes = await _transacaoRepository.FindAllAsync(
                item => item.UsuarioId == usuarioId,
                item => item.Categoria!,
                item => item.Conta!);

            await using var memoria = new MemoryStream();
            await using (var escritor = new StreamWriter(
                memoria,
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: true),
                leaveOpen: true))
            {
                await escritor.WriteLineAsync(
                    "Data;Tipo;Categoria;Conta;Descricao;Valor;Observacao");

                foreach (var transacao in transacoes.OrderByDescending(item => item.Data))
                {
                    var campos = new[]
                    {
                        transacao.Data.ToString("dd/MM/yyyy", CulturaBrasileira),
                        transacao.EhReceita() ? "Receita" : "Despesa",
                        transacao.Categoria?.Nome ?? string.Empty,
                        transacao.Conta?.Nome ?? string.Empty,
                        transacao.Descricao,
                        transacao.Valor.ToString("0.00", CulturaBrasileira),
                        transacao.Observacao ?? string.Empty
                    };

                    await escritor.WriteLineAsync(string.Join(';', campos.Select(ProtegerCampoCsv)));
                }
            }

            return memoria.ToArray();
        }

        private static async Task<List<IReadOnlyList<string>>> LerCsvAsync(
            Stream arquivo,
            CancellationToken cancellationToken)
        {
            var linhas = new List<IReadOnlyList<string>>();
            using var leitor = new StreamReader(
                arquivo,
                Encoding.UTF8,
                detectEncodingFromByteOrderMarks: true,
                leaveOpen: true);

            while (await leitor.ReadLineAsync(cancellationToken) is { } linha)
            {
                linhas.Add(SepararLinha(linha));
            }

            return linhas;
        }

        private static List<CabecalhoMes> LocalizarMeses(IReadOnlyList<IReadOnlyList<string>> linhas)
        {
            var cabecalhos = new List<CabecalhoMes>();

            for (var indice = 0; indice < linhas.Count; indice++)
            {
                var primeiroCampo = linhas[indice].FirstOrDefault()?.Trim() ?? string.Empty;
                if (TentarLerMes(primeiroCampo, out var mes, out var ano))
                {
                    cabecalhos.Add(new CabecalhoMes(indice, mes, ano));
                }
            }

            return cabecalhos;
        }

        private static void PreencherAnos(List<CabecalhoMes> cabecalhos)
        {
            var primeiroComAno = cabecalhos.FindIndex(item => item.Ano > 0);
            if (primeiroComAno < 0)
            {
                cabecalhos[0].Ano = DateTime.Today.Year;
                primeiroComAno = 0;
            }

            for (var indice = primeiroComAno - 1; indice >= 0; indice--)
            {
                var proximo = cabecalhos[indice + 1];
                cabecalhos[indice].Ano = proximo.Ano - (cabecalhos[indice].Mes > proximo.Mes ? 1 : 0);
            }

            for (var indice = primeiroComAno + 1; indice < cabecalhos.Count; indice++)
            {
                if (cabecalhos[indice].Ano > 0)
                {
                    continue;
                }

                var anterior = cabecalhos[indice - 1];
                cabecalhos[indice].Ano = anterior.Ano + (cabecalhos[indice].Mes < anterior.Mes ? 1 : 0);
            }
        }

        private static string? IdentificarSecao(IReadOnlyList<string> campos)
        {
            foreach (var campo in campos.Take(MaximoColunasAnalisadas))
            {
                var texto = RemoverAcentos(campo).Trim().ToUpperInvariant();
                if (texto == "A RECEBER")
                {
                    return "Receita";
                }

                if (texto == "A PAGAR")
                {
                    return "Despesa";
                }
            }

            return null;
        }

        private static bool TentarLerMes(string texto, out int mes, out int ano)
        {
            mes = 0;
            ano = 0;
            var normalizado = RemoverAcentos(texto).Trim().ToLowerInvariant();
            var correspondencia = MesRegex().Match(normalizado);
            if (!correspondencia.Success)
            {
                return false;
            }

            var meses = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                ["jan"] = 1, ["janeiro"] = 1,
                ["fev"] = 2, ["fevereiro"] = 2,
                ["mar"] = 3, ["marco"] = 3,
                ["abr"] = 4, ["abril"] = 4,
                ["mai"] = 5, ["maio"] = 5,
                ["jun"] = 6, ["junho"] = 6,
                ["jul"] = 7, ["julho"] = 7,
                ["ago"] = 8, ["agosto"] = 8,
                ["set"] = 9, ["setembro"] = 9,
                ["out"] = 10, ["outubro"] = 10,
                ["nov"] = 11, ["novembro"] = 11,
                ["dez"] = 12, ["dezembro"] = 12
            };

            if (!meses.TryGetValue(correspondencia.Groups["mes"].Value, out mes))
            {
                return false;
            }

            var anoTexto = correspondencia.Groups["ano"].Value;
            if (anoTexto.Length > 0)
            {
                ano = int.Parse(anoTexto, CultureInfo.InvariantCulture);
                if (ano < 100)
                {
                    ano += 2000;
                }
            }

            return true;
        }

        private static bool EhDescricaoDeLancamento(string descricao)
        {
            if (string.IsNullOrWhiteSpace(descricao))
            {
                return false;
            }

            var normalizada = RemoverAcentos(descricao).Trim().ToUpperInvariant();
            return !normalizada.StartsWith("TOTAL", StringComparison.Ordinal)
                && !normalizada.StartsWith("RESULTADO", StringComparison.Ordinal)
                && !normalizada.StartsWith("LIMITE", StringComparison.Ordinal)
                && normalizada is not "A RECEBER" and not "A PAGAR";
        }

        private static bool TentarConverterValor(string texto, out decimal valor)
        {
            valor = 0;
            if (string.IsNullOrWhiteSpace(texto)
                || texto.Equals("PGO", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var normalizado = texto
                .Replace("R$", string.Empty, StringComparison.OrdinalIgnoreCase)
                .Replace("\u00A0", string.Empty, StringComparison.Ordinal)
                .Trim();

            return decimal.TryParse(
                normalizado,
                NumberStyles.Number | NumberStyles.AllowLeadingSign,
                CulturaBrasileira,
                out valor);
        }

        private static string ProtegerCampoCsv(string valor)
        {
            var campo = valor;
            if (campo.Length > 0 && campo[0] is '=' or '+' or '-' or '@')
            {
                campo = "'" + campo;
            }

            if (campo.Contains(';') || campo.Contains('"') || campo.Contains('\n') || campo.Contains('\r'))
            {
                campo = $"\"{campo.Replace("\"", "\"\"")}\"";
            }

            return campo;
        }

        private static IReadOnlyList<string> SepararLinha(string linha)
        {
            var campos = new List<string>();
            var campo = new StringBuilder();
            var dentroDeAspas = false;

            for (var indice = 0; indice < linha.Length; indice++)
            {
                var caractere = linha[indice];
                if (caractere == '"')
                {
                    if (dentroDeAspas && indice + 1 < linha.Length && linha[indice + 1] == '"')
                    {
                        campo.Append('"');
                        indice++;
                    }
                    else
                    {
                        dentroDeAspas = !dentroDeAspas;
                    }
                }
                else if (caractere == ';' && !dentroDeAspas)
                {
                    campos.Add(campo.ToString());
                    campo.Clear();
                }
                else
                {
                    campo.Append(caractere);
                }
            }

            campos.Add(campo.ToString());
            return campos;
        }

        private static string RemoverAcentos(string texto)
        {
            var normalizado = texto.Normalize(NormalizationForm.FormD);
            var resultado = new StringBuilder(normalizado.Length);

            foreach (var caractere in normalizado)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(caractere) != UnicodeCategory.NonSpacingMark)
                {
                    resultado.Append(caractere);
                }
            }

            return resultado.ToString().Normalize(NormalizationForm.FormC);
        }

        [GeneratedRegex("^(?<mes>jan|janeiro|fev|fevereiro|mar|marco|abr|abril|mai|maio|jun|junho|jul|julho|ago|agosto|set|setembro|out|outubro|nov|novembro|dez|dezembro)(?:[\\s/\\-]+(?<ano>\\d{2,4}))?$")]
        private static partial Regex MesRegex();

        private sealed class CabecalhoMes
        {
            public CabecalhoMes(int indiceLinha, int mes, int ano)
            {
                IndiceLinha = indiceLinha;
                Mes = mes;
                Ano = ano;
            }

            public int IndiceLinha { get; }
            public int Mes { get; }
            public int Ano { get; set; }
        }
    }
}
