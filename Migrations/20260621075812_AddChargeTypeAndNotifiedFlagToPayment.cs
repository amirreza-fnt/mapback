using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backendMap.Migrations
{
    /// <inheritdoc />
    public partial class AddChargeTypeAndNotifiedFlagToPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ChargeType",
                table: "Payments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "NotifiedToMunicipality",
                table: "Payments",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChargeType",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "NotifiedToMunicipality",
                table: "Payments");
        }
    }
}
