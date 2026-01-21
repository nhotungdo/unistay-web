using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Unistay_Web.Migrations
{
    /// <inheritdoc />
    public partial class AddOnboardingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CompatibilityAnalysis",
                table: "UserProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "UserProfiles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HouseNumber",
                table: "UserProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAddressVerified",
                table: "UserProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsOnboardingComplete",
                table: "UserProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LandlordVerificationStatus",
                table: "UserProfiles",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OnboardingStep",
                table: "UserProfiles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "StreetName",
                table: "UserProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ZodiacSign",
                table: "UserProfiles",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompatibilityAnalysis",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "HouseNumber",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "IsAddressVerified",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "IsOnboardingComplete",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "LandlordVerificationStatus",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "OnboardingStep",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "StreetName",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "ZodiacSign",
                table: "UserProfiles");
        }
    }
}
