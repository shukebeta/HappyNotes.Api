using Api.Framework.Exceptions;
using Api.Framework.Helper;
using HappyNotes.Common;

namespace HappyNotes.Extensions;

public static class ExceptionHelper
{
    public static CustomException<object> New(object data, EventId eventId, params object[] extraObjects)
    {
        return CustomExceptionHelper.New(data, (int) eventId, eventId.Description(extraObjects));
    }
}
