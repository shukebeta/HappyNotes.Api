using Api.Framework.Extensions;
using Api.Framework.Models;
using AutoMapper;
using HappyNotes.Common;
using HappyNotes.Entities;
using HappyNotes.Models;
using Microsoft.AspNetCore.SignalR;

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
        CreateMap<PostNoteRequest, Note>()
            .ForMember(m => m.TagList, _ => _.MapFrom((src,dst) => src.Content.GetTags()))
            .ForMember(m => m.IsLong, _ => _.MapFrom((src,dst) => src.Content.IsLong()))
            .ForMember(m => m.CreatedAt, _ => _.MapFrom((src,dst) => DateTime.UtcNow.ToUnixTimeSeconds()))
            .AfterMap((src, dst) =>
            {
                dst.Tags = string.Join(" ", dst.TagList);
                dst.Content = dst.IsLong ? src.Content.GetShort() : src.Content;
            })
            ;

    }
}
