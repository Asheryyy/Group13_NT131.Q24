using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BEapp.Migrations
{
    /// <inheritdoc />
    public partial class WeatherLogs : Migration
    {
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "WeatherLogs",
				columns: table => new
				{
					Id = table.Column<int>(nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					Temperature = table.Column<float>(nullable: false),
					Timestamp = table.Column<DateTime>(nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_WeatherLogs", x => x.Id);
				});
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(name: "WeatherLogs");
		}
	}
}
