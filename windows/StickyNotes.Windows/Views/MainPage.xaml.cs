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
                NotePicker.Text = ViewModel.SelectedNote?.Title ?? string.Empty;
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
            NotePicker.Text = ViewModel.SelectedNote?.Title ?? string.Empty;
        };
    }

    private void Editor_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (ReferenceEquals(sender, TitleEditor) && ViewModel.SearchText.Length == 0)
            NotePicker.Text = TitleEditor.Text;
        ScheduleSave();
    }
    private void NotePicker_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            ViewModel.SearchText = sender.Text;
    }

    private void NotePicker_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
    {
        if (args.SelectedItem is not StickyNotes.Windows.Models.Note note) return;
        SelectNote(sender, note);
    }

    private void NotePicker_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        if (args.ChosenSuggestion is StickyNotes.Windows.Models.Note note)
            SelectNote(sender, note);
        else if (ViewModel.Notes.FirstOrDefault() is { } first)
            SelectNote(sender, first);
    }

    private void SelectNote(AutoSuggestBox sender, StickyNotes.Windows.Models.Note note)
    {
        ViewModel.SelectedNote = note;
        ViewModel.SearchText = string.Empty;
        sender.Text = note.Title;
    }
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
        MarkdownPreview.Children.Clear();
        foreach (var element in MarkdownRenderer.Render(ViewModel.SelectedNote?.Content ?? string.Empty))
            MarkdownPreview.Children.Add(element);
    }

}
