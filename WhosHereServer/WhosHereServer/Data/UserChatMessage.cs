using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WhosHereServer.Data
{
    public class UserChatMessage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string MessageId { get; set; }

        public AppUser Receiver { get; set; }
        public string ReceiverId { get; set; }

        public AppUser Sender { get; set; }
        public string SenderId { get; set; }

        public DateTime SendTime { get; set; }

        public string Message { get; set; }
    }
}
