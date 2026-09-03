using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelAI.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Viagens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Destino = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataFim = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NumViajantes = table.Column<int>(type: "int", nullable: false),
                    Orcamento = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Viagens", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Itinerarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Versao = table.Column<int>(type: "int", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ViagemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Itinerarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Itinerarios_Viagens_ViagemId",
                        column: x => x.ViagemId,
                        principalTable: "Viagens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DiasItinerario",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NumeroDia = table.Column<int>(type: "int", nullable: false),
                    Data = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ItinerarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiasItinerario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiasItinerario_Itinerarios_ItinerarioId",
                        column: x => x.ItinerarioId,
                        principalTable: "Itinerarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Atividades",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Ordem = table.Column<int>(type: "int", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    HoraInicio = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HoraFim = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Local = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Detalhes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DiaItinerarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Atividades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Atividades_DiasItinerario_DiaItinerarioId",
                        column: x => x.DiaItinerarioId,
                        principalTable: "DiasItinerario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PrevisoesTempo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TempMax = table.Column<float>(type: "real", nullable: false),
                    TempMin = table.Column<float>(type: "real", nullable: false),
                    Condicao = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProbabilidadePrecipitacao = table.Column<float>(type: "real", nullable: false),
                    DiaItinerarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrevisoesTempo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrevisoesTempo_DiasItinerario_DiaItinerarioId",
                        column: x => x.DiaItinerarioId,
                        principalTable: "DiasItinerario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Atividades_DiaItinerarioId",
                table: "Atividades",
                column: "DiaItinerarioId");

            migrationBuilder.CreateIndex(
                name: "IX_DiasItinerario_ItinerarioId",
                table: "DiasItinerario",
                column: "ItinerarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Itinerarios_ViagemId",
                table: "Itinerarios",
                column: "ViagemId");

            migrationBuilder.CreateIndex(
                name: "IX_PrevisoesTempo_DiaItinerarioId",
                table: "PrevisoesTempo",
                column: "DiaItinerarioId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Atividades");

            migrationBuilder.DropTable(
                name: "PrevisoesTempo");

            migrationBuilder.DropTable(
                name: "DiasItinerario");

            migrationBuilder.DropTable(
                name: "Itinerarios");

            migrationBuilder.DropTable(
                name: "Viagens");
        }
    }
}
