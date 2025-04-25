using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Publisher.Entities;

public class OutboxMessage
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Type { get; set; }
    public string Payload { get; set; }
    public DateTime? ProcessedAt { get; set; }
}