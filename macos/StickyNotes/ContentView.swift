import SwiftUI

struct ContentView: View {
    @EnvironmentObject private var store: NoteStore
    @Environment(\.openWindow) private var openWindow
    @State private var preview = false

    var body: some View {
        NavigationSplitView {
            List(store.filteredNotes, selection: $store.selection) { note in
                VStack(alignment: .leading, spacing: 4) {
                    HStack {
                        Text(note.title).font(.headline).lineLimit(1)
                        Spacer()
                        Button { openWindow(value: note.id) } label: { Image(systemName: "macwindow.on.rectangle") }
                            .buttonStyle(.borderless).help("Open as desktop sticky")
                    }
                    Text(note.content).foregroundStyle(.secondary).lineLimit(2)
                    Text(note.updatedAt, style: .relative).font(.caption).foregroundStyle(.tertiary)
                }
                .padding(.vertical, 4)
                .tag(note.id)
            }
            .searchable(text: $store.searchText, prompt: "Search notes")
            .navigationTitle("Sticky Notes")
            .toolbar { Button(action: store.addNote) { Label("New note", systemImage: "plus") } }
        } detail: {
            if let id = store.selection, let note = store.binding(for: id) {
                NoteEditor(note: note, preview: $preview, delete: store.deleteSelection)
                    .id(id)
            } else {
                ContentUnavailableView("No Note Selected", systemImage: "note.text", description: Text("Create or select a note."))
            }
        }
    }
}

private struct NoteEditor: View {
    @Binding var note: Note
    @Binding var preview: Bool
    let delete: () -> Void

    var tint: Color {
        switch note.color { case .yellow: .yellow; case .pink: .pink; case .blue: .blue; case .green: .green; case .gray: .gray }
    }

    var body: some View {
        VStack(spacing: 14) {
            HStack {
                TextField("Title", text: $note.title).font(.title2.bold()).textFieldStyle(.plain)
                Picker("Color", selection: $note.color) {
                    ForEach(NoteColor.allCases) { color in Text(color.label).tag(color) }
                }.frame(width: 130)
                Toggle("Preview", isOn: $preview).toggleStyle(.switch)
                Button(role: .destructive, action: delete) { Image(systemName: "trash") }
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
        .padding(20)
        .background(tint.opacity(0.12))
        .navigationTitle(note.title)
    }

    private var markdown: AttributedString {
        (try? AttributedString(markdown: note.content,
            options: .init(interpretedSyntax: .full, failurePolicy: .returnPartiallyParsedIfPossible))) ?? AttributedString(note.content)
    }
}
