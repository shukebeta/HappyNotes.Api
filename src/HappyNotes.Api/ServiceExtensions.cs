using HappyNotes.Services;
using HappyNotes.Services.interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace HappyNotes.Api;

public static class ServiceExtensions
{
    public static void RegisterServices(this IServiceCollection services)
    {
        services.AddSingleton<IAccountService, AccountService>();
        services.AddSingleton<INoteService, NoteService>();
    }
}
