using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ONT_3rdyear_Project.Migrations
{
    /// <inheritdoc />
    public partial class lastMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Patients_AspNetUsers_ApplicationUsersId",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "DoctorID",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "PatientAllergyId",
                table: "PatientAllergies");

            migrationBuilder.RenameColumn(
                name: "ApplicationUsersId",
                table: "Patients",
                newName: "ApplicationUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Patients_ApplicationUsersId",
                table: "Patients",
                newName: "IX_Patients_ApplicationUserId");

            migrationBuilder.AddColumn<bool>(
                name: "isActive",
                table: "Instructions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDischarged",
                table: "Discharges",
                type: "bit",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "fee4927f-6e6a-4ae1-a4a3-409f71d181ff", "AQAAAAIAAYagAAAAENb9YFnROuHT5ta5nbhw6SAZEG2hbbXn76sx2iEb6dXShTDXga4Ki81sv3WD/7/DPg==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "f69664b1-0535-4e9c-855a-43436021bba5", "AQAAAAIAAYagAAAAECwnIkWtRecRSorg5JMiWBJjKg7QBU498r2ZRoKmrcJPKzpbDNYDz0rD7/F9sbsEXQ==" });

            migrationBuilder.UpdateData(
                table: "HospitalInfo",
                keyColumn: "HospitalInfoId",
                keyValue: 1,
                column: "LastUpdated",
                value: new DateTime(2025, 9, 11, 16, 22, 53, 678, DateTimeKind.Local).AddTicks(7396));

            migrationBuilder.UpdateData(
                table: "Instructions",
                keyColumn: "InstructionID",
                keyValue: 1,
                column: "isActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "Instructions",
                keyColumn: "InstructionID",
                keyValue: 2,
                column: "isActive",
                value: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Patients_AspNetUsers_ApplicationUserId",
                table: "Patients",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Patients_AspNetUsers_ApplicationUserId",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "isActive",
                table: "Instructions");

            migrationBuilder.RenameColumn(
                name: "ApplicationUserId",
                table: "Patients",
                newName: "ApplicationUsersId");

            migrationBuilder.RenameIndex(
                name: "IX_Patients_ApplicationUserId",
                table: "Patients",
                newName: "IX_Patients_ApplicationUsersId");

            migrationBuilder.AddColumn<int>(
                name: "DoctorID",
                table: "Patients",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PatientAllergyId",
                table: "PatientAllergies",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDischarged",
                table: "Discharges",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "d2435721-09e2-41cf-ac36-b98a6ad10763", "AQAAAAIAAYagAAAAEDZA9UqQWW2lt+DPL53onqRQ4JwCQUNJIPz0dTpYWGzkhsKdmQ+8RcmZFbyXJFTOKw==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "aa2807dd-0944-459c-9d0c-86c94cbefb6a", "AQAAAAIAAYagAAAAEL13O/U8Z+nUI3ZyyuP+/Nupmz+r8fX01MfqEIGVWJ4dkadC59GD1kwx/rMCztnqtg==" });

            migrationBuilder.UpdateData(
                table: "HospitalInfo",
                keyColumn: "HospitalInfoId",
                keyValue: 1,
                column: "LastUpdated",
                value: new DateTime(2025, 9, 6, 19, 2, 42, 733, DateTimeKind.Local).AddTicks(7960));

            migrationBuilder.UpdateData(
                table: "Patients",
                keyColumn: "PatientID",
                keyValue: 1,
                column: "DoctorID",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Patients",
                keyColumn: "PatientID",
                keyValue: 2,
                column: "DoctorID",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Patients",
                keyColumn: "PatientID",
                keyValue: 3,
                column: "DoctorID",
                value: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_Patients_AspNetUsers_ApplicationUsersId",
                table: "Patients",
                column: "ApplicationUsersId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
