namespace CashFlow.ApiGateway.Policies;

using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

public static class AuthorizationPolicies
{
    public static IServiceCollection AddCustomAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy("Service1Policy", policy =>
                policy.RequireRole("Admin", "User"));
        });

        return services;
    }
}