using System.Text.RegularExpressions;
using ReverseMarkdown;

namespace HappyNotes.Common;

public static partial class StringExtensions
{
    private static readonly Regex Separator = _Separator();
    private static readonly Regex Tags = _Tags();
    private static readonly Regex Space = _Space();
    private static readonly Regex NoteId = _NoteId();
    private static readonly Regex ImagePattern = _ImagePattern();
    private static readonly Regex ImagesSuffixPattern = _ImagesSuffixPattern();
    private static readonly Converter MarkdownConverter = new();

    /// <summary>
    /// 4 new line in a row or <!-- more --> means a manual separated long note
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(@"\n{4,}|\r{4,}|(?:\r\n){4,}|<!--\s*more\s*-->", RegexOptions.IgnoreCase, "")]
    private static partial Regex _Separator();

    [GeneratedRegex(@"\s+", RegexOptions.IgnoreCase, "")]
    private static partial Regex _Space();

    [GeneratedRegex(@"(?<=(?:^|[^\\])#)[\p{L}_\p{N}]{1,32}(?=[^\p{L}\p{N}_]|$)", RegexOptions.Singleline, "")]
    private static partial Regex _Tags();

    [GeneratedRegex(@"@[1-9][0-9]{0,31}(?=[^\d]|$)", RegexOptions.Singleline, "")]
    private static partial Regex _NoteId();

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
        return parts[0].Length <= maxLength ? parts[0] : str[..maxLength];
    }

    public static List<string> GetNoteIds(this string? str, int maxIdLength = 33, int maxTotalLength = 160)
    {
        if (str is null) return [];
        var matches = NoteId.Matches(str);
        if (matches.Count == 0) return [];
        var noteIds = new List<string>();
        var totalLength = 0;
        // Join multiple match into one string, then split it to one unique list
        var candidate = Space.Split(string.Join(' ', matches)).Distinct();
        foreach (var tag in candidate)
        {
            if (tag.Length > maxIdLength)
                continue;

            // Check if adding this tag (plus a space) would exceed the max total length
            totalLength += tag.Length + 1; // +1 for the space
            if (totalLength > maxTotalLength)
                break;
            noteIds.Add(tag);
        }

        return noteIds;
    }

    public static List<string> GetTags(this string? str, int maxTagLength = 32, int maxTotalLength = 352)
    {
        if (str is null) return [];
        var matches = Tags.Matches(str);
        var tags = new List<string>();
        if (matches.Any())
        {
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

        }
        return tags.Concat(str.GetNoteIds()).ToList();
    }

    public static bool IsHtml(this string input)
    {
        return Regex.IsMatch(input, @"<\s*([a-zA-Z]+)[^>]*>.*</\s*\1\s*>",
                   RegexOptions.Singleline | RegexOptions.IgnoreCase)
               || Regex.IsMatch(input, @"<\s*([a-zA-Z]+)*[^>]*/>", RegexOptions.IgnoreCase);
    }

    public static string ToMarkdown(this string htmlInput)
    {
        return IsHtml(htmlInput) ? MarkdownConverter.Convert(htmlInput).Trim() : htmlInput;
    }

    public static List<(string Alt, string ImgUrl, Match Match)> GetImgInfos(this string? markdownInput)
    {
        if (markdownInput is null) return [];
        var matches = ImagePattern.Matches(markdownInput);
        var imgInfos = new List<(string Alt, string ImgUrl, Match Match)>();
        if (matches.Count == 0) return imgInfos;
        foreach (Match match in matches)
        {
            var altText = match.Groups[1].Value.Trim();
            var imageUrl = match.Groups[2].Value.Trim();
            imgInfos.Add((altText, imageUrl, match));
        }
        return imgInfos;
    }

    public static string RemoveImageReference(this string? markdownInput)
    {
        if (markdownInput is null) return string.Empty;
        var match = ImagesSuffixPattern.Match(markdownInput);
        if (match.Success)
        {
            markdownInput = markdownInput.Replace(match.Value, " ").Trim();
        }
        return markdownInput;
    }

    [GeneratedRegex(@"!\[(.*?)\]\((.*?)\)", RegexOptions.Compiled)]
    private static partial Regex _ImagePattern();
    [GeneratedRegex(@"((?:\s*image\s*\d\s*)+)", RegexOptions.Compiled | RegexOptions.Singleline)]
    private static partial Regex _ImagesSuffixPattern();
}
