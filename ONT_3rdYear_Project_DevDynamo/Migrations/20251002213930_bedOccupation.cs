using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ONT_3rdyear_Project.Migrations
{
    /// <inheritdoc />
    public partial class bedOccupation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "06595bf1-b7ae-4b55-868d-9c80f10ce2ea", "AQAAAAIAAYagAAAAELDAJdxHHkdKk6u7TPQ/mDQKnlz5CWEe0nNR1bKnKUIn23uIf7xlW8PPjRZ2UcKl/Q==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "defda11f-43cd-4a5b-aac9-27b4f425a747", "AQAAAAIAAYagAAAAEHcSWl0D/PsBf1hKzzbmRq8SneluoXy5VviYD4El5Yvg6miqGqaUSI3wzl6WZMh+Mw==" });

            migrationBuilder.UpdateData(
                table: "Beds",
                keyColumn: "BedId",
                keyValue: 1,
                column: "IsOccupied",
                value: true);

            migrationBuilder.UpdateData(
                table: "Beds",
                keyColumn: "BedId",
                keyValue: 3,
                column: "IsOccupied",
                value: true);

            migrationBuilder.UpdateData(
                table: "HospitalInfo",
                keyColumn: "HospitalInfoId",
                keyValue: 1,
                column: "LastUpdated",
                value: new DateTime(2025, 10, 2, 23, 39, 25, 762, DateTimeKind.Local).AddTicks(1638));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
                table: "Beds",
                keyColumn: "BedId",
                keyValue: 1,
                column: "IsOccupied",
                value: false);

            migrationBuilder.UpdateData(
                table: "Beds",
                keyColumn: "BedId",
                keyValue: 3,
                column: "IsOccupied",
                value: false);

            migrationBuilder.UpdateData(
                table: "HospitalInfo",
                keyColumn: "HospitalInfoId",
                keyValue: 1,
                column: "LastUpdated",
                value: new DateTime(2025, 9, 20, 15, 47, 48, 786, DateTimeKind.Local).AddTicks(1786));
        }
    }
}
