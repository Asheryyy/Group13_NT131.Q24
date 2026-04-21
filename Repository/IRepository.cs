using BEapp.Models.Entities;

namespace BEapp.Repository
{
	public interface IRepository
	{
		void saveHistory(string status, float value);
		Task<bool> deleteHistory(int id);
		Task<List<Humidity>> GetAllData();
		Task<bool> UpdateHistoryAsync(int id, float newValue, string status);
	}
}