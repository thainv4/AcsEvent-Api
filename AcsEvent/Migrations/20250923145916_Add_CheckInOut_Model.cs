using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AcsEvent.Migrations
{
    /// <inheritdoc />
    public partial class Add_CheckInOut_Model : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CheckInOuts",
                columns: table => new
                {
                    MaNV = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TimeIn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    TimeOut = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DiMuon = table.Column<bool>(type: "bit", nullable: false),
                    VeSom = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CheckInOuts", x => x.MaNV);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CheckInOuts");
        }
    }
}
