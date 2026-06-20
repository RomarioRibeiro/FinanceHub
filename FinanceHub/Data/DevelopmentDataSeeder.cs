using FinanceHub.Models;
using FinanceHub.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FinanceHub.Data
{
    public static class DevelopmentDataSeeder
    {
        public static async Task SeedAsync(FinanceHubContext context)
        {
            await context.Database.MigrateAsync();

            if (await context.Usuario.AnyAsync())
            {
                return;
            }

            var perfilAdmin = new Perfil
            {
                Nome = "Administrador",
                Descricao = "Acesso administrativo"
            };

            var perfilUsuario = new Perfil
            {
                Nome = "Usuario",
                Descricao = "Acesso padrao"
            };

            var usuario = new Usuario
            {
                Nome = "Administrador Dev",
                Email = "admin@financehub.local",
                Ativo = true
            };

            var passwordHasher = new PasswordHasher<Usuario>();
            usuario.SenhaHash = passwordHasher.HashPassword(usuario, "Admin@123");
            usuario.Perfis.Add(perfilAdmin);
            usuario.Perfis.Add(perfilUsuario);

            var categoriaDespesa = new Categoria
            {
                Nome = "Alimentacao",
                Descricao = "Gastos com refeicoes",
                TipoCategoria = TipoCategoria.DESPESA
            };

            var categoriaReceita = new Categoria
            {
                Nome = "Salario",
                Descricao = "Receitas recorrentes",
                TipoCategoria = TipoCategoria.RECEITA
            };

            var conta = new Conta
            {
                Usuario = usuario,
                Nome = "Conta Principal",
                Tipo = TipoConta.CONTA_CORRENTE,
                Ativa = true
            };
            conta.DefinirSaldoInicial(2500m);

            var transacao = new Transacao
            {
                Usuario = usuario,
                Categoria = categoriaDespesa,
                Conta = conta,
                TipoCategoria = TipoCategoria.DESPESA,
                Descricao = "Mercado inicial",
                Valor = 125.40m,
                Data = DateTime.Now.AddDays(-2),
                Observacao = "Seed de desenvolvimento"
            };
            conta.AtualizarSaldo(transacao.ObterImpactoNoSaldo());

            var meta = new Meta
            {
                Usuario = usuario,
                Nome = "Reserva de emergencia",
                ValorAlvo = 10000m,
                ValorAtual = 2500m,
                DataInicio = DateTime.Today.AddDays(-30),
                DataFim = DateTime.Today.AddMonths(6),
                Ativa = true
            };

            var recorrencia = new LancamentoRecorrente
            {
                Usuario = usuario,
                Categoria = categoriaReceita,
                Conta = conta,
                Descricao = "Salario mensal",
                Valor = 5000m,
                Tipo = TipoCategoria.RECEITA,
                DataInicial = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1),
                Frequencia = FrequenciaRecorrencia.MENSAL,
                DiaReferencia = 1,
                Ativo = true
            };

            var relatorio = new Relatorio
            {
                Usuario = usuario,
                Tipo = TipoRelatorio.RESUMO_MENSAL,
                DataInicio = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1),
                DataFim = DateTime.Today
            };

            context.AddRange(
                perfilAdmin,
                perfilUsuario,
                usuario,
                categoriaDespesa,
                categoriaReceita,
                conta,
                transacao,
                meta,
                recorrencia,
                relatorio);

            await context.SaveChangesAsync();
        }
    }
}
