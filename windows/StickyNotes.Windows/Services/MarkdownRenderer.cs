using System.Text.RegularExpressions;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml.Documents;

namespace StickyNotes.Windows.Services;

public static partial class MarkdownRenderer
{
    public static IEnumerable<Block> Render(string markdown)
    {
        var inCode = false;
        foreach (var rawLine in markdown.Replace("\r\n", "\n").Split('\n'))
        {
            if (rawLine.StartsWith("```")) { inCode = !inCode; continue; }
            if (inCode)
            {
                var code = new Paragraph { FontFamily = new Microsoft.UI.Xaml.Media.FontFamily("Consolas") };
                code.Inlines.Add(new Run { Text = rawLine });
                yield return code;
                continue;
            }

            var line = rawLine;
            var paragraph = new Paragraph { Margin = new Thickness(0, 0, 0, 8) };
            if (line.StartsWith("### ")) { paragraph.FontSize = 19; paragraph.FontWeight = FontWeights.SemiBold; line = line[4..]; }
            else if (line.StartsWith("## ")) { paragraph.FontSize = 23; paragraph.FontWeight = FontWeights.SemiBold; line = line[3..]; }
            else if (line.StartsWith("# ")) { paragraph.FontSize = 29; paragraph.FontWeight = FontWeights.Bold; line = line[2..]; }
            else if (line.StartsWith("> ")) { paragraph.FontStyle = global::Windows.UI.Text.FontStyle.Italic; paragraph.Margin = new Thickness(16, 0, 0, 8); line = line[2..]; }
            else if (BulletRegex().IsMatch(line)) line = "• " + BulletRegex().Replace(line, string.Empty);
            AddInlines(paragraph.Inlines, line);
            yield return paragraph;
        }
    }

    private static void AddInlines(InlineCollection inlines, string text)
    {
        var cursor = 0;
        foreach (Match match in InlineRegex().Matches(text))
        {
            if (match.Index > cursor) inlines.Add(new Run { Text = text[cursor..match.Index] });
            Inline inline;
            if (match.Groups[1].Success) inline = new Bold { Inlines = { new Run { Text = match.Groups[1].Value } } };
            else if (match.Groups[2].Success) inline = new Italic { Inlines = { new Run { Text = match.Groups[2].Value } } };
            else if (match.Groups[3].Success) inline = new Run { Text = match.Groups[3].Value, FontFamily = new Microsoft.UI.Xaml.Media.FontFamily("Consolas") };
            else inline = new Hyperlink { NavigateUri = new Uri(match.Groups[5].Value), Inlines = { new Run { Text = match.Groups[4].Value } } };
            inlines.Add(inline);
            cursor = match.Index + match.Length;
        }
        if (cursor < text.Length) inlines.Add(new Run { Text = text[cursor..] });
    }

    [GeneratedRegex(@"^\s*[-*+]\s+")] private static partial Regex BulletRegex();
    [GeneratedRegex(@"\*\*(.+?)\*\*|(?<!\*)\*(.+?)\*(?!\*)|`(.+?)`|\[([^]]+)\]\((https?://[^)]+)\)")]
    private static partial Regex InlineRegex();
}
