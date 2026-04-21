using BEapp.Data.DataBase;
using BEapp.Hubs;
using BEapp.Models.DTO;
using BEapp.Models.Entities;
using BEapp.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using static BEapp.Interface.ISystemCreate;

namespace BEapp.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class HumidityController : ControllerBase
	{
		private readonly HumidityService _service;
		private readonly IHubContext<HumidityHub> _hubContext;
		private readonly ISystemState _state;
		private readonly AppDbContext _context;

		// LẠI LÀ DI: ASP.NET sẽ tự "bơm" đồ thật vào cái phễu này
		public HumidityController(HumidityService service, IHubContext<HumidityHub> hubContext, ISystemState state, AppDbContext context)
		{
			_service = service;
			_hubContext = hubContext;
			_state = state;
			_context = context;
		}
		[HttpGet("{value}")] // Định nghĩa method GET: api/humidity/85
		public IActionResult GetHumidityStatus(float value)
		{
			// Gọi cái hàm m đã Unit Test "xanh lè" lúc nãy
			var status = _service.GetStatus(value);

			// Trả về kết quả kèm mã 200 (OK) dưới dạng JSON
			return Ok(new
			{
				humidityValue = value,
				message = status,
				serverTime = DateTime.Now
			});
		}
		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteIdRecord(float id)
		{
			var isDeleted = await _service.DeleteRecord((int)id);
			if (isDeleted)
			{
				return Ok(new
				{
					id = id,
					message = "Đã xóa thành công",
					serverTime = DateTime.Now
				});
			}
			else
			{
				return BadRequest(new
				{
					id = id,
					message = "Xóa thất bại, ID không hợp lệ",
					serverTime = DateTime.Now
				});
			}
		}
		[HttpPost]
		public async Task<IActionResult> CreateRecord([FromBody] HumidityDto data)
		{
			var record = new HumidityRecord
			{
				Value = data.Value,
				DeviceName = data.DeviceName,
				Timestamp = DateTime.Now
			};

			_context.HumidityRecords.Add(record);

			// 2. Logic Tự động & Ghi log Máy bơm
			string autoMessage = "";
			if (data.Value < 30 && !_state.IsManualMode)
			{
				autoMessage = "Hệ thống tự động: BẬT MÁY BƠM!";
				// Ghi log máy bơm vào DB
				_context.PumpLogs.Add(new PumpLog { Action = "BẬT", Source = "Tự động" });
			}
			else if (data.Value > 80 && !_state.IsManualMode)
			{
				autoMessage = "Hệ thống tự động: TẮT MÁY BƠM!";
				_context.PumpLogs.Add(new PumpLog { Action = "TẮT", Source = "Tự động" });
			}

			// 3. Lưu tất cả thay đổi vào Database (Chốt đơn!)
			await _context.SaveChangesAsync();

			// 4. Bắn SignalR cho Android như cũ
			await _hubContext.Clients.All.SendAsync("ReceiveHumidityUpdate", data);
			if (!string.IsNullOrEmpty(autoMessage))
			{
				await _hubContext.Clients.All.SendAsync("ReceiveAutoLog", autoMessage);
			}

			return Ok();
		}
		[HttpGet]
		public async Task<IActionResult> GetAllDataHistory()
		{
			var data = await _service.GetAllData();
			if (data == null || data.Count == 0)
			{
				return NotFound(new { message = "Không có dữ liệu nào trong lịch sử!" });
			}
			return Ok(data);
		}
		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateRecord(int id, [FromBody] HumidityDto data)
		{
			// 1. Thêm await vào đây để đợi kết quả trả về bool thật sự
			var result = await _service.UpdateRecordAsync(id, data);

			if (!result)
			{
				return BadRequest(new { message = "Cập nhật thất bại, ID không hợp lệ!" });
			}

			return Ok(new { message = "Cập nhật thành công!", data = result });
		}
		[HttpGet("history")]
		public async Task<IActionResult> GetHistory()
		{
			// Lấy 20 bản ghi mới nhất để hiện lên App
			var history = await _context.HumidityRecords
				.OrderByDescending(x => x.Timestamp)
				.Take(20)
				.ToListAsync();

			return Ok(history);
		}
	}
}
