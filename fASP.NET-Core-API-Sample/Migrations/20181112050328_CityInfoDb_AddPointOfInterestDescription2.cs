using Microsoft.EntityFrameworkCore.Migrations;

namespace fASP.NETCoreAPISample.Migrations
{
    public partial class CityInfoDb_AddPointOfInterestDescription2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description2",
                table: "PointsOfInterest",
                maxLength: 200,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description2",
                table: "PointsOfInterest");
        }
    }
}
