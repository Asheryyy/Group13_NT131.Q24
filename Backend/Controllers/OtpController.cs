using BEapp.Data.DataBase;
using BEapp.Models.Entities;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimeKit;

namespace BEapp.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class OtpController : ControllerBase
	{
		private readonly AppDbContext _context;
		private readonly IConfiguration _config;

		public OtpController(AppDbContext context, IConfiguration config)
		{
			_context = context;
			_config = config;
		}

		// POST: api/Otp/Send - Gửi OTP về email
		[HttpPost("Send")]
		public async Task<IActionResult> SendOtp([FromBody] SendOtpRequest request)
		{
			// 1. Tạo OTP 6 số
			var otp = new Random().Next(100000, 999999).ToString();

			// 2. Lưu vào DB
			_context.OtpRecords.Add(new OtpRecord
			{
				Email = request.Email,
				OtpCode = otp,
				ExpiredAt = DateTime.Now.AddMinutes(10), // Hết hạn sau 10 phút
				IsUsed = false
			});
			await _context.SaveChangesAsync();

			// 3. Gửi email
			try
			{
				var email = new MimeMessage();
				email.From.Add(MailboxAddress.Parse(_config["EmailSettings:FromEmail"]));
				email.To.Add(MailboxAddress.Parse(request.Email));
				email.Subject = "Mã OTP xác thực tài khoản";
				email.Body = new TextPart("html")
				{
					Text = $@"
                        <h2>Xác thực tài khoản</h2>
                        <p>Mã OTP của bạn là: <strong style='font-size:24px'>{otp}</strong></p>
                        <p>Mã có hiệu lực trong <strong>10 phút</strong></p>
                        <p>Nếu bạn không yêu cầu, hãy bỏ qua email này.</p>
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

				return Ok(new { message = "Đã gửi OTP về email!" });
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = $"Lỗi gửi email: {ex.Message}" });
			}
		}

		// POST: api/Otp/Verify - Xác thực OTP
		[HttpPost("Verify")]
		public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
		{
			var otpRecord = await _context.OtpRecords
				.Where(x => x.Email == request.Email
					&& x.OtpCode == request.OtpCode
					&& !x.IsUsed
					&& x.ExpiredAt > DateTime.Now)
				.OrderByDescending(x => x.ExpiredAt)
				.FirstOrDefaultAsync();

			if (otpRecord == null)
			{
				return BadRequest(new { message = "OTP không hợp lệ hoặc đã hết hạn!" });
			}

			// Đánh dấu OTP đã dùng
			otpRecord.IsUsed = true;
			await _context.SaveChangesAsync();

			return Ok(new { message = "Xác thực OTP thành công!" });
		}
	}

	public class SendOtpRequest
	{
		public string Email { get; set; } = "";
	}

	public class VerifyOtpRequest
	{
		public string Email { get; set; } = "";
		public string OtpCode { get; set; } = "";
	}
}