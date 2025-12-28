using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ONT_3rdyear_Project.Migrations
{
    /// <inheritdoc />
    public partial class updated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Discharges_PatientID",
                table: "Discharges");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "Patients",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<bool>(
                name: "IsDischarged",
                table: "Discharges",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DischargeDate",
                table: "Discharges",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<int>(
                name: "AdmissionID",
                table: "Discharges",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Discharges",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "55730e64-9632-405e-8128-017caf27a7f0", "AQAAAAIAAYagAAAAEGxdojPY1YJpTnHh6XfrD3rCGgij3QqsHV8u+05zL/CokSz6hxLJOZZhtIh5EvfCJQ==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "e7629b27-68a9-41b1-8cfb-a80495b74207", "AQAAAAIAAYagAAAAEPOJdzIIJDVlzRz5zXniJxjGaFdIPN2Q5iIRlnzCfsKShg/zAH3gK7APZHA9OdqGFQ==" });

            migrationBuilder.UpdateData(
                table: "HospitalInfo",
                keyColumn: "HospitalInfoId",
                keyValue: 1,
                column: "LastUpdated",
                value: new DateTime(2025, 10, 17, 16, 1, 16, 967, DateTimeKind.Local).AddTicks(5576));

            migrationBuilder.UpdateData(
                table: "Patients",
                keyColumn: "PatientID",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2025, 10, 17, 16, 1, 16, 963, DateTimeKind.Local).AddTicks(5994));

            migrationBuilder.UpdateData(
                table: "Patients",
                keyColumn: "PatientID",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2025, 10, 17, 16, 1, 16, 964, DateTimeKind.Local).AddTicks(9292));

            migrationBuilder.UpdateData(
                table: "Patients",
                keyColumn: "PatientID",
                keyValue: 3,
                column: "CreatedDate",
                value: new DateTime(2025, 10, 17, 16, 1, 16, 964, DateTimeKind.Local).AddTicks(9308));

            migrationBuilder.CreateIndex(
                name: "IX_Discharges_AdmissionID",
                table: "Discharges",
                column: "AdmissionID");

            migrationBuilder.CreateIndex(
                name: "IX_Discharges_PatientID",
                table: "Discharges",
                column: "PatientID",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Discharges_Admissions_AdmissionID",
                table: "Discharges",
                column: "AdmissionID",
                principalTable: "Admissions",
                principalColumn: "AdmissionID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Discharges_Admissions_AdmissionID",
                table: "Discharges");

            migrationBuilder.DropIndex(
                name: "IX_Discharges_AdmissionID",
                table: "Discharges");

            migrationBuilder.DropIndex(
                name: "IX_Discharges_PatientID",
                table: "Discharges");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "AdmissionID",
                table: "Discharges");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Discharges");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDischarged",
                table: "Discharges",
                type: "bit",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DischargeDate",
                table: "Discharges",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_Discharges_PatientID",
                table: "Discharges",
                column: "PatientID");
        }
    }
}
