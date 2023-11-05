using Microsoft.AspNetCore.SignalR;
using Projet_Final.Models;

namespace Projet_Final.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}
