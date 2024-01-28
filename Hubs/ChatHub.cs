using Microsoft.AspNetCore.SignalR;
using Projet_Final.Models;

namespace Projet_Final.Hubs
{
    public class ChatHub : Hub
    {
        private static int utilisateursConnectes = 0;
        private static List<string> messages = new List<string>();

        public override async Task OnConnectedAsync()
        {
            utilisateursConnectes++;
            await Clients.Caller.SendAsync("UserConnected", utilisateursConnectes);

            // Envoyer les messages précédents à l'utilisateur qui vient de se connecter
            await Clients.Client(Context.ConnectionId).SendAsync("LoadPreviousMessages", messages);

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            utilisateursConnectes--;

            // Envoyer le nombre d'utilisateurs connectés uniquement au client qui se déconnecte
            await Clients.Client(Context.ConnectionId).SendAsync("UserConnected", utilisateursConnectes);

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(string utilisateur, string message)
        {
            var timestamp = DateTime.UtcNow.ToString("o");
            var formattedMessage = $"[{timestamp}] {utilisateur}: {message}";

            messages.Add(formattedMessage);

            await Clients.All.SendAsync("ReceiveMessage", utilisateur, message, timestamp, utilisateursConnectes);
        }

        private string FormatMessage(string message)
        {
            // Extraire la date du message existant
            var dateIndex = message.IndexOf('[') + 1;
            var dateString = message.Substring(dateIndex, message.IndexOf(']') - dateIndex);

            // Parser la date et la reformater
            if (DateTime.TryParse(dateString, out DateTime originalDate))
            {
                return message.Replace(dateString, originalDate.ToString("yyyy/MM/dd"));
            }

            return message;
        }
    }
}
