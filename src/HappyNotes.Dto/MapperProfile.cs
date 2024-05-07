using AutoMapper;
using HappyNotes.Entities;

namespace HappyNotes.Dto;

public class MapperProfile: Profile
{
    public MapperProfile()
    {
        CreateMap<User, UserDto>();
    }
}