using System.ComponentModel.DataAnnotations;

namespace BEapp.Models.Entities
{
	public class Humidity
	{
		[Key] // Khóa chính (Primary Key)
		public int Id { get; set; }
		public float Value { get; set; }
		public string Status { get; set; }
		public DateTime CreatedAt { get; set; } = DateTime.Now;
	}
}