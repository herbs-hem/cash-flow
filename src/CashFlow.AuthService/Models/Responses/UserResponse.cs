namespace AuthService.Models.Responses;

public record UserResponse(string Email, string UserName, List<string> Roles);
