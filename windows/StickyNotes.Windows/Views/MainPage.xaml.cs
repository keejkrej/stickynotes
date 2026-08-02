using Microsoft.UI.Text;
using Microsoft.UI.Xaml.Documents;
using StickyNotes.Windows.Services;

namespace StickyNotes.Windows.Views;

public sealed partial class MainPage : Page
{
    private readonly DispatcherTimer _saveTimer = new() { Interval = TimeSpan.FromMilliseconds(500) };
    public MainViewModel ViewModel { get; } = new();
    public bool HasSelection => ViewModel.SelectedNote is not null;
    public Visibility EditorVisibility => ViewModel.IsPreview ? Visibility.Collapsed : Visibility.Visible;
    public Visibility PreviewVisibility => ViewModel.IsPreview ? Visibility.Visible : Visibility.Collapsed;

    public MainPage()
    {
        InitializeComponent();
        _saveTimer.Tick += SaveTimer_Tick;
        ViewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(ViewModel.SelectedNote))
            {
                Bindings.Update();
                RenderMarkdown();
            }
            if (args.PropertyName == nameof(ViewModel.IsPreview))
            {
                Bindings.Update();
                RenderMarkdown();
            }
        };
        Loaded += async (_, _) =>
        {
            App.MainWindow.SetTitleBar(DragRegion);
            await ViewModel.InitializeAsync();
        };
    }

    private void Editor_TextChanged(object sender, TextChangedEventArgs e) => ScheduleSave();
    private void Close_Click(object sender, RoutedEventArgs e) => App.MainWindow.Close();

    private void ScheduleSave()
    {
        if (ViewModel.SelectedNote is null) return;
        _saveTimer.Stop();
        _saveTimer.Start();
        if (ViewModel.IsPreview) RenderMarkdown();
    }

    private async void SaveTimer_Tick(object? sender, object e)
    {
        _saveTimer.Stop();
        await ViewModel.NoteChangedAsync();
    }

    private void RenderMarkdown()
    {
        MarkdownPreview.Blocks.Clear();
        foreach (var block in MarkdownRenderer.Render(ViewModel.SelectedNote?.Content ?? string.Empty))
            MarkdownPreview.Blocks.Add(block);
    }

}
