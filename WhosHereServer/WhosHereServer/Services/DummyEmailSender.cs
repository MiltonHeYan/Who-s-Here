namespace WhosHereServer.Services
{
    public class DummyEmailSender : IEmailSender
    {
        public Task SendEmailAsync(string senderEmailAddress, string senderName, string receiverEmailAddress, string subject, string plainContent, string htmlContent)
        {
            Console.WriteLine("=====================================");
            Console.WriteLine("Dummy Email Sent");
            Console.WriteLine("*************************************");
            Console.WriteLine($"Sender: {senderName} <{senderEmailAddress}>");
            Console.WriteLine($"Receiver: {receiverEmailAddress}");
            Console.WriteLine($"Subject: {subject}");
            Console.WriteLine("************** Content **************");
            Console.WriteLine(plainContent);
            Console.WriteLine("=====================================");
            return Task.CompletedTask;
        }
    }
}
