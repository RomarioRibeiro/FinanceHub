using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceHub.Migrations
{
    /// <inheritdoc />
    public partial class LembretesMetas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CanalLembrete",
                table: "Meta",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DiaReferenciaLembrete",
                table: "Meta",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FrequenciaLembrete",
                table: "Meta",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "LembreteAtivo",
                table: "Meta",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MensagemLembrete",
                table: "Meta",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProximoLembreteEm",
                table: "Meta",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "NotificacaoMeta",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    MetaId = table.Column<int>(type: "int", nullable: false),
                    Canal = table.Column<int>(type: "int", nullable: false),
                    Mensagem = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DataAgendada = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CriadaEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessadaEm = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EnviadaEm = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LidaEm = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Tentativas = table.Column<int>(type: "int", nullable: false),
                    UltimoErro = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificacaoMeta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificacaoMeta_Meta_MetaId",
                        column: x => x.MetaId,
                        principalTable: "Meta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NotificacaoMeta_Usuario_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NotificacaoMeta_MetaId_DataAgendada",
                table: "NotificacaoMeta",
                columns: new[] { "MetaId", "DataAgendada" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NotificacaoMeta_UsuarioId",
                table: "NotificacaoMeta",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NotificacaoMeta");

            migrationBuilder.DropColumn(
                name: "CanalLembrete",
                table: "Meta");

            migrationBuilder.DropColumn(
                name: "DiaReferenciaLembrete",
                table: "Meta");

            migrationBuilder.DropColumn(
                name: "FrequenciaLembrete",
                table: "Meta");

            migrationBuilder.DropColumn(
                name: "LembreteAtivo",
                table: "Meta");

            migrationBuilder.DropColumn(
                name: "MensagemLembrete",
                table: "Meta");

            migrationBuilder.DropColumn(
                name: "ProximoLembreteEm",
                table: "Meta");
        }
    }
}
