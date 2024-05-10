using System.ComponentModel;

namespace HappyNotes.Extensions;

public static class EnumExtensions
{
    public static string Description(this Enum element, params object[] extraObjets)
    {
        var type = element.GetType();
        var memberInfo = type.GetMember(element.ToString());
        var attributes = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
        var description = attributes.Length > 0 ? ((DescriptionAttribute)attributes[0]).Description : element.ToString();
        return string.Format(description, extraObjets);
    }
}
