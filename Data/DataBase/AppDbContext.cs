using BEapp.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BEapp.Data.DataBase
{
	public class AppDbContext : DbContext
	{
		public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
		{
		}

		// Khai báo DbSet: EF Core sẽ nhìn vào đây để tạo bảng "CheckDatas" trong DB
		public DbSet<CheckData> CheckDatas { get; set; }
		public DbSet<User> Users { get; set; }
		public DbSet<Humidity> Humidities { get; set; }
		public DbSet<HumidityRecord> HumidityRecords { get; set; }
		public DbSet<PumpLog> PumpLogs { get; set; }
		public DbSet<WeatherLog> WeatherLogs { get; set; }
	}
}
