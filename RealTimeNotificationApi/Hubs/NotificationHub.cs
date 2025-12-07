using Microsoft.AspNetCore.SignalR;

namespace RealTimeNotificationApi.Hubs
{
    public class NotificationHub : Hub
    {
        // Optional: a method if client wants to call server explicitly
        public async Task SendMessage(string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", message);
        }
    }
}
