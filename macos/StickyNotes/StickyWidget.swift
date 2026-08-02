import SwiftUI
import AppKit

struct StickyWidget: View {
    @Binding var note: Note
    @Environment(\.dismissWindow) private var dismissWindow
    @State private var preview = false

    var body: some View {
        VStack(spacing: 10) {
            HStack {
                Image(systemName: "note.text").foregroundStyle(.secondary)
                TextField("Title", text: $note.title).font(.headline).textFieldStyle(.plain)
                Button { preview.toggle() } label: { Image(systemName: preview ? "pencil" : "eye") }
                    .buttonStyle(.plain).help("Toggle Markdown preview")
                Button { dismissWindow() } label: { Image(systemName: "xmark") }
                    .buttonStyle(.plain).help("Close sticky")
            }
            Divider()
            if preview {
                ScrollView {
                    Text(markdown).frame(maxWidth: .infinity, alignment: .topLeading).textSelection(.enabled)
                }
            } else {
                TextEditor(text: $note.content)
                    .font(.system(.body, design: .monospaced))
                    .scrollContentBackground(.hidden)
            }
        }
        .padding(14)
        .frame(minWidth: 280, minHeight: 300)
        .modifier(StickyGlassMaterial(tint: tint))
        .background(FloatingWindowConfigurator())
    }

    private var tint: Color {
        switch note.color {
        case .yellow: Color(red: 1, green: 0.85, blue: 0.4)
        case .pink: Color(red: 1, green: 0.72, blue: 0.8)
        case .blue: Color(red: 0.68, green: 0.85, blue: 1)
        case .green: Color(red: 0.72, green: 0.9, blue: 0.72)
        case .gray: Color(red: 0.84, green: 0.84, blue: 0.84)
        }
    }

    private var markdown: AttributedString {
        (try? AttributedString(markdown: note.content,
            options: .init(interpretedSyntax: .full, failurePolicy: .returnPartiallyParsedIfPossible))) ?? AttributedString(note.content)
    }
}

private struct StickyGlassMaterial: ViewModifier {
    let tint: Color

    @ViewBuilder
    func body(content: Content) -> some View {
        if #available(macOS 26.0, *) {
            content
                .glassEffect(.regular.tint(tint.opacity(0.55)), in: RoundedRectangle(cornerRadius: 18, style: .continuous))
        } else {
            content
                .background(.ultraThinMaterial, in: RoundedRectangle(cornerRadius: 18, style: .continuous))
                .background(tint.opacity(0.32), in: RoundedRectangle(cornerRadius: 18, style: .continuous))
        }
    }
}

private struct FloatingWindowConfigurator: NSViewRepresentable {
    func makeNSView(context: Context) -> NSView {
        let view = NSView()
        DispatchQueue.main.async { configure(view.window) }
        return view
    }

    func updateNSView(_ view: NSView, context: Context) {
        DispatchQueue.main.async { configure(view.window) }
    }

    private func configure(_ window: NSWindow?) {
        guard let window else { return }
        window.level = .floating
        window.isMovableByWindowBackground = true
        window.collectionBehavior.insert(.canJoinAllSpaces)
        window.titlebarAppearsTransparent = true
        window.standardWindowButton(.closeButton)?.isHidden = true
        window.standardWindowButton(.miniaturizeButton)?.isHidden = true
        window.standardWindowButton(.zoomButton)?.isHidden = true
    }
}
