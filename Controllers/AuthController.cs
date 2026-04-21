using BEapp.Data.DataBase;
using BEapp.Interface;
using BEapp.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static BEapp.Interface.ISystemCreate;

namespace BEapp.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
	public class AuthController : ControllerBase
	{
		public readonly AppDbContext _context;
		private readonly IConfiguration _config;
		private readonly ISystemState _state; // ← Thêm dòng này

		public AuthController(AppDbContext context, IConfiguration config, ISystemState state)
		{
			_context = context;
			_config = config; // Tiêm vào đây
			_state = state;
		}
		private string CreateToken(User user)
		{
			// 1. Tạo "nội dung" vé (Claims)
			var claims = new List<Claim> {
				new Claim(ClaimTypes.Name, user.UserName),
				new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
			};

			// 2. Lấy chìa khóa từ appsettings.json
			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

			// 3. Tiến hành in vé
			var token = new JwtSecurityToken(
				issuer: _config["Jwt:Issuer"],
				audience: _config["Jwt:Audience"],
				claims: claims,
				expires: DateTime.Now.AddDays(1), // Vé có hạn 1 ngày
				signingCredentials: creds
			);

			return new JwtSecurityTokenHandler().WriteToken(token);
		}
		[AllowAnonymous]
		[HttpPost("Register")]
		public async Task<IActionResult> Register(RegisterRequset request) 
		{
			if(await _context.Users.AnyAsync(u => u.UserName == request.Username))
			{
				return BadRequest("Username đã tồn tại");
			}
			string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
			var newuser = new User
			{
				Id = Guid.NewGuid().ToString(),
				UserName = request.Username,
				PasswordHash = passwordHash,
				Email = request.Email
			};
			_context.Users.Add(newuser);
			await _context.SaveChangesAsync();
			return Ok(new { message = "Đăng ký thành công!", id = newuser.Id });
		}
		[AllowAnonymous]
		[HttpPost("Login")]
		public async Task<IActionResult> Login(LoginRequest request)
		{
				var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == request.UserName);
				if (user == null)
				{
					return BadRequest("Username không tồn tại");
				}
				bool isValidPassword = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
				if(!isValidPassword)
				{
				return BadRequest("Mật khẩu không đúng");
				}
			string token = CreateToken(user);
			return Ok(new { message = "Đăng nhập thành công!", token = token, username = user.UserName });
		}
		[HttpPost("ControlPump")]
		[Authorize]
		public async Task<IActionResult> ControlPump([FromBody] PumpRequest request)
		{
			bool bật = request.Status == 1;

			_state.IsManualMode = true;
			_state.IsPumpOn = bật;

			_context.PumpLogs.Add(new PumpLog
			{
				Action = bật ? "BẬT" : "TẮT",
				Source = "Thủ công"
			});
			await _context.SaveChangesAsync();

			return Ok(new
			{
				message = $"Đã {(bật ? "BẬT" : "TẮT")} máy bơm!",
				isPumpOn = _state.IsPumpOn,
				isManualMode = _state.IsManualMode
			});
		}

		public class PumpRequest
		{
			public int Status { get; set; } // 1: Bật, 0: Tắt
		}
	}
}
