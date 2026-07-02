using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backendMap.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLocationViewId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_LocationViews",
                table: "LocationViews");

            migrationBuilder.DropIndex(
                name: "IX_LocationViews_UserId_LocationCode",
                table: "LocationViews");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "LocationViews");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LocationViews",
                table: "LocationViews",
                columns: new[] { "UserId", "LocationCode", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_LocationViews",
                table: "LocationViews");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "LocationViews",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_LocationViews",
                table: "LocationViews",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_LocationViews_UserId_LocationCode",
                table: "LocationViews",
                columns: new[] { "UserId", "LocationCode" },
                unique: true);
        }
    }
}
