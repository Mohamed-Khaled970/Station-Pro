using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StationPro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Admins",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admins", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Admins_Email",
                table: "Admins",
                column: "Email",
                unique: true);

            migrationBuilder.InsertData(
                      table: "Admins",
                      columns: new[] { "Email", "PasswordHash", "Name", "CreatedAt" },
                      values: new object[]
                      {
                              "admin@stationpro.com",
                              "$2a$11$veU27awjAT1D0DQqqnnkR.Cu/nynKY34K9kqEIuwbBYBlRfryGU4q",  // <-- paste the hash from step 1
                              "Super Admin",
                              new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                      });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Admins");
        }
    }
}
