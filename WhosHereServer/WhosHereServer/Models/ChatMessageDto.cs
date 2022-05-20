namespace WhosHereServer.Models
{
    public class SendChatMessageDto
    {
        public string ReceiverId { get; set; }

        public string Message { get; set; }
    }

    public class ReceiveChatMessageDto
    {
        public string SenderId { get; set; }

        public DateTime SendTime { get; set; }

        public string Message { get; set; }
    }
}
