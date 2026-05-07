using BEapp.Data.DataBase;
using BEapp.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BEapp.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class CheckDataController : ControllerBase
	{
		private readonly AppDbContext _context;

		// "Xin" hệ thống cấp cho cái điều khiển _context đã học ở bài trước
		public CheckDataController(AppDbContext context)
		{
			_context = context;
		}

		// 1. Cổng LẤY dữ liệu (Để app Android vẽ biểu đồ)
		[HttpGet]
		public async Task<ActionResult<IEnumerable<CheckData>>> GetHistory()
		{
			return await _context.CheckDatas.OrderByDescending(x => x.Timestamp).ToListAsync();
		}

		// 2. Cổng LƯU dữ liệu (Để nhận data từ cảm biến/App)
		[HttpPost]
		public async Task<ActionResult<CheckData>> PostData(CheckData data)
		{
			_context.CheckDatas.Add(data);
			await _context.SaveChangesAsync();

			return Ok(new { message = "Lưu dữ liệu tưới cây thành công!", id = data.Id });
		}
		[HttpPut("{id}")]
		public async Task<IActionResult> PutData(int id, CheckData data)
		{
			var existingData = await _context.CheckDatas.FindAsync(id);
			if (existingData ==null)
			{
				return NotFound(new { message = "Không tìm thấy dữ liệu với ID đã cho!" });
			}
			existingData.Temperature = data.Temperature;
			existingData.Humidity = data.Humidity;
			existingData.Timestamp = data.Timestamp;
			await _context.SaveChangesAsync();
			return Ok(new { message = "Cập nhật dữ liệu thành công!" });
		}
		[HttpDelete("{id}")]
		public async Task<IActionResult>DeleteAction(int id)
		{
			var data = await _context.CheckDatas.FindAsync(id);
			if (data == null)
			{
				return NotFound(new { message = "Không tìm thấy dữ liệu với ID đã cho!" });
			}
			_context.CheckDatas.Remove(data);
			await _context.SaveChangesAsync();
			return Ok(new { message = "Xóa dữ liệu thành công!"});
		}
	}
}
