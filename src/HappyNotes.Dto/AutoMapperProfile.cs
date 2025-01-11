using System.Globalization;
using Api.Framework.Extensions;
using Api.Framework.Models;
using AutoMapper;
using HappyNotes.Common;
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
        CreateMap<PostNoteRequest, Note>()
            .ForMember(m => m.TagList, m => m.MapFrom((src,_) => src.Content.GetTags()))
            .ForMember(m => m.IsLong, m => m.MapFrom((src,_) => src.Content.IsLong()))
            .ForMember(m => m.CreatedAt, m => m.MapFrom<CreatedAtResolver>())
            .AfterMap((src, dst) =>
            {
                dst.Tags = string.Join(" ", dst.TagList);
                dst.Content = dst.IsLong ? src.Content.GetShort() : src.Content ?? string.Empty;
            })
            ;

    }
}

internal class CreatedAtResolver : IValueResolver<PostNoteRequest, Note, long>
{
    public long Resolve(PostNoteRequest source, Note destination, long member, ResolutionContext context)
    {
        if (string.IsNullOrWhiteSpace(source.PublishDateTime) || string.IsNullOrWhiteSpace(source.TimezoneId))
            return DateTime.UtcNow.ToUnixTimeSeconds();

        var dateStr = source.PublishDateTime;
        if (dateStr.Length.Equals(10)) dateStr += " 20:00:00";

        DateTime date = DateTime.ParseExact(dateStr, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(source.TimezoneId!);

        return TimeZoneInfo.ConvertTime(new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second, DateTimeKind.Unspecified), timeZone).ToUnixTimeSeconds();
    }
}
