using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ONT_3rdyear_Project.Migrations
{
    /// <inheritdoc />
    public partial class V1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "e68271d6-ae01-4407-b5f1-8a3af2c227cc", "AQAAAAIAAYagAAAAEP46ee7XlOt5UGcca0C7wvncTAwMzD1BgMg9vMF6X0j/jnlm62Yf+RU7P6azr+h+oA==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "7f2c255c-6991-4cd5-8ad1-a5482fca5f68", "AQAAAAIAAYagAAAAEJEQrztTaTgVFGnvkSjfRqZm6Ugzm+Pug6cl9BzsEgRgxkzjvYBrUsatR1AUTL0AHQ==" });

            migrationBuilder.UpdateData(
                table: "HospitalInfo",
                keyColumn: "HospitalInfoId",
                keyValue: 1,
                column: "LastUpdated",
                value: new DateTime(2025, 9, 14, 5, 44, 40, 533, DateTimeKind.Local).AddTicks(9825));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "eb6248f3-3f65-48d5-91ea-acb307e1f8b8", "AQAAAAIAAYagAAAAEMIqdreDhG8IhCNMZhBKP8dM+gyy4R+xPh+qXsymty/sH0kDH0numj1G5z2/byIgVA==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "ba5f99bc-8090-4c4b-a271-1adb24eb0dcd", "AQAAAAIAAYagAAAAEMfz+YS/dGJ6YeHpJXp1ZlZHMpTluFGi5X2l6PQqttI8BQ4IzpVjqMvUfsELWBXB8g==" });

            migrationBuilder.UpdateData(
                table: "HospitalInfo",
                keyColumn: "HospitalInfoId",
                keyValue: 1,
                column: "LastUpdated",
                value: new DateTime(2025, 9, 14, 5, 42, 4, 476, DateTimeKind.Local).AddTicks(5833));
        }
    }
}
