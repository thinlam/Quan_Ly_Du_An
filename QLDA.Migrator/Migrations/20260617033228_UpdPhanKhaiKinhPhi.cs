using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class UpdPhanKhaiKinhPhi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
          

            migrationBuilder.AddColumn<string>(
                name: "TrichYeu",
                table: "PhanKhaiKinhPhi",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
          
            migrationBuilder.DropColumn(
                name: "TrichYeu",
                table: "PhanKhaiKinhPhi");

          
        }
    }
}
