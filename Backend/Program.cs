using BEapp.Data.DataBase;
using BEapp.Hubs;
using BEapp.Interface;
using BEapp.Middleware;
using BEapp.Repository;
using BEapp.Service;
using BEapp.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using System.Text;
using static BEapp.Service.HumidityService;
var cultureInfo = new CultureInfo("en-US");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
var builder = WebApplication.CreateBuilder(args);
// 1. Khai báo chính sách "Mở cửa cho tất cả"
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowAll", policy =>
	{
		policy.WithOrigins("null") // Quan trọng: khi mở file html trực tiếp từ ổ cứng, origin nó là "null"
			  .AllowAnyHeader()
			  .AllowAnyMethod()
			  .AllowCredentials()
			  .SetIsOriginAllowed(_ => true); // Cho phép mọi nguồn
	});
});
// Add services to the container.
builder.Services.AddSignalR();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	.AddJwtBearer(options =>
	{
		options.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuer = true,
			ValidateAudience = true,
			ValidateLifetime = true,
			ValidateIssuerSigningKey = true,
			ValidIssuer = builder.Configuration["Jwt:Issuer"],
			ValidAudience = builder.Configuration["Jwt:Audience"],
			IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
		};
	});

builder.Services.AddAuthorization();
// Bảo với ASP.NET: "Hễ thằng nào cần HumidityService thì hãy tạo một cái cho nó!"
builder.Services.AddScoped<HumidityService>();

// Nếu m có IRepository và SqlRepository thì đăng ký luôn ở đây
builder.Services.AddScoped<IRepository, SqlRepository>();
builder.Services.AddSingleton<ISystemCreate.ISystemState, ISystemCreate.SystemState>();
// 1. Lấy chuỗi kết nối từ appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 2. Đăng nhập DbContext vào hệ thống Dependency Injection
builder.Services.AddDbContext<AppDbContext>(options =>
	options.UseSqlServer(connectionString));
builder.Services.AddHostedService<ManualModeResetService>();
var app = builder.Build();
app.UseCors("AllowAll"); // Phải nằm TRƯỚC MapHub và MapControllers

app.UseSwagger();
app.UseSwaggerUI(c =>
{
	c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");

	// Thêm dòng này để khi vào link gốc (không cần gõ /swagger) nó hiện ra luôn
	c.RoutePrefix = string.Empty;
});
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.UseAuthorization();

app.UseMiddleware<ExceptionMiddleware>();

app.MapHub<HumidityHub>("/humidityHub");

app.MapControllers();

app.Run();
