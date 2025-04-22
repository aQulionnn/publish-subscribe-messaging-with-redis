namespace Subscriber.Entities;

public class InboxMessage
{
    public Guid Id { get; init; }
    public string Payload { get; set; }
    public bool Processed { get; set; } = false;
}