using System;
using System.Net.NetworkInformation;
using static BEapp.Service.HumidityService;
using Microsoft.EntityFrameworkCore;
using BEapp.Data.DataBase;
using BEapp.Models.Entities;

namespace BEapp.Repository
{
	// Mày phải ghi " : IRepository" để máy biết thằng này đúng là "Bảo vệ"
	public class SqlRepository : IRepository
	{
		private readonly AppDbContext _context;

		// Constructor nhận DbContext từ hệ thống
		public SqlRepository(AppDbContext context)
		{
			_context = context;
		}
		public void saveHistory(string status, float value)
		{
			// Tạm thời mình chỉ lưu status, lát t chỉ m cách lưu cả Value sau
			var record = new Humidity
			{
				Status = status,
				Value = value, // Sử dụng giá trị value được truyền vào
				CreatedAt = DateTime.Now
			};

			_context.Humidities.Add(record); // Thêm vào hàng đợi
			_context.SaveChanges();         // Nhấn nút "Lưu" chính thức xuống SQL

			Console.WriteLine("[DB] Đã lưu vào SQL Server thành công!");
		}
		public async Task<bool> deleteHistory(int id)
		{
			var item = _context.Humidities.Find(id); // Tìm thằng có ID này
			if (item == null) return false;

			_context.Humidities.Remove(item); // Xóa nó đi
			await _context.SaveChangesAsync();
			return true;
		}
		public async Task<List<Humidity>> GetAllData()
		{
  			return await _context.Humidities.ToListAsync(); // Lấy tất cả dữ liệu từ bảng Humidities	
		}
		// Trong SqlRepository.cs
		public async Task<bool> UpdateHistoryAsync(int id, float newValue, string status)
		{
			var item = await _context.Humidities.FindAsync(id);
			if (item == null) return false;

			item.Value = newValue;
			item.Status = status; // Nhận status đã tính sẵn từ Service
			item.CreatedAt = DateTime.Now;

			await _context.SaveChangesAsync();
			return true;
		}
	}
}
