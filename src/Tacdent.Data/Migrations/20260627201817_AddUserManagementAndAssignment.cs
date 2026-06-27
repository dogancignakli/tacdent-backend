using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tacdent.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserManagementAndAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AssignedUserId",
                table: "Appointments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_AssignedUserId",
                table: "Appointments",
                column: "AssignedUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Users_AssignedUserId",
                table: "Appointments",
                column: "AssignedUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Users_AssignedUserId",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_AssignedUserId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "AssignedUserId",
                table: "Appointments");
        }
    }
}
