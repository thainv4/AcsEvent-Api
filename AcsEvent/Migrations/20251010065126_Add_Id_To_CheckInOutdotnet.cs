using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AcsEvent.Migrations
{
    /// <inheritdoc />
    public partial class Add_Id_To_CheckInOutdotnet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_CheckInOuts",
                table: "CheckInOuts");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "CheckInOuts",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CheckInOuts",
                table: "CheckInOuts",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_CheckInOuts",
                table: "CheckInOuts");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "CheckInOuts");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CheckInOuts",
                table: "CheckInOuts",
                column: "MaNV");
        }
    }
}
