using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Subscriber.Entities;

public class InboxMessage
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; init; }
    public string Payload { get; set; }
    public bool Processed { get; set; } = false;
}