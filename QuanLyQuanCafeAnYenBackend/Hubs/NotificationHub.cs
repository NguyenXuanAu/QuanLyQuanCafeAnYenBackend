using Microsoft.AspNetCore.SignalR;

namespace QuanLyQuanCafeAnYenBackend.Hubs
{
    public class NotificationHub : Hub
    {
        // Hàm này được gọi từ máy của Lai (hoặc bất kỳ Admin nào)
        public async Task SendOrderNotification(string message)
        {
            // Clients.All sẽ gửi tin nhắn đến TẤT CẢ các máy đang mở trang web
            // kể cả các Admin khác đang trực máy
            await Clients.All.SendAsync("ReceiveOrderUpdate", new { msg = message });
        }
    }
}