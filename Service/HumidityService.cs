using BEapp.Models.DTO;
using BEapp.Models.Entities;
using BEapp.Repository;

namespace BEapp.Service
{
	public class HumidityService
	{
		private readonly IRepository _repo;
		public HumidityService(IRepository repo) { _repo = repo; }
		public async Task<string> GetStatus(float humidity)
		{
			var status = humidity > 70 ? "Độ ẩm cao" : "Độ ẩm bình thường";

			// Gửi cả status VÀ humidity sang cho Repo
			_repo.saveHistory(status, humidity);

			return status;
		}
		public async Task<bool> DeleteRecord(int id)
		{
			if (id <= 0) return false; // Không có ID nào âm cả, chặn lại luôn
			return await _repo.deleteHistory(id);
		}
		public async Task<List<Humidity>> GetAllData()
		{
  			return await _repo.GetAllData();		
		}
		public async Task<bool> UpdateRecordAsync(int id, HumidityDto data)
		{
			// 1. Tính toán logic ở đây (Nhiệm vụ của Service mà!)
			var status = data.Value > 70 ? "Độ ẩm cao" : "Độ ẩm bình thường";

			// 2. Truyền ĐỦ 3 tham số sang cho Repo
			return await _repo.UpdateHistoryAsync(id, data.Value, status);
		}
		// Trong HumidityService.cs
		public async Task<PumpCommandDto> ProcessHumidityAndGetCommandAsync(HumidityDto data)
		{
			// 1. Lưu vào Database như bình thường
			var status = data.Value < 30 ? "Cần tưới nước" : "Độ ẩm ổn định";
			_repo.saveHistory(status, data.Value);

			// 2. Logic ra lệnh: Dưới 30% thì bảo thằng ESP32 bật bơm
			if (data.Value < 30)
			{
				return new PumpCommandDto
				{
					Command = "ON",
					Message = "Độ ẩm thấp quá, tưới đi!"
				};
			}

			return new PumpCommandDto
			{
				Command = "OFF",
				Message = "Đủ nước rồi, nghỉ ngơi đi."
			};
		}
	}
}
