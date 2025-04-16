namespace CashFlow.Domain.Entities;

public sealed record UserRole(
    Guid UserId,
    Guid RoleId)
{
    public User User { get; set; }
    public Role Role { get; set; }
}