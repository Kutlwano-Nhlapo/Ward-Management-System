using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ONT_3rdyear_Project.Migrations
{
    /// <inheritdoc />
    public partial class PrescriptionDateToDateTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCompleted",
                table: "TreatVisits",
                type: "bit",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateIssued",
                table: "Prescriptions",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AlterColumn<string>(
                name: "NurseRequest",
                table: "Instructions",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

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

            migrationBuilder.UpdateData(
                table: "TreatVisits",
                keyColumn: "TreatVisitID",
                keyValue: 1,
                column: "IsCompleted",
                value: null);

            migrationBuilder.UpdateData(
                table: "TreatVisits",
                keyColumn: "TreatVisitID",
                keyValue: 2,
                column: "IsCompleted",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCompleted",
                table: "TreatVisits");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "DateIssued",
                table: "Prescriptions",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "NurseRequest",
                table: "Instructions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "259e15d8-3736-4b81-9127-e3bae4b46739", "AQAAAAIAAYagAAAAEDF3M4viaTei7O2Br0ghroy6DpVUWKe26E1x8Tx0WSHbZQvAkZRXLhH9M+ZWexwRfg==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "806c3b64-c1e8-4bdb-9068-a4b35bb4b43c", "AQAAAAIAAYagAAAAENPnYudOE3+4XqBLgL47BEngyk87in3+VjkxP7b0+xUXqntxqkb7H0tlB5Q17o3wnQ==" });

            migrationBuilder.UpdateData(
                table: "HospitalInfo",
                keyColumn: "HospitalInfoId",
                keyValue: 1,
                column: "LastUpdated",
                value: new DateTime(2025, 9, 20, 15, 47, 48, 786, DateTimeKind.Local).AddTicks(1786));
        }
    }
}
