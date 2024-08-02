using HappyNotes.Repositories;
using HappyNotes.Repositories.interfaces;
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
        services.AddSingleton<INoteTagService, NoteTagService>();
        services.AddSingleton<INoteRepository, NoteRepository>();
        services.AddSingleton<ITelegramService, TelegramService>();
    }
}
