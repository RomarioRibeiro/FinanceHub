using System;
using FinanceHub.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceHub.Migrations
{
    [DbContext(typeof(FinanceHubContext))]
    [Migration("20260619120000_RecorrenciaContaUltimaGeracao")]
    public partial class RecorrenciaContaUltimaGeracao : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ContaId",
                table: "LancamentoRecorrente",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UltimaGeracaoEm",
                table: "LancamentoRecorrente",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LancamentoRecorrente_ContaId",
                table: "LancamentoRecorrente",
                column: "ContaId");

            migrationBuilder.AddForeignKey(
                name: "FK_LancamentoRecorrente_Conta_ContaId",
                table: "LancamentoRecorrente",
                column: "ContaId",
                principalTable: "Conta",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LancamentoRecorrente_Conta_ContaId",
                table: "LancamentoRecorrente");

            migrationBuilder.DropIndex(
                name: "IX_LancamentoRecorrente_ContaId",
                table: "LancamentoRecorrente");

            migrationBuilder.DropColumn(
                name: "ContaId",
                table: "LancamentoRecorrente");

            migrationBuilder.DropColumn(
                name: "UltimaGeracaoEm",
                table: "LancamentoRecorrente");
        }
    }
}
