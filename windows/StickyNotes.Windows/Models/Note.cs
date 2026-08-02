namespace StickyNotes.Windows.Models;

public sealed class Note
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = "Untitled note";
    public string Content { get; set; } = string.Empty;
    public string Color { get; set; } = "Yellow";
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.Now;

    public override string ToString() => Title;
}
