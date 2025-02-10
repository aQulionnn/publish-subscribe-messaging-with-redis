namespace Contracts;

public sealed record Message(Guid Id, DateTime CreatedOn, string Description);
