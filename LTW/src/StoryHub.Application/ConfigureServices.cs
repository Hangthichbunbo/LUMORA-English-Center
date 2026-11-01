using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace StoryHub.Application;

public static class ConfigureServices
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        return services;
    }
}
