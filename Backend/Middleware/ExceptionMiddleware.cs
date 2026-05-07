using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.Json;

namespace BEapp.Middleware
{
	public class ExceptionMiddleware
	{
		private readonly RequestDelegate _next;
		public ExceptionMiddleware(RequestDelegate next)
		{
			_next = next;
		}
		public async Task InvokeAsync(HttpContext context)
		{
			try
			{
				await _next(context);
			}
			catch (Exception ex)
			{
				await HandleExceptionAsync(context, ex);
			}
		}
		private static Task HandleExceptionAsync(HttpContext context, Exception exception)
		{
			context.Response.ContentType = "application/json";
			context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

			// Tạo ra cái hộp lỗi JSON cực đẹp
			var response = new
			{
				StatusCode = context.Response.StatusCode,
				Message = "Server đang bận tí. Lỗi cụ thể nè: " + exception.Message,
				Detailed = exception.StackTrace // Chỉ nên hiện cái này khi đang code (Debug) thôi nhé
			};

			var json = JsonSerializer.Serialize(response);
			return context.Response.WriteAsync(json);
		}
	}
}
