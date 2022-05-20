using WhosHereServer.Data;

namespace WhosHereServer.Services
{
    public interface IChatRelayService
    {
        /// <summary>
        /// Register the user to chat relay service, indicating the user is now online.
        /// </summary>
        /// <param name="userId"></param>
        void RegisterUser(string userId, RelayMessageReceivedEventHandler relayHandler);

        /// <summary>
        /// Unregister the user from chat relay service, indicating the user is now offline.
        /// </summary>
        /// <param name="userId"></param>
        void UnregisterUser(string userId);

        /// <summary>
        /// Send chat message to user.
        /// </summary>
        /// <remarks></remarks>
        /// <param name="chatMessage"></param>
        void SendChatMessage(UserChatMessage chatMessage);
    }

    public delegate void RelayMessageReceivedEventHandler(object sender, RelayMessageReceivedEventArgs e);

    public class RelayMessageReceivedEventArgs
    {
        public UserChatMessage ChatMessage { get; set; }

        public RelayMessageReceivedEventArgs(UserChatMessage chatMessage)
        {
            ChatMessage = chatMessage ?? throw new ArgumentNullException(nameof(chatMessage));
        }
    }
}
