namespace WhosHereServer.Services
{
    public class MailGunEmailSender : IEmailSender
    {
        public Task SendEmailAsync(string senderEmailAddress, string senderName, string receiverEmailAddress, string subject, string plainContent, string htmlContent)
        {
            throw new NotImplementedException();
        }
    }
}
