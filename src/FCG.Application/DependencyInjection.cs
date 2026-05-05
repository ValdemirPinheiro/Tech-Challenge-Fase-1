using FCG.Application.UseCases.Auth;
using FCG.Application.UseCases.Games;
using FCG.Application.UseCases.Library;
using FCG.Application.UseCases.Promotions;
using FCG.Application.UseCases.Users;
using Microsoft.Extensions.DependencyInjection;

namespace FCG.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Auth
        services.AddScoped<RegisterUserUseCase>();
        services.AddScoped<LoginUseCase>();

        // Users
        services.AddScoped<ListUsersUseCase>();
        services.AddScoped<UpdateUserRoleUseCase>();
        services.AddScoped<DeleteUserUseCase>();

        // Games
        services.AddScoped<CreateGameUseCase>();
        services.AddScoped<UpdateGameUseCase>();
        services.AddScoped<DeleteGameUseCase>();
        services.AddScoped<ListGamesUseCase>();
        services.AddScoped<AcquireGameUseCase>();

        // Promotions
        services.AddScoped<CreatePromotionUseCase>();
        services.AddScoped<ListActivePromotionsUseCase>();

        // Library
        services.AddScoped<GetUserLibraryUseCase>();

        return services;
    }
}
