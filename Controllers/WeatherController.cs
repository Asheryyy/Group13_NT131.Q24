using BEapp.Data.DataBase;
using BEapp.Models.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // ← Thêm dòng này

// Controllers/WeatherController.cs
[Route("api/[controller]")]
[ApiController]
public class WeatherController : ControllerBase
{
	private readonly AppDbContext _context;

	public WeatherController(AppDbContext context)
	{
		_context = context;
	}

	// POST: Lưu nhiệt độ mới
	[HttpPost]
	public async Task<IActionResult> SaveWeather([FromBody] WeatherDto data)
	{
		_context.WeatherLogs.Add(new WeatherLog
		{
			Temperature = data.Temperature,
			Timestamp = DateTime.Now
		});
		await _context.SaveChangesAsync();
		return Ok();
	}

	// GET: Lấy 24 bản ghi gần nhất
	[HttpGet]
	public async Task<IActionResult> GetWeatherHistory()
	{
		var history = await _context.WeatherLogs
			.OrderByDescending(x => x.Timestamp)
			.Take(24)
			.OrderBy(x => x.Timestamp) // Sắp xếp lại theo thời gian tăng dần
			.ToListAsync();
		return Ok(history);
	}
}

