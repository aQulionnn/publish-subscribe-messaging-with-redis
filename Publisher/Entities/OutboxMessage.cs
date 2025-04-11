namespace Publisher.Entities;

public class OutboxMessage
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Type { get; set; }
    public string Payload { get; set; }
    public DateTime? ProcessedAt { get; set; }
}