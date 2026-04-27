using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Parcial.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedUsuariosSeparados : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "c31ccfbc-df70-456b-9666-4f083ec3f08e",
                columns: new[] { "Email", "NormalizedEmail", "NormalizedUserName", "UserName" },
                values: new object[] { "analista@parcial.com", "ANALISTA@PARCIAL.COM", "ANALISTA@PARCIAL.COM", "analista@parcial.com" });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[,]
                {
                    { "1a311f98-fd47-47d4-9a11-4fbd56f8de03", 0, "75925CD1-08AF-49A1-98C1-4E56E8A6AE04", "cliente2@parcial.com", true, false, null, "CLIENTE2@PARCIAL.COM", "CLIENTE2@PARCIAL.COM", "AQAAAAIAAYagAAAAEIJrgXtc5St3z4Z5kh9XpgeSk6nQvrj6yqNPT0R7LIa/73TLwZNLFAbojebTtOLung==", null, false, "B6B4B3CA-F8A9-430E-9C9D-B61CC688D272", false, "cliente2@parcial.com" },
                    { "f489a6ff-7eb1-4d5a-86d8-5bf910ca0701", 0, "D8689711-5F43-40B6-8F06-71DBDECC80B7", "cliente1@parcial.com", true, false, null, "CLIENTE1@PARCIAL.COM", "CLIENTE1@PARCIAL.COM", "AQAAAAIAAYagAAAAEIJrgXtc5St3z4Z5kh9XpgeSk6nQvrj6yqNPT0R7LIa/73TLwZNLFAbojebTtOLung==", null, false, "7A31D7D1-5B16-4A78-80FA-5AF8E3310C5D", false, "cliente1@parcial.com" }
                });

            migrationBuilder.UpdateData(
                table: "Clientes",
                keyColumn: "Id",
                keyValue: 1,
                column: "UsuarioId",
                value: "f489a6ff-7eb1-4d5a-86d8-5bf910ca0701");

            migrationBuilder.UpdateData(
                table: "Clientes",
                keyColumn: "Id",
                keyValue: 2,
                column: "UsuarioId",
                value: "1a311f98-fd47-47d4-9a11-4fbd56f8de03");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1a311f98-fd47-47d4-9a11-4fbd56f8de03");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "f489a6ff-7eb1-4d5a-86d8-5bf910ca0701");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "c31ccfbc-df70-456b-9666-4f083ec3f08e",
                columns: new[] { "Email", "NormalizedEmail", "NormalizedUserName", "UserName" },
                values: new object[] { "analista@parcial.local", "ANALISTA@PARCIAL.LOCAL", "ANALISTA@PARCIAL.LOCAL", "analista@parcial.local" });

            migrationBuilder.UpdateData(
                table: "Clientes",
                keyColumn: "Id",
                keyValue: 1,
                column: "UsuarioId",
                value: "c31ccfbc-df70-456b-9666-4f083ec3f08e");

            migrationBuilder.UpdateData(
                table: "Clientes",
                keyColumn: "Id",
                keyValue: 2,
                column: "UsuarioId",
                value: "c31ccfbc-df70-456b-9666-4f083ec3f08e");
        }
    }
}
