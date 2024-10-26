using Api.Framework.Models;
using AutoMapper;
using HappyNotes.Entities;
using HappyNotes.Models;

namespace HappyNotes.Dto;

public class AutoMapperProfile: Profile
{
    public AutoMapperProfile()
    {
        CreateMap(typeof(PageData<>), typeof(PageData<>));
        CreateMap<User, UserDto>();
        CreateMap<UserSettings, UserSettingsDto>();
        CreateMap<Note, NoteDto>();
        CreateMap<PostMastodonApplicationRequest, MastodonApplication>();
        CreateMap<TelegramSettings, TelegramSettingsDto>();
    }
}
