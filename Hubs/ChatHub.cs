using Microsoft.AspNetCore.SignalR;
using LicencjatUG.Server.Data;
using LicencjatUG.Server.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LicencjatUG.Server.Hubs
{
    public class ChatHub : Hub
    {
        private readonly DataContext _context;
        private static Dictionary<string, string> Users = new Dictionary<string, string>();

        public ChatHub(DataContext context)
        {
            _context = context;
        }

        public async Task SendMessage(string sender, string receiver, string message)
        {
            Console.WriteLine($"[SendMessage] Otrzymano: sender={sender}, receiver={receiver}, message={message}");
            try
            {
                var msg = new Message
                {
                    Sender = sender,
                    Receiver = receiver,
                    Content = message,
                    Timestamp = DateTime.UtcNow
                };

                _context.Messages.Add(msg);
                await _context.SaveChangesAsync();
                Console.WriteLine($"Zapisano w bazie (ID={msg.Id}).");

                await Clients.All.SendAsync("ReceiveMessage", sender, message, msg.Timestamp);
                // lub => Clients.User(receiver).SendAsync(...) - ale wymaga poprawnego IUserIdProvider
            }
            catch (Exception ex)
            {
                Console.WriteLine("Błąd zapisu wiadomości: " + ex.Message);
            }
        }


        public override async Task OnConnectedAsync()
        {
            string username = Context.GetHttpContext().Request.Query["username"];
            if (!string.IsNullOrEmpty(username))
            {
                Users[Context.ConnectionId] = username;
                await Clients.All.SendAsync("UserConnected", username);
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            if (Users.ContainsKey(Context.ConnectionId))
            {
                string username = Users[Context.ConnectionId];
                Users.Remove(Context.ConnectionId);
                await Clients.All.SendAsync("UserDisconnected", username);
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}
