using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace innova_project.Migrations
{
    /// <inheritdoc />
    public partial class Adddd3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "MonthlyExpenseLimit",
                table: "Users",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MonthlyExpenseLimit",
                table: "Users");
        }
    }
}
