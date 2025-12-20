namespace SOLFranceBackend.Events
{
    public record NotificationEvent : IntegrationEvent
    {
        public string? Message { get; set; }
    }
}
