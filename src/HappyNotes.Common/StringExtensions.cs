using System.Text.RegularExpressions;
namespace HappyNotes.Common;

public static partial class StringExtensions
{
    private static readonly Regex Separator = _Separator();
    private static readonly Regex TagsPattern = _Tags();
    private static readonly Regex Space = _Space();

    /// <summary>
    /// 4 new line in a row or <!-- more --> means a manual separated long note
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(@"\n{4,}|\r{4,}|(?:\r\n){4,}|<!--\s*more\s*-->", RegexOptions.IgnoreCase, "")]
    private static partial Regex _Separator();

    [GeneratedRegex(@"\s+", RegexOptions.IgnoreCase, "")]
    private static partial Regex _Space();


    [GeneratedRegex(@"(?<=#)[\p{L}_\p{N}]{1,32}(?=[^\p{L}\p{N}_]|$)", RegexOptions.Singleline, "")]
    private static partial Regex _Tags();

    public static bool IsLong(this string? str)
    {
        var content = str?.Trim() ?? string.Empty;
        return Separator.Match(content).Success || content.Length > Constants.ShortNotesMaxLength;
    }

    public static string GetShort(this string? str, int maxLength = 0)
    {
        if (str is null) return string.Empty;
        var parts = Separator.Split(str, 2);

        maxLength = maxLength <= 0 ? Constants.ShortNotesMaxLength : maxLength;
        if (parts[0].Length <= maxLength) return parts[0];
        return str!.Substring(0, maxLength);
    }

    public static List<string> GetTags(this string? str, int maxTagLength = 32, int maxTotalLength = 512)
    {
        if (str is null) return [];
        var matches = TagsPattern.Matches(str);
        if (matches.Any())
        {
            var tags = new List<string>();
            var totalLength = 0;
            var candidate = Space.Split(string.Join(' ', matches)) // first join multiple match into one string, then split it to one array
                .Select(m => m.ToLower()).Distinct();
            foreach (var tag in candidate)
            {
                if (tag.Length > maxTagLength)
                    continue;

                // Check if adding this tag (plus a space) would exceed the max total length
                totalLength += tag.Length + 1; // +1 for the space
                if (totalLength > maxTotalLength)
                    break;
                tags.Add(tag);
            }

            return tags;
        }

        return [];
    }

    public static string GetEscapedMarkdown(this string text)
    {
        return Regex.Replace(text, @"[_*\[\]()~>#`+\-=|{}.!]", @"\$0", RegexOptions.Singleline, TimeSpan.FromSeconds(10));
    }
}
