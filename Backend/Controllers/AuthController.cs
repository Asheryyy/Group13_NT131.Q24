using BEapp.Data.DataBase;
using BEapp.Interface;
using BEapp.Models.Entities;
using MailKit.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MimeKit;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static BEapp.Interface.ISystemCreate;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

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
			_state.LastManualCommandTime = DateTime.Now;

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
		// POST: api/Auth/ForgotPassword - Quên mật khẩu
		[AllowAnonymous]
		[HttpPost("ForgotPassword")]
		public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
		{
			// 1. Kiểm tra email tồn tại không
			var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
			if (user == null)
			{
				return BadRequest(new { message = "Email không tồn tại!" });
			}

			// 2. Tạo mật khẩu mới random 8 ký tự
			var newPassword = GenerateRandomPassword();

			// 3. Hash và lưu vào DB
			user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
			await _context.SaveChangesAsync();

			// 4. Gửi email
			try
			{
				var email = new MimeMessage();
				email.From.Add(MailboxAddress.Parse(_config["EmailSettings:FromEmail"]));
				email.To.Add(MailboxAddress.Parse(request.Email));
				email.Subject = "Mật khẩu mới của bạn";
				email.Body = new TextPart("html")
				{
					Text = $@"
                <h2>Khôi phục mật khẩu</h2>
                <p>Mật khẩu mới của bạn là: <strong style='font-size:20px'>{newPassword}</strong></p>
                <p>Vui lòng đăng nhập và đổi mật khẩu ngay!</p>
            "
				};

				using var smtp = new SmtpClient();
				await smtp.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
				await smtp.AuthenticateAsync(
					_config["EmailSettings:FromEmail"],
					_config["EmailSettings:Password"]
				);
				await smtp.SendAsync(email);
				await smtp.DisconnectAsync(true);

				return Ok(new { message = "Mật khẩu mới đã gửi về email!" });
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = $"Lỗi gửi email: {ex.Message}" });
			}
		}

		// POST: api/Auth/ChangePassword - Đổi mật khẩu
		[Authorize]
		[HttpPost("ChangePassword")]
		public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
		{
			// Lấy username từ JWT token
			var username = User.Identity?.Name;
			var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == username);

			if (user == null)
				return BadRequest(new { message = "Người dùng không tồn tại!" });

			// Kiểm tra mật khẩu cũ
			if (!BCrypt.Net.BCrypt.Verify(request.OldPassword, user.PasswordHash))
				return BadRequest(new { message = "Mật khẩu cũ không đúng!" });

			// Lưu mật khẩu mới
			user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
			await _context.SaveChangesAsync();

			return Ok(new { message = "Đổi mật khẩu thành công!" });
		}

		// Hàm tạo mật khẩu random
		private string GenerateRandomPassword()
		{
			const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789";
			var random = new Random();
			return new string(Enumerable.Repeat(chars, 8)
				.Select(s => s[random.Next(s.Length)]).ToArray());
		}

		// Request classes
		public class ForgotPasswordRequest
		{
			public string Email { get; set; } = "";
		}

		public class ChangePasswordRequest
		{
			public string OldPassword { get; set; } = "";
			public string NewPassword { get; set; } = "";
		}

		public class PumpRequest
		{
			public int Status { get; set; } // 1: Bật, 0: Tắt
		}
	}
}
