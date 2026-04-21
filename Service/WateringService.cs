using BEapp.Repository;
using Moq;
using System.Diagnostics.Eventing.Reader;

namespace BEapp.Service
{
	public class WateringService
	{
		private readonly IRepository _repo;
		public WateringService(IRepository repo) { _repo = repo; } // "Bơm" đồ giả vào đây
		public string GetStatus(float temp)
		{
			var status = temp > 34 ? "Nóng quá" : "Mát";
			_repo.saveHistory(status, temp); // Gọi thằng phụ thuộc
			return status;
		}
	}
}
