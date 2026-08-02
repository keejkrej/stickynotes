using System.Collections.ObjectModel;
using StickyNotes.Windows.Models;
using StickyNotes.Windows.Services;

namespace StickyNotes.Windows.ViewModels;

public partial class MainViewModel : BaseViewModel
{
    private readonly NoteStore _store = new();
    private List<Note> _allNotes = [];

    public MainViewModel() : base("Sticky Notes") { }

    public ObservableCollection<Note> Notes { get; } = [];
    public IReadOnlyList<string> Colors { get; } = ["Yellow", "Pink", "Blue", "Green", "Gray"];

    [ObservableProperty] public partial Note? SelectedNote { get; set; }
    [ObservableProperty] public partial string SearchText { get; set; } = string.Empty;
    [ObservableProperty] public partial bool IsPreview { get; set; }
    [ObservableProperty] public partial string StatusText { get; set; } = "Ready";

    partial void OnSearchTextChanged(string value) => RefreshVisibleNotes();

    public async Task InitializeAsync()
    {
        _allNotes = (await _store.LoadAsync()).OrderByDescending(note => note.UpdatedAt).ToList();
        if (_allNotes.Count == 0)
        {
            _allNotes.Add(new Note
            {
                Title = "Welcome",
                Content = "# Welcome to Sticky Notes\n\nWrite with **Markdown**, then choose Preview.\n\n- Notes save automatically\n- Search by title or text\n- Pick a color for every idea"
            });
            await SaveAsync();
        }
        RefreshVisibleNotes();
        SelectedNote = Notes.FirstOrDefault();
    }

    [RelayCommand]
    private async Task AddNoteAsync()
    {
        var note = new Note();
        _allNotes.Insert(0, note);
        SearchText = string.Empty;
        RefreshVisibleNotes();
        SelectedNote = note;
        IsPreview = false;
        await SaveAsync();
    }

    [RelayCommand(CanExecute = nameof(CanDelete))]
    private async Task DeleteNoteAsync()
    {
        if (SelectedNote is null) return;
        _allNotes.Remove(SelectedNote);
        RefreshVisibleNotes();
        SelectedNote = Notes.FirstOrDefault();
        await SaveAsync();
    }

    private bool CanDelete() => SelectedNote is not null;
    partial void OnSelectedNoteChanged(Note? value) => DeleteNoteCommand.NotifyCanExecuteChanged();

    public async Task NoteChangedAsync()
    {
        if (SelectedNote is null) return;
        SelectedNote.UpdatedAt = DateTimeOffset.Now;
        NotesListReorder();
        await SaveAsync();
    }

    private void NotesListReorder()
    {
        _allNotes = _allNotes.OrderByDescending(note => note.UpdatedAt).ToList();
    }

    private void RefreshVisibleNotes()
    {
        var query = SearchText.Trim();
        var selected = SelectedNote;
        Notes.Clear();
        foreach (var note in _allNotes.Where(note => query.Length == 0 ||
                     note.Title.Contains(query, StringComparison.CurrentCultureIgnoreCase) ||
                     note.Content.Contains(query, StringComparison.CurrentCultureIgnoreCase)))
            Notes.Add(note);
        if (selected is not null && Notes.Contains(selected)) SelectedNote = selected;
    }

    private async Task SaveAsync()
    {
        StatusText = "Saving…";
        await _store.SaveAsync(_allNotes);
        StatusText = $"Saved {DateTime.Now:t}";
    }
}
