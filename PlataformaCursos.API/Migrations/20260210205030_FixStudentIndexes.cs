using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlataformaCursos.API.Migrations
{
    /// <inheritdoc />
    public partial class FixStudentIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "EmailIndex",
                table: "Students");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "Students",
                column: "NormalizedEmail",
                unique: true,
                filter: "[NormalizedEmail] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "EmailIndex",
                table: "Students");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "Students",
                column: "NormalizedEmail");
        }
    }
}
