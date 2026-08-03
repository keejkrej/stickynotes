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

The Windows app itself is a draggable, resizable floating sticky. It inherits the system light/dark theme and uses the system Desktop Acrylic backdrop without a custom color overlay. Use the note picker below its drag bar to switch notes.

## macOS (SwiftUI)

Open `macos/StickyNotes.xcodeproj` in Xcode 26 or newer, select your development team if signing is requested, and Run. The deployment target is macOS 14. This project is intentionally not compiled on Windows.

Run the macOS unit tests from Terminal with:

```sh
xcodebuild -project macos/StickyNotes.xcodeproj -scheme StickyNotes -destination 'platform=macOS' CODE_SIGNING_ALLOWED=NO test
```

Notes are stored in `~/Library/Application Support/StickyNotes/notes.json`.

Like the Windows version, the macOS app is a single compact, resizable, always-on-top sticky. Use the note picker to find or switch notes without leaving the floating window.

## Markdown

The editor supports headings, emphasis, links, lists, quotes, fenced/inline code, and plain text. SwiftUI uses Foundation's native Markdown-to-`AttributedString` support. WinUI renders Markdown into native `RichTextBlock` elements.
