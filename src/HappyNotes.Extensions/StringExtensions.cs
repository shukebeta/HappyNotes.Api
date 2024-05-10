using System.Text.RegularExpressions;
using HappyNotes.Common;

namespace HappyNotes.Extensions;

public static partial class StringExtensions
{
    private static readonly Regex Separator = MyRegex();

    /// <summary>
    /// 4 new line in a row or <!-- more --> means a manual separated long note
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(@"\n{4}|\r{4}|(?:\r\n){4}|<!--\s*more\s*-->", RegexOptions.IgnoreCase, "")]
    private static partial Regex MyRegex();

    public static bool IsLong(this string str)
    {
        var content = str?.Trim() ?? string.Empty;
        return Separator.Match(content).Success || content.Length > Constants.ShortNotesMaxLength;
    }

    public static string GetShort(this string? str)
    {
        var parts = Separator.Split(str ?? string.Empty, 2);

        if (parts[0].Length <= Constants.ShortNotesMaxLength) return parts[0];
        return str!.Substring(0, Constants.ShortNotesMaxLength);
    }
}
