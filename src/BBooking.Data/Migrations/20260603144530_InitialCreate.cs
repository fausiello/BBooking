using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BBooking.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Localita",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Regione = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Localita", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Servizi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Servizi", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Utenti",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Ruolo = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Utenti", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CaseVacanze",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Descrizione = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PrezzoPerNotte = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    HostId = table.Column<int>(type: "int", nullable: false),
                    LocalitaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseVacanze", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CaseVacanze_Localita_LocalitaId",
                        column: x => x.LocalitaId,
                        principalTable: "Localita",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CaseVacanze_Utenti_HostId",
                        column: x => x.HostId,
                        principalTable: "Utenti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CasaVacanzeServizio",
                columns: table => new
                {
                    CaseVacanzeId = table.Column<int>(type: "int", nullable: false),
                    ServiziId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CasaVacanzeServizio", x => new { x.CaseVacanzeId, x.ServiziId });
                    table.ForeignKey(
                        name: "FK_CasaVacanzeServizio_CaseVacanze_CaseVacanzeId",
                        column: x => x.CaseVacanzeId,
                        principalTable: "CaseVacanze",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CasaVacanzeServizio_Servizi_ServiziId",
                        column: x => x.ServiziId,
                        principalTable: "Servizi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Prenotazioni",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DataInizio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataFine = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Stato = table.Column<int>(type: "int", nullable: false),
                    Totale = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    GuestId = table.Column<int>(type: "int", nullable: false),
                    CasaVacanzeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prenotazioni", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Prenotazioni_CaseVacanze_CasaVacanzeId",
                        column: x => x.CasaVacanzeId,
                        principalTable: "CaseVacanze",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Prenotazioni_Utenti_GuestId",
                        column: x => x.GuestId,
                        principalTable: "Utenti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Localita",
                columns: new[] { "Id", "Nome", "Regione" },
                values: new object[,]
                {
                    { 1, "Roma", "Lazio" },
                    { 2, "Napoli", "Campania" }
                });

            migrationBuilder.InsertData(
                table: "Servizi",
                columns: new[] { "Id", "Nome" },
                values: new object[,]
                {
                    { 1, "Wi-Fi" },
                    { 2, "Piscina" },
                    { 3, "Aria Condizionata" }
                });

            migrationBuilder.InsertData(
                table: "Utenti",
                columns: new[] { "Id", "Email", "Nome", "Password", "Ruolo" },
                values: new object[,]
                {
                    { 1, "admin@bbooking.com", "Admin", "Admin123", 2 },
                    { 2, "host@bbooking.com", "Host", "Host123", 1 },
                    { 3, "guest@bbooking.com", "Guest", "Guest123", 0 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CasaVacanzeServizio_ServiziId",
                table: "CasaVacanzeServizio",
                column: "ServiziId");

            migrationBuilder.CreateIndex(
                name: "IX_CaseVacanze_HostId",
                table: "CaseVacanze",
                column: "HostId");

            migrationBuilder.CreateIndex(
                name: "IX_CaseVacanze_LocalitaId",
                table: "CaseVacanze",
                column: "LocalitaId");

            migrationBuilder.CreateIndex(
                name: "IX_Prenotazioni_CasaVacanzeId",
                table: "Prenotazioni",
                column: "CasaVacanzeId");

            migrationBuilder.CreateIndex(
                name: "IX_Prenotazioni_GuestId",
                table: "Prenotazioni",
                column: "GuestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CasaVacanzeServizio");

            migrationBuilder.DropTable(
                name: "Prenotazioni");

            migrationBuilder.DropTable(
                name: "Servizi");

            migrationBuilder.DropTable(
                name: "CaseVacanze");

            migrationBuilder.DropTable(
                name: "Localita");

            migrationBuilder.DropTable(
                name: "Utenti");
        }
    }
}
