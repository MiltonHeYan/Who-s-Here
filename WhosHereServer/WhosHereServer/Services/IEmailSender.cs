using System.Runtime.Serialization;

namespace WhosHereServer.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string senderEmailAddress, string senderName, string receiverEmailAddress, string subject, string plainContent, string htmlContent);
    }

    /// <summary>
    /// Throwns when the <see cref="IEmailSender"/> failed to send an email.
    /// </summary>
    public class EmailException : Exception
    {
        public EmailException()
        {
        }

        public EmailException(string? message) : base(message)
        {
        }

        public EmailException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

    }
}
