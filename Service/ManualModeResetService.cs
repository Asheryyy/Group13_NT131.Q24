// Services/ManualModeResetService.cs
using static BEapp.Interface.ISystemCreate;

namespace BEapp.Services
{
	public class ManualModeResetService : BackgroundService
	{
		private readonly ISystemState _state;
		private readonly ILogger<ManualModeResetService> _logger;

		public ManualModeResetService(ISystemState state, ILogger<ManualModeResetService> logger)
		{
			_state = state;
			_logger = logger;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				// Kiểm tra mỗi 30 giây
				await Task.Delay(30000, stoppingToken);

				if (_state.IsManualMode && _state.LastManualCommandTime.HasValue)
				{
					var timeSinceLastCommand = DateTime.Now - _state.LastManualCommandTime.Value;

					if (timeSinceLastCommand.TotalMinutes >= 1)
					{
						_state.IsManualMode = false;
						_state.IsPumpOn = false;
						_logger.LogInformation("Đã reset về chế độ tự động sau 1 phút không có lệnh!");
					}
				}
			}
		}
	}
}