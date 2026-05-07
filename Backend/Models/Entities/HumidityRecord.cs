namespace BEapp.Models.Entities
{
	public class HumidityRecord
	{
		public int Id { get; set; }
		public double Value { get; set; }
		public string DeviceName { get; set; }
		public DateTime Timestamp { get; set; } = DateTime.Now;
	}
}
