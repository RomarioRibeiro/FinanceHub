using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceHub.Migrations
{
    /// <inheritdoc />
    public partial class CategoriasPorUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UsuarioId",
                table: "Categoria",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(
                """
                IF EXISTS (SELECT 1 FROM [Categoria])
                   AND NOT EXISTS (SELECT 1 FROM [Usuario])
                BEGIN
                    THROW 50001, N'Nao e possivel distribuir categorias sem usuarios cadastrados.', 1;
                END;

                DECLARE @CategoriasOriginais TABLE
                (
                    [Id] int NOT NULL PRIMARY KEY,
                    [Nome] nvarchar(50) NOT NULL,
                    [Descricao] nvarchar(50) NOT NULL,
                    [TipoCategoria] int NOT NULL
                );

                INSERT INTO @CategoriasOriginais ([Id], [Nome], [Descricao], [TipoCategoria])
                SELECT [Id], [Nome], [Descricao], [TipoCategoria]
                FROM [Categoria]
                WHERE [UsuarioId] IS NULL;

                DECLARE @CategoriasPorUsuario TABLE
                (
                    [UsuarioId] int NOT NULL,
                    [CategoriaAntigaId] int NOT NULL,
                    [CategoriaNovaId] int NOT NULL,
                    PRIMARY KEY ([UsuarioId], [CategoriaAntigaId])
                );

                MERGE [Categoria] AS destino
                USING
                (
                    SELECT
                        usuario.[Id] AS [UsuarioId],
                        categoria.[Id] AS [CategoriaAntigaId],
                        categoria.[Nome],
                        categoria.[Descricao],
                        categoria.[TipoCategoria]
                    FROM [Usuario] AS usuario
                    CROSS JOIN @CategoriasOriginais AS categoria
                ) AS origem
                ON 1 = 0
                WHEN NOT MATCHED BY TARGET THEN
                    INSERT ([UsuarioId], [Nome], [Descricao], [TipoCategoria])
                    VALUES (
                        origem.[UsuarioId],
                        origem.[Nome],
                        origem.[Descricao],
                        origem.[TipoCategoria])
                OUTPUT
                    origem.[UsuarioId],
                    origem.[CategoriaAntigaId],
                    inserted.[Id]
                INTO @CategoriasPorUsuario (
                    [UsuarioId],
                    [CategoriaAntigaId],
                    [CategoriaNovaId]);

                UPDATE transacao
                SET transacao.[CategoriaId] = categoria.[CategoriaNovaId]
                FROM [Transacao] AS transacao
                INNER JOIN @CategoriasPorUsuario AS categoria
                    ON categoria.[UsuarioId] = transacao.[UsuarioId]
                    AND categoria.[CategoriaAntigaId] = transacao.[CategoriaId];

                UPDATE recorrencia
                SET recorrencia.[CategoriaId] = categoria.[CategoriaNovaId]
                FROM [LancamentoRecorrente] AS recorrencia
                INNER JOIN @CategoriasPorUsuario AS categoria
                    ON categoria.[UsuarioId] = recorrencia.[UsuarioId]
                    AND categoria.[CategoriaAntigaId] = recorrencia.[CategoriaId];

                DELETE categoria
                FROM [Categoria] AS categoria
                INNER JOIN @CategoriasOriginais AS original
                    ON original.[Id] = categoria.[Id];
                """);

            migrationBuilder.AlterColumn<int>(
                name: "UsuarioId",
                table: "Categoria",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categoria_UsuarioId",
                table: "Categoria",
                column: "UsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Categoria_Usuario_UsuarioId",
                table: "Categoria",
                column: "UsuarioId",
                principalTable: "Usuario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categoria_Usuario_UsuarioId",
                table: "Categoria");

            migrationBuilder.DropIndex(
                name: "IX_Categoria_UsuarioId",
                table: "Categoria");

            migrationBuilder.DropColumn(
                name: "UsuarioId",
                table: "Categoria");
        }
    }
}
