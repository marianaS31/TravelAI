using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelAI.Migrations
{
    /// <inheritdoc />
    public partial class AddUtilizadores : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UtilizadorId",
                table: "Viagens",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Utilizadores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Utilizadores", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Viagens_UtilizadorId",
                table: "Viagens",
                column: "UtilizadorId");

            migrationBuilder.CreateIndex(
                name: "IX_Utilizadores_Email",
                table: "Utilizadores",
                column: "Email",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Viagens_Utilizadores_UtilizadorId",
                table: "Viagens",
                column: "UtilizadorId",
                principalTable: "Utilizadores",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Viagens_Utilizadores_UtilizadorId",
                table: "Viagens");

            migrationBuilder.DropTable(
                name: "Utilizadores");

            migrationBuilder.DropIndex(
                name: "IX_Viagens_UtilizadorId",
                table: "Viagens");

            migrationBuilder.DropColumn(
                name: "UtilizadorId",
                table: "Viagens");
        }
    }
}
