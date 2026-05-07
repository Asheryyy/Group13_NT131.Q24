using BEapp.Data.DataBase;
using BEapp.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static BEapp.Interface.ISystemCreate;

namespace BEapp.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class PumpController : ControllerBase
	{
		private readonly ISystemState _state;
		private readonly AppDbContext _context;

		public PumpController(ISystemState state, AppDbContext context)
		{
			_state = state;
			_context = context;
		}

		// ESP32 gọi cái này mỗi 5 giây
		[HttpGet("Status")]
		public IActionResult GetStatus()
		{
			return Ok(new
			{
				PumpOnOrOff = _state.IsPumpOn,
				IsManualMode = _state.IsManualMode
			});
		}
	}
}