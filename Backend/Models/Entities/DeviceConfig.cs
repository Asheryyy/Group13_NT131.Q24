// Models/Entities/DeviceConfig.cs
namespace BEapp.Models.Entities
{
	public class DeviceConfig
	{
		public int Id { get; set; }
		public string DeviceName { get; set; } = "";
		public float LowerThreshold { get; set; } = 30; // Ngưỡng thấp mặc định
		public float UpperThreshold { get; set; } = 80; // Ngưỡng cao mặc định
		public DateTime UpdatedAt { get; set; } = DateTime.Now;
	}
}