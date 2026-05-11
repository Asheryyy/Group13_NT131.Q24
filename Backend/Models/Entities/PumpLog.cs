namespace BEapp.Models.Entities
{
	public class PumpLog
	{
		public int Id { get; set; }
		public string Action { get; set; } // "BẬT" hoặc "TẮT"
		public string Source { get; set; } // "Tự động" hoặc "Tài bấm"
		public DateTime Timestamp { get; set; } = DateTime.UtcNow.AddHours(7);
	}
}
