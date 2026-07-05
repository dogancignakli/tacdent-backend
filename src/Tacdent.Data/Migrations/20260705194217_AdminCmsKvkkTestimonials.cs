using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Tacdent.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdminCmsKvkkTestimonials : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PriceFrom",
                table: "Services",
                newName: "PriceFromTry");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Services",
                newName: "NameTr");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Services",
                newName: "DescriptionTr");

            migrationBuilder.AddColumn<string>(
                name: "DescriptionEn",
                table: "Services",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "Services",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "NameEn",
                table: "Services",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "PriceFromEur",
                table: "Services",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "ServiceId",
                table: "Appointments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Consents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppointmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConsentType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TextVersion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AcceptedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PatientName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Consents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Consents_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Testimonials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AuthorName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    QuoteTr = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    QuoteEn = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Rating = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Testimonials", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "DescriptionEn", "DescriptionTr", "DisplayOrder", "NameEn", "NameTr", "PriceFromEur" },
                values: new object[] { "Comprehensive oral exam, cleaning, and preventive care.", "Kapsamlı ağız muayenesi, temizlik ve koruyucu bakım.", 1, "General Checkup", "Genel Muayene", 25m });

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "DescriptionEn", "DescriptionTr", "DisplayOrder", "NameEn", "NameTr", "PriceFromEur" },
                values: new object[] { "Professional whitening for a brighter, confident smile.", "Daha parlak ve özgüvenli bir gülüş için profesyonel beyazlatma.", 2, "Teeth Whitening", "Diş Beyazlatma", 65m });

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "DescriptionEn", "DescriptionTr", "DisplayOrder", "NameEn", "NameTr", "PriceFromEur" },
                values: new object[] { "Permanent tooth replacement with natural-looking results.", "Doğal görünümlü kalıcı diş değişimi.", 3, "Dental Implants", "Diş İmplantı", 350m });

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "DescriptionEn", "DescriptionTr", "DisplayOrder", "NameEn", "NameTr", "PriceFromEur" },
                values: new object[] { "Clear aligners and braces for straighter teeth.", "Şeffaf plaklar ve teller ile düzgün dişler.", 4, "Orthodontics", "Ortodonti", 800m });

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "DescriptionEn", "DescriptionTr", "DisplayOrder", "NameEn", "NameTr", "PriceFromEur" },
                values: new object[] { "Same-day relief for pain, breaks, and urgent issues.", "Ağrı, kırık ve acil durumlar için aynı gün müdahale.", 5, "Emergency Care", "Acil Bakım", 40m });

            migrationBuilder.InsertData(
                table: "Testimonials",
                columns: new[] { "Id", "AuthorName", "CreatedAt", "DisplayOrder", "IsActive", "QuoteEn", "QuoteTr", "Rating", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "Sarah M.", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, true, "The team made me feel calm from the moment I walked in. Best dental experience I've had.", "Kapıdan girdiğim andan itibaren kendimi rahat hissettim. Şimdiye kadarki en iyi diş deneyimim.", 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, "James L.", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, true, "Booking online was easy and the reminder meant I never missed my appointment.", "Çevrimiçi randevu almak kolaydı ve hatırlatma sayesinde randevumu unutmadım.", 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, "Ayşe K.", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, true, "Professional, gentle, and transparent about every step. I highly recommend them.", "Profesyonel, nazik ve her adımı şeffaf anlattılar. Kesinlikle tavsiye ederim.", 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_ServiceId",
                table: "Appointments",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Consents_AppointmentId",
                table: "Consents",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Consents_ConsentType_TextVersion",
                table: "Consents",
                columns: new[] { "ConsentType", "TextVersion" });

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Services_ServiceId",
                table: "Appointments",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Services_ServiceId",
                table: "Appointments");

            migrationBuilder.DropTable(
                name: "Consents");

            migrationBuilder.DropTable(
                name: "Testimonials");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_ServiceId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "DescriptionEn",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "NameEn",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "PriceFromEur",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "ServiceId",
                table: "Appointments");

            migrationBuilder.RenameColumn(
                name: "PriceFromTry",
                table: "Services",
                newName: "PriceFrom");

            migrationBuilder.RenameColumn(
                name: "NameTr",
                table: "Services",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "DescriptionTr",
                table: "Services",
                newName: "Description");

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Comprehensive oral exam, cleaning, and preventive care.", "General Checkup" });

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Professional whitening for a brighter, confident smile.", "Teeth Whitening" });

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Permanent tooth replacement with natural-looking results.", "Dental Implants" });

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Clear aligners and braces for straighter teeth.", "Orthodontics" });

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Same-day relief for pain, breaks, and urgent issues.", "Emergency Care" });
        }
    }
}
