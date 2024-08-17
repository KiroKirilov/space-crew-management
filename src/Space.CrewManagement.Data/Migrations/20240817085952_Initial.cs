using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Space.CrewManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Licenses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Licenses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MemberTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CrewMembers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Birthday = table.Column<DateOnly>(type: "date", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CountryCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    ProfileImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastCertificationDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    StatusDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LicenseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MemberTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrewMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CrewMembers_Licenses_LicenseId",
                        column: x => x.LicenseId,
                        principalTable: "Licenses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CrewMembers_MemberTypes_MemberTypeId",
                        column: x => x.MemberTypeId,
                        principalTable: "MemberTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Licenses",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { new Guid("09917044-4413-43a4-82bf-4689ba49d2a2"), " An individual wishing to work as a cabin crewmember in commercial air transport within an EC member State must hold a valid cabin crew attestation(CCA).", "Cabin crew license" },
                    { new Guid("ead47c5e-c268-44ea-8837-30ff16e0ee10"), " Pilots with an ATP certificate are eligible to fly for an airline and will meet the hiring minimums of most regional airline pilot jobs.", "Airline transport pilot (ATP) license" }
                });

            migrationBuilder.InsertData(
                table: "MemberTypes",
                columns: new[] { "Id", "Name", "Type" },
                values: new object[,]
                {
                    { new Guid("f3b3b3b3-3b3b-3b3b-3b3b-3b3b3b3b3b3b"), "Pilot", 0 },
                    { new Guid("f3b3b3b3-3b3b-3b3b-3b3b-3b3b3b3b3b3c"), "Regular", 1 },
                    { new Guid("f3b3b3b3-3b3b-3b3b-3b3b-3b3b3b3b3b3d"), "Steward", 2 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CrewMembers_Email",
                table: "CrewMembers",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CrewMembers_LicenseId",
                table: "CrewMembers",
                column: "LicenseId");

            migrationBuilder.CreateIndex(
                name: "IX_CrewMembers_MemberTypeId",
                table: "CrewMembers",
                column: "MemberTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CrewMembers");

            migrationBuilder.DropTable(
                name: "Licenses");

            migrationBuilder.DropTable(
                name: "MemberTypes");
        }
    }
}
