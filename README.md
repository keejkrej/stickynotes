# Sticky Notes

A small native sticky-notes app for Windows and macOS. Notes can live as compact, frameless, always-on-top desktop widgets. Both versions support local persistence, search, note colors, autosave, and Markdown editing with a native preview.

## Windows (WinUI 3)

Requirements: Windows 10 1809+, .NET 10 SDK, and the Windows App SDK prerequisites.

```powershell
cd windows/StickyNotes.Windows
dotnet build StickyNotes.Windows.csproj -p:Platform=x64
dotnet run --project StickyNotes.Windows.csproj -p:Platform=x64
```

Notes are stored in `%LOCALAPPDATA%\StickyNotes\notes.json`.

The Windows app itself is a draggable, resizable floating sticky. It uses the system Desktop Acrylic backdrop with a translucent note-color tint. Use the note picker in its drag bar to switch notes.

## macOS (SwiftUI)

Open `macos/StickyNotes.xcodeproj` in Xcode 26 or newer, select your development team if signing is requested, and Run. The deployment target is macOS 14. This project is intentionally not compiled on Windows.

Notes are stored in `~/Library/Application Support/StickyNotes/notes.json`.

The main window manages the collection. Click the window icon beside any note to open it as a separate floating sticky that follows you across Spaces. On macOS 26 it uses native Liquid Glass; macOS 14–15 receive an Ultra Thin Material fallback.

## Markdown

The editor supports headings, emphasis, links, lists, quotes, fenced/inline code, and plain text. SwiftUI uses Foundation's native Markdown-to-`AttributedString` support. WinUI renders Markdown into native `RichTextBlock` elements.
