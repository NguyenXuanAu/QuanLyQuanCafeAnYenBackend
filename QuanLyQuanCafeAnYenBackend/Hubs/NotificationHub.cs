using Microsoft.AspNetCore.SignalR;

namespace QuanLyQuanCafeAnYenBackend.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task SendOrderNotification(string message, string senderConnectionId)
        {
            if (string.IsNullOrEmpty(senderConnectionId))
            {
                // Nếu không có connectionId thì gửi tất cả
                await Clients.All.SendAsync("ReceiveOrderUpdate", new { msg = message });
            }
            else
            {
                // Gửi cho tất cả TRỪ người bấm
                await Clients.AllExcept(senderConnectionId)
                    .SendAsync("ReceiveOrderUpdate", new { msg = message });
            }
        }
    }
}