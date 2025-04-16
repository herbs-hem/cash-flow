namespace CashFlow.Application.Dtos;

public sealed record UserDto(Guid Id, string Username, string Email, string Password, List<string> Roles);