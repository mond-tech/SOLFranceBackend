namespace SOLFranceBackend.Interfaces
{
    public interface IEmailQueue
    {
        void QueueEmail(string to, string subject, string body);
    }

}
