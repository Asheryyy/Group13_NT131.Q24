using BEapp.Data.DataBase;
using BEapp.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BEapp.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class DeviceController : ControllerBase
	{
		private readonly AppDbContext _context;

		public DeviceController(AppDbContext context)
		{
			_context = context;
		}

		// GET: api/device/config?deviceName=ESP32_Wokwi_Tai
		[HttpGet("config")]
		public async Task<IActionResult> GetConfig([FromQuery] string deviceName)
		{
			var config = await _context.DeviceConfigs
				.FirstOrDefaultAsync(x => x.DeviceName == deviceName);

			if (config == null)
			{
				// Trả về config mặc định nếu chưa có
				return Ok(new
				{
					deviceName = deviceName,
					lowerThreshold = 30f,
					upperThreshold = 80f
				});
			}

			return Ok(new
			{
				deviceName = config.DeviceName,
				lowerThreshold = config.LowerThreshold,
				upperThreshold = config.UpperThreshold
			});
		}

		// PUT: api/device/config
		[HttpPut("config")]
		[Authorize]
		public async Task<IActionResult> UpdateConfig([FromBody] DeviceConfigRequest request)
		{
			// Validate ngưỡng
			if (request.LowerThreshold >= request.UpperThreshold)
			{
				return BadRequest(new { message = "Ngưỡng thấp phải nhỏ hơn ngưỡng cao!" });
			}

			var config = await _context.DeviceConfigs
				.FirstOrDefaultAsync(x => x.DeviceName == request.DeviceName);

			if (config == null)
			{
				// Tạo mới nếu chưa có
				_context.DeviceConfigs.Add(new DeviceConfig
				{
					DeviceName = request.DeviceName,
					LowerThreshold = request.LowerThreshold,
					UpperThreshold = request.UpperThreshold,
					UpdatedAt = DateTime.Now
				});
			}
			else
			{
				// Cập nhật nếu đã có
				config.LowerThreshold = request.LowerThreshold;
				config.UpperThreshold = request.UpperThreshold;
				config.UpdatedAt = DateTime.Now;
			}

			await _context.SaveChangesAsync();

			return Ok(new
			{
				message = "Cập nhật ngưỡng thành công!",
				deviceName = request.DeviceName,
				lowerThreshold = request.LowerThreshold,
				upperThreshold = request.UpperThreshold
			});
		}
	}

	public class DeviceConfigRequest
	{
		public string DeviceName { get; set; } = "";
		public float LowerThreshold { get; set; }
		public float UpperThreshold { get; set; }
	}
}