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

            // Envoyer le nombre d'utilisateurs connectés à tous les clients
            await Clients.All.SendAsync("UserConnected", utilisateursConnectes);

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

        private bool verifierMessage(string message)
        {
            List<string> motsIndesirables = new List<string> { "pute", "merde", "salope", "fuck you", "bitch", "con", "criss" };

            foreach (var mot in motsIndesirables)
            {
                if (message.Contains(mot, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            return true;
        }

        public async Task DeleteMessage(string user, string message, DateTime timestamp)
        {
            var userIdIndex = message.IndexOf("[UserId:");
            if (userIdIndex >= 0)
            {
                var userIdEndIndex = message.IndexOf("]", userIdIndex);
                if (userIdEndIndex >= 0)
                {
                    var userId = message.Substring(userIdIndex + "[UserId:".Length, userIdEndIndex - userIdIndex - "[UserId:".Length);

                    // Supprimer le message uniquement si l'ID utilisateur correspond à l'ID actuel
                    if (userId == Context.ConnectionId)
                    {
                        var formattedMessage = $"{user}: {message} [{timestamp}]";
                        messages.Remove(formattedMessage);

                        await Clients.All.SendAsync("DeleteMessage", user, message, timestamp, utilisateursConnectes);
                    }
                }
            }
        }
    }
}
