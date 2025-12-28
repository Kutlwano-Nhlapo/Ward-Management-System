using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ONT_3rdyear_Project.Migrations
{
    /// <inheritdoc />
    public partial class AddIsReadToInstruction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRead",
                table: "Instructions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReadAt",
                table: "Instructions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "8de2898c-7f95-4f33-8422-041ac4837af5", "AQAAAAIAAYagAAAAEAuQ9VRHwYU8LC6Mi5WzcjbbBLMRYNFusKoUd9MrrbupsQNcF3yQZ+Mm63galmiAbg==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "830a97e3-764d-4d8c-8dc5-e08c9977047d", "AQAAAAIAAYagAAAAENz7c34gXQUYGHEJ6+vKOOfrTN1zknnnGPLEPQ6f6JQvuh/tuaC4PwGW/m+TVzpTmw==" });

            migrationBuilder.UpdateData(
                table: "HospitalInfo",
                keyColumn: "HospitalInfoId",
                keyValue: 1,
                column: "LastUpdated",
                value: new DateTime(2025, 10, 6, 13, 35, 33, 127, DateTimeKind.Local).AddTicks(3829));

            migrationBuilder.UpdateData(
                table: "Instructions",
                keyColumn: "InstructionID",
                keyValue: 1,
                columns: new[] { "IsRead", "ReadAt" },
                values: new object[] { false, null });

            migrationBuilder.UpdateData(
                table: "Instructions",
                keyColumn: "InstructionID",
                keyValue: 2,
                columns: new[] { "IsRead", "ReadAt" },
                values: new object[] { false, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRead",
                table: "Instructions");

            migrationBuilder.DropColumn(
                name: "ReadAt",
                table: "Instructions");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "dd064fd1-a8d2-4454-bff7-88774b734161", "AQAAAAIAAYagAAAAEDxDDvwERmCcRgwJiS7NXgh35wlCQqoVfHZ0jAXimT1qq4DJoBD1ewjUtgF3xXEuvQ==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "2d0d8f6f-7322-4550-b10b-e5305c531644", "AQAAAAIAAYagAAAAEIxK3p2kox6TAZwcK45CQ6YjssqJ+c+GCvorn8imYJoOJPvlarKHwHCBT8+INnP+AA==" });

            migrationBuilder.UpdateData(
                table: "HospitalInfo",
                keyColumn: "HospitalInfoId",
                keyValue: 1,
                column: "LastUpdated",
                value: new DateTime(2025, 10, 2, 6, 37, 49, 87, DateTimeKind.Local).AddTicks(7469));
        }
    }
}
