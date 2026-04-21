using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore;

namespace BEapp.Models.Entities
{
	public class WeatherLog
	{
		public int Id { get; set; }
		public float Temperature { get; set; }
		public DateTime Timestamp { get; set; } = DateTime.Now;
	}
}
