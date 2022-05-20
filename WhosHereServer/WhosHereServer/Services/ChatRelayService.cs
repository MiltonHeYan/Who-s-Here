using WhosHereServer.Data;

namespace WhosHereServer.Services
{
    public class ChatRelayService : IChatRelayService
    {
        private readonly IServiceScopeFactory scopeFactory;
        private Dictionary<string, RelayMessageReceivedEventHandler> sendingQueues;

        public ChatRelayService(IServiceScopeFactory scopeFactory)
        {
            this.scopeFactory = scopeFactory;
            sendingQueues = new Dictionary<string, RelayMessageReceivedEventHandler>();
        }

        // TODO: broken when the event does not have any subscribe 
        public void RegisterUser(string userId, RelayMessageReceivedEventHandler relayHandler)
        {
            sendingQueues[userId] = relayHandler;
        }

        public void SendChatMessage(UserChatMessage chatMessage)
        {
            if (sendingQueues.TryGetValue(chatMessage.ReceiverId, out RelayMessageReceivedEventHandler handler))
            {
                handler?.Invoke(this, new RelayMessageReceivedEventArgs(chatMessage));
            }
            else
            {
                CacheChatMessage(chatMessage);
            }
        }

        public void UnregisterUser(string userId)
        {
            sendingQueues.Remove(userId);
        }

        private void CacheChatMessage(params UserChatMessage[] chatMessage)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                context.UserChatMessage.AddRangeAsync(chatMessage);
                context.SaveChanges();
            }
            
        }
    }
}
