using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ONT_3rdyear_Project.Migrations
{
    /// <inheritdoc />
    public partial class SyncAdmission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "PatientAllergies");

            migrationBuilder.RenameColumn(
                name: "AdmisionID",
                table: "Admissions",
                newName: "AdmissionID");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateOfBirth",
                table: "Patients",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "PatientAllergies",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.InsertData(
                table: "Allergies",
                columns: new[] { "AllergyId", "Description", "IsDeleted", "Name" },
                values: new object[,]
                {
                    { 1, "Allergic reaction to penicillin antibiotics", false, "Penicillin" },
                    { 2, "Allergic reaction to peanut proteins", false, "Peanuts" },
                    { 3, "Allergic reaction to shellfish", false, "Shellfish" },
                    { 4, "Allergic reaction to latex materials", false, "Latex" },
                    { 5, "Allergic reaction to dust mite proteins", false, "Dust Mites" }
                });

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

            migrationBuilder.UpdateData(
                table: "Patients",
                keyColumn: "PatientID",
                keyValue: 1,
                column: "DateOfBirth",
                value: new DateTime(2000, 1, 15, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "Patients",
                keyColumn: "PatientID",
                keyValue: 2,
                column: "DateOfBirth",
                value: new DateTime(1995, 6, 21, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "Patients",
                keyColumn: "PatientID",
                keyValue: 3,
                column: "DateOfBirth",
                value: new DateTime(2003, 2, 28, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Allergies",
                keyColumn: "AllergyId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Allergies",
                keyColumn: "AllergyId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Allergies",
                keyColumn: "AllergyId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Allergies",
                keyColumn: "AllergyId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Allergies",
                keyColumn: "AllergyId",
                keyValue: 5);

            migrationBuilder.RenameColumn(
                name: "AdmissionID",
                table: "Admissions",
                newName: "AdmisionID");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "DateOfBirth",
                table: "Patients",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "PatientAllergies",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "PatientAllergies",
                type: "nvarchar(max)",
                nullable: true);

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

            migrationBuilder.UpdateData(
                table: "Patients",
                keyColumn: "PatientID",
                keyValue: 1,
                column: "DateOfBirth",
                value: new DateOnly(2000, 1, 15));

            migrationBuilder.UpdateData(
                table: "Patients",
                keyColumn: "PatientID",
                keyValue: 2,
                column: "DateOfBirth",
                value: new DateOnly(1995, 6, 21));

            migrationBuilder.UpdateData(
                table: "Patients",
                keyColumn: "PatientID",
                keyValue: 3,
                column: "DateOfBirth",
                value: new DateOnly(2003, 2, 28));
        }
    }
}
