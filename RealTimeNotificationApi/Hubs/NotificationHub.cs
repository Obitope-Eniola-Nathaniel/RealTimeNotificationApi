using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using RealTimeNotificationApi.Infrastructure;

namespace RealTimeNotificationApi.Hubs
{
    // Require JWT auth to connect to this hub
    [Authorize]
    public class NotificationHub : Hub
    {
        private readonly INotificationRepository _notificationRepository;

        public NotificationHub(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        // Called automatically when client connects
        public override async Task OnConnectedAsync()
        {
            // Get userId from JWT claims
            var userId = Context.User?.FindFirst("userId")?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                // Load undelivered notifications for this user
                var pending = await _notificationRepository
                    .GetUndeliveredForUserAsync(userId);

                // Send them to this client only
                foreach (var n in pending)
                {
                    await Clients.Caller.SendAsync("ReceiveMessage", n.Message);
                }

                // Mark them as delivered
                await _notificationRepository
                    .MarkAsDeliveredAsync(pending.Select(p => p.Id));
            }

            await base.OnConnectedAsync();
        }

        // Optional: allow client to send messages (not necessary for our demo)
        public async Task SendMessage(string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", message);
        }
    }
}
