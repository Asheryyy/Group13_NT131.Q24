namespace BEapp.Models.DTO
{
	public class HumidityDto
	{
		public float Value { get; set; }
		public string DeviceName { get; set; } // Để biết con ESP32 nào gửi lên
	}
}
