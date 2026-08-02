using System.Text.Json;
using StickyNotes.Windows.Models;

namespace StickyNotes.Windows.Services;

public sealed class NoteStore
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private readonly string _path = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "StickyNotes", "notes.json");

    public async Task<IReadOnlyList<Note>> LoadAsync()
    {
        if (!File.Exists(_path)) return [];
        try
        {
            await using var stream = File.OpenRead(_path);
            return await JsonSerializer.DeserializeAsync<List<Note>>(stream, JsonOptions) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    public async Task SaveAsync(IEnumerable<Note> notes)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        var temporaryPath = _path + ".tmp";
        await using (var stream = File.Create(temporaryPath))
            await JsonSerializer.SerializeAsync(stream, notes, JsonOptions);
        File.Move(temporaryPath, _path, true);
    }
}
