using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Parcial.Data.Migrations
{
    /// <inheritdoc />
    public partial class BootstrapDominio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UsuarioId = table.Column<string>(type: "TEXT", nullable: false),
                    IngresosMensuales = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.Id);
                    table.CheckConstraint("CK_Clientes_IngresosMensuales", "IngresosMensuales > 0");
                    table.ForeignKey(
                        name: "FK_Clientes_AspNetUsers_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SolicitudesCredito",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ClienteId = table.Column<int>(type: "INTEGER", nullable: false),
                    MontoSolicitado = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    FechaSolicitud = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Estado = table.Column<int>(type: "INTEGER", nullable: false),
                    MotivoRechazo = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesCredito", x => x.Id);
                    table.CheckConstraint("CK_SolicitudesCredito_MontoSolicitado", "MontoSolicitado > 0");
                    table.ForeignKey(
                        name: "FK_SolicitudesCredito_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "4f6fd1d9-b1ef-4200-9827-6ce304f2b001", "f9c55b8a-3c88-4b03-9193-cfc6beb8ebbc", "Analista", "ANALISTA" });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "c31ccfbc-df70-456b-9666-4f083ec3f08e", 0, "6D71E2FC-A9BD-4186-81D0-6AC7A9E85683", "analista@parcial.local", true, false, null, "ANALISTA@PARCIAL.LOCAL", "ANALISTA@PARCIAL.LOCAL", "AQAAAAIAAYagAAAAEIJrgXtc5St3z4Z5kh9XpgeSk6nQvrj6yqNPT0R7LIa/73TLwZNLFAbojebTtOLung==", null, false, "EAAE2E2A-A58F-4B08-B9EE-E06D9EA82185", false, "analista@parcial.local" });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { "4f6fd1d9-b1ef-4200-9827-6ce304f2b001", "c31ccfbc-df70-456b-9666-4f083ec3f08e" });

            migrationBuilder.InsertData(
                table: "Clientes",
                columns: new[] { "Id", "Activo", "IngresosMensuales", "UsuarioId" },
                values: new object[,]
                {
                    { 1, true, 1000m, "c31ccfbc-df70-456b-9666-4f083ec3f08e" },
                    { 2, true, 2500m, "c31ccfbc-df70-456b-9666-4f083ec3f08e" }
                });

            migrationBuilder.InsertData(
                table: "SolicitudesCredito",
                columns: new[] { "Id", "ClienteId", "Estado", "FechaSolicitud", "MontoSolicitado", "MotivoRechazo" },
                values: new object[,]
                {
                    { 1, 1, 0, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 800m, null },
                    { 2, 2, 1, new DateTime(2026, 4, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), 2000m, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_UsuarioId",
                table: "Clientes",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesCredito_ClienteId",
                table: "SolicitudesCredito",
                column: "ClienteId",
                unique: true,
                filter: "Estado = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SolicitudesCredito");

            migrationBuilder.DropTable(
                name: "Clientes");

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "4f6fd1d9-b1ef-4200-9827-6ce304f2b001", "c31ccfbc-df70-456b-9666-4f083ec3f08e" });

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "4f6fd1d9-b1ef-4200-9827-6ce304f2b001");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "c31ccfbc-df70-456b-9666-4f083ec3f08e");
        }
    }
}
