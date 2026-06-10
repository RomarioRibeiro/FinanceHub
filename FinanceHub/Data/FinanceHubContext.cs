using FinanceHub.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace FinanceHub.Data
{
    public class FinanceHubContext : DbContext
    {
        public FinanceHubContext(DbContextOptions<FinanceHubContext> options)
            : base(options)
        {
        }

        public DbSet<Usuario> Usuario { get; set; } = default!;
        public DbSet<Perfil> Perfil { get; set; } = default!;
        public DbSet<Categoria> Categoria { get; set; } = default!;
        public DbSet<Conta> Conta { get; set; } = default!;
        public DbSet<Saldo> Saldo { get; set; } = default!;
        public DbSet<Transacao> Transacao { get; set; } = default!;
        public DbSet<Meta> Meta { get; set; } = default!;
        public DbSet<LancamentoRecorrente> LancamentoRecorrente { get; set; } = default!;
        public DbSet<Relatorio> Relatorio { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("Usuario");
                entity.Property(e => e.Nome).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Email).HasMaxLength(150).IsRequired();
                entity.Property(e => e.SenhaHash).HasMaxLength(128).IsRequired();

                entity.HasMany(e => e.Perfis)
                    .WithMany(e => e.Usuarios)
                    .UsingEntity<Dictionary<string, object>>(
                        "usuario_perfil",
                        r => r.HasOne<Perfil>()
                            .WithMany()
                            .HasForeignKey("PerfilId")
                            .HasConstraintName("FK_usuario_perfil_Perfil_PerfilId"),
                        l => l.HasOne<Usuario>()
                            .WithMany()
                            .HasForeignKey("UsuarioId")
                            .HasConstraintName("FK_usuario_perfil_Usuario_UsuarioId"),
                        j =>
                        {
                            j.ToTable("usuario_perfil");
                            j.HasKey("UsuarioId", "PerfilId");
                        });
            });

            modelBuilder.Entity<Perfil>(entity =>
            {
                entity.ToTable("Perfil");
                entity.Property(e => e.Nome).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Descricao).HasMaxLength(200).IsRequired();
            });

            modelBuilder.Entity<Categoria>(entity =>
            {
                entity.ToTable("Categoria");
                entity.Property(e => e.Nome).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Descricao).IsRequired();
                entity.Property(e => e.TipoCategoria).HasConversion<int>();
            });

            modelBuilder.Entity<Conta>(entity =>
            {
                entity.ToTable("Conta");
                entity.Property(e => e.Nome).HasMaxLength(100).IsRequired();
                entity.Property(e => e.SaldoInicial).HasPrecision(18, 2);
                entity.Property(e => e.SaldoAtual).HasPrecision(18, 2);

                entity.HasOne(e => e.Usuario)
                    .WithMany(e => e.Contas)
                    .HasForeignKey(e => e.UsuarioId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Saldo>(entity =>
            {
                entity.ToTable("Saldo");
                entity.Property(e => e.Valor).HasPrecision(18, 2);

                entity.HasOne(e => e.Conta)
                    .WithMany(e => e.Saldos)
                    .HasForeignKey(e => e.ContaId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Transacao>(entity =>
            {
                entity.ToTable("Transacao");
                entity.Property(e => e.Descricao).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Valor).HasPrecision(18, 2);

                entity.HasOne(e => e.Usuario)
                    .WithMany(e => e.Transacoes)
                    .HasForeignKey(e => e.UsuarioId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Categoria)
                    .WithMany(e => e.Transacoes)
                    .HasForeignKey(e => e.CategoriaId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Conta)
                    .WithMany(e => e.Transacoes)
                    .HasForeignKey(e => e.ContaId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Meta>(entity =>
            {
                entity.ToTable("Meta");
                entity.Property(e => e.Nome).HasMaxLength(100).IsRequired();
                entity.Property(e => e.ValorAlvo).HasPrecision(18, 2);
                entity.Property(e => e.ValorAtual).HasPrecision(18, 2);

                entity.HasOne(e => e.Usuario)
                    .WithMany(e => e.Metas)
                    .HasForeignKey(e => e.UsuarioId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<LancamentoRecorrente>(entity =>
            {
                entity.ToTable("LancamentoRecorrente");
                entity.Property(e => e.Descricao).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Valor).HasPrecision(18, 2);

                entity.HasOne(e => e.Usuario)
                    .WithMany(e => e.LancamentosRecorrentes)
                    .HasForeignKey(e => e.UsuarioId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Categoria)
                    .WithMany(e => e.LancamentosRecorrentes)
                    .HasForeignKey(e => e.CategoriaId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(e => e.Frequencia).HasConversion<int>();
                entity.Property(e => e.Tipo).HasConversion<int>();
            });

            modelBuilder.Entity<Relatorio>(entity =>
            {
                entity.ToTable("Relatorio");
                entity.Property(e => e.Tipo).HasConversion<int>();

                entity.HasOne(e => e.Usuario)
                    .WithMany(e => e.Relatorios)
                    .HasForeignKey(e => e.UsuarioId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
