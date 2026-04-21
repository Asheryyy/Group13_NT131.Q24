using System.ComponentModel.DataAnnotations;

namespace BEapp.Models.Entities
{
	public class CheckData
	{
		[Key]
		public int Id { get; set; }
		public double Humidity { get; set; }
		public double Temperature { get; set; }
		public DateTime Timestamp { get; set; } = DateTime.Now;
	}
}
