using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using MdInline = Markdig.Syntax.Inlines.Inline;
using XamlInlineCollection = Microsoft.UI.Xaml.Documents.InlineCollection;

namespace StickyNotes.Windows.Services;

public static class MarkdownRenderer
{
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();

    public static IEnumerable<UIElement> Render(string markdown)
    {
        foreach (var block in Markdown.Parse(markdown, Pipeline))
            yield return RenderBlock(block);
    }

    private static UIElement RenderBlock(Markdig.Syntax.Block block) => block switch
    {
        HeadingBlock heading => RenderLeaf(heading, heading.Level switch
        {
            1 => 18,
            2 => 16,
            _ => 14
        }, heading.Level == 1 ? FontWeights.Bold : FontWeights.SemiBold),
        ParagraphBlock paragraph => RenderLeaf(paragraph, 12, FontWeights.Normal),
        ListBlock list => RenderList(list),
        QuoteBlock quote => RenderQuote(quote),
        CodeBlock code => RenderCode(code),
        ThematicBreakBlock => new Border
        {
            Height = 1,
            Margin = new Thickness(0, 5, 0, 7),
            Background = (Brush)Application.Current.Resources["DividerStrokeColorDefaultBrush"]
        },
        ContainerBlock container => RenderContainer(container),
        _ => new TextBlock { Text = block.ToString(), FontSize = 12, TextWrapping = TextWrapping.Wrap }
    };

    private static TextBlock RenderLeaf(LeafBlock block, double fontSize, global::Windows.UI.Text.FontWeight weight)
    {
        var text = new TextBlock
        {
            FontSize = fontSize,
            FontWeight = weight,
            Margin = new Thickness(0, 0, 0, 5),
            TextWrapping = TextWrapping.Wrap,
            IsTextSelectionEnabled = true
        };
        if (block.Inline is not null) AppendInlines(text.Inlines, block.Inline);
        return text;
    }

    private static UIElement RenderList(ListBlock list)
    {
        var stack = new StackPanel { Spacing = 3, Margin = new Thickness(0, 0, 0, 5) };
        var number = 1;
        foreach (var child in list)
        {
            if (child is not ListItemBlock item) continue;
            var row = new Grid { ColumnSpacing = 5 };
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            row.Children.Add(new TextBlock
            {
                Text = list.IsOrdered ? $"{number++}." : "•",
                FontSize = 12,
                MinWidth = 14,
                TextAlignment = TextAlignment.Right
            });
            var content = RenderContainer(item);
            Grid.SetColumn(content, 1);
            row.Children.Add(content);
            stack.Children.Add(row);
        }
        return stack;
    }

    private static UIElement RenderQuote(QuoteBlock quote)
    {
        return new Border
        {
            BorderBrush = (Brush)Application.Current.Resources["DividerStrokeColorDefaultBrush"],
            BorderThickness = new Thickness(2, 0, 0, 0),
            Padding = new Thickness(9, 0, 0, 0),
            Margin = new Thickness(0, 0, 0, 5),
            Child = RenderContainer(quote)
        };
    }

    private static TextBlock RenderCode(CodeBlock code) => new()
    {
        Text = code.Lines.ToString(),
        FontFamily = new FontFamily("Cascadia Mono, Consolas"),
        FontSize = 12,
        Margin = new Thickness(0, 0, 0, 5),
        TextWrapping = TextWrapping.Wrap,
        IsTextSelectionEnabled = true
    };

    private static StackPanel RenderContainer(ContainerBlock container)
    {
        var stack = new StackPanel();
        foreach (var child in container) stack.Children.Add(RenderBlock(child));
        return stack;
    }

    private static void AppendInlines(XamlInlineCollection destination, ContainerInline source)
    {
        for (var inline = source.FirstChild; inline is not null; inline = inline.NextSibling)
            destination.Add(RenderInline(inline));
    }

    private static Microsoft.UI.Xaml.Documents.Inline RenderInline(MdInline inline) => inline switch
    {
        LiteralInline literal => new Run { Text = literal.Content.ToString() },
        CodeInline code => new Run { Text = code.Content, FontFamily = new FontFamily("Cascadia Mono, Consolas") },
        LineBreakInline => new LineBreak(),
        EmphasisInline emphasis => RenderEmphasis(emphasis),
        LinkInline link => RenderLink(link),
        HtmlInline html => new Run { Text = html.Tag },
        ContainerInline container => RenderSpan(container),
        _ => new Run { Text = inline.ToString() }
    };

    private static Span RenderEmphasis(EmphasisInline emphasis)
    {
        Span span = emphasis.DelimiterCount >= 2 ? new Bold() : new Italic();
        AppendInlines(span.Inlines, emphasis);
        return span;
    }

    private static Microsoft.UI.Xaml.Documents.Inline RenderLink(LinkInline link)
    {
        if (Uri.TryCreate(link.Url, UriKind.Absolute, out var uri) && uri.Scheme is "http" or "https")
        {
            var hyperlink = new Hyperlink { NavigateUri = uri };
            AppendInlines(hyperlink.Inlines, link);
            return hyperlink;
        }
        return RenderSpan(link);
    }

    private static Span RenderSpan(ContainerInline container)
    {
        var span = new Span();
        AppendInlines(span.Inlines, container);
        return span;
    }
}
