using Microsoft.AspNetCore.SignalR;

namespace BEapp.Hubs
{
	// Thằng này kế thừa từ Hub của SignalR
	public class HumidityHub : Hub
	{
		// Hiện tại để trống cũng được, vì mình chỉ dùng để Server "đẩy" tin xuống
		// Sau này m muốn App gửi tin lên Hub thì viết hàm ở đây
	}
}