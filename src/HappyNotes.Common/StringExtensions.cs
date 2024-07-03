using System.Text.RegularExpressions;
using HappyNotes.Common;

namespace HappyNotes.Extensions;

public static partial class StringExtensions
{
    private static readonly Regex Separator = _Separator();
    private static readonly Regex Tags = _Tags();
    private static readonly Regex Space = _Space();

    /// <summary>
    /// 4 new line in a row or <!-- more --> means a manual separated long note
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(@"\n{4,}|\r{4,}|(?:\r\n){4,}|<!--\s*more\s*-->", RegexOptions.IgnoreCase, "")]
    private static partial Regex _Separator();

    [GeneratedRegex(@"\s+", RegexOptions.IgnoreCase, "")]
    private static partial Regex _Space();


    [GeneratedRegex(@"(?<=#)[\p{L}_\p{N}]+(?:\s[\p{L}_\p{N}]+)*", RegexOptions.IgnoreCase, "")]
    private static partial Regex _Tags();

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

    public static string[] GetTags(this string? content)
    {
        content ??= string.Empty;
        var match = Tags.Match(content);
        if (match.Success)
        {
            return Space.Split(match.Value);
        }

        return [];
    }
}
