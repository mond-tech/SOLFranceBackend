using System.Threading.Channels;

namespace SOLFranceBackend.Interfaces.Implementation
{
    public class EmailQueue : IEmailQueue
    {
        private readonly Channel<EmailMessage> _channel =
            Channel.CreateUnbounded<EmailMessage>();

        public ChannelReader<EmailMessage> Reader => _channel.Reader;

        public void QueueEmail(string to, string subject, string body)
        {
            _channel.Writer.TryWrite(new EmailMessage(to, subject, body));
        }
    }

    public record EmailMessage(string To, string Subject, string Body);

}
