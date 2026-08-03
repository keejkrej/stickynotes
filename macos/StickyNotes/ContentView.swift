import SwiftUI
import AppKit

struct ContentView: View {
    @EnvironmentObject private var store: NoteStore
    @Environment(\.dismissWindow) private var dismissWindow
    @State private var preview = false
    @State private var pickerText = ""
    @FocusState private var pickerFocused: Bool

    var body: some View {
        VStack(spacing: 10) {
            header
            notePicker

            if let id = store.selection, let note = store.binding(for: id) {
                TextField("Note title", text: note.title)
                    .textFieldStyle(.roundedBorder)

                Group {
                    if preview {
                        ScrollView {
                            Text(markdown(note.wrappedValue.content))
                                .frame(maxWidth: .infinity, alignment: .topLeading)
                                .textSelection(.enabled)
                        }
                    } else {
                        TextEditor(text: note.content)
                            .font(.system(size: 12))
                            .scrollContentBackground(.hidden)
                    }
                }
                .frame(maxWidth: .infinity, maxHeight: .infinity)
            } else {
                ContentUnavailableView("No Note Selected", systemImage: "note.text")
                    .frame(maxWidth: .infinity, maxHeight: .infinity)
            }

            Text(store.statusText)
                .font(.system(size: 11))
                .foregroundStyle(.secondary)
                .frame(maxWidth: .infinity, alignment: .trailing)
        }
        .padding(.init(top: 10, leading: 16, bottom: 14, trailing: 16))
        .background(.ultraThinMaterial)
        .background(FloatingWindowConfigurator())
        .onAppear { syncPickerText() }
        .onChange(of: store.selection) { syncPickerText() }
    }

    private var header: some View {
        HStack(spacing: 9) {
            Image(systemName: "note.text")
            Text("STICKY NOTES")
                .font(.system(size: 12, weight: .semibold))
                .tracking(1)
            Spacer()
            Button {
                store.addNote()
                preview = false
            } label: {
                Image(systemName: "plus")
                    .frame(width: 24, height: 24)
            }
            .help("New note")
            Button { dismissWindow() } label: {
                Image(systemName: "xmark")
                    .frame(width: 24, height: 24)
            }
            .help("Close")
        }
    }

    private var notePicker: some View {
        HStack(spacing: 8) {
            HStack(spacing: 6) {
                Image(systemName: "magnifyingglass")
                    .foregroundStyle(.secondary)
                TextField("Find or switch notes", text: $pickerText)
                    .textFieldStyle(.plain)
                    .focused($pickerFocused)
                    .onChange(of: pickerText) {
                        if pickerFocused { store.searchText = pickerText }
                    }
                    .onSubmit { selectFirstMatch() }
                Menu {
                    ForEach(store.filteredNotes) { note in
                        Button(note.title) { select(note.id) }
                    }
                } label: {
                    Image(systemName: "chevron.down")
                        .frame(width: 20, height: 20)
                }
                .menuStyle(.borderlessButton)
                .fixedSize()
            }
            .padding(.horizontal, 8)
            .frame(height: 34)
            .background(.quaternary, in: RoundedRectangle(cornerRadius: 6))

            Group {
                if preview {
                    Button { preview.toggle() } label: {
                        Image(systemName: "eye")
                            .frame(width: 24, height: 24)
                    }
                    .buttonStyle(.borderedProminent)
                } else {
                    Button { preview.toggle() } label: {
                        Image(systemName: "eye")
                            .frame(width: 24, height: 24)
                    }
                    .buttonStyle(.bordered)
                }
            }
            .help("Markdown preview")

            Button(role: .destructive) {
                store.deleteSelection()
            } label: {
                Image(systemName: "trash")
                    .frame(width: 24, height: 24)
            }
            .help("Delete note")
            .disabled(store.selection == nil)
        }
    }

    private func selectFirstMatch() {
        guard let note = store.filteredNotes.first else { return }
        select(note.id)
    }

    private func select(_ id: Note.ID) {
        store.selection = id
        store.searchText = ""
        pickerFocused = false
        syncPickerText()
    }

    private func syncPickerText() {
        pickerText = store.selectedNote?.title ?? ""
    }

    private func markdown(_ content: String) -> AttributedString {
        (try? AttributedString(markdown: content,
            options: .init(interpretedSyntax: .full, failurePolicy: .returnPartiallyParsedIfPossible))) ?? AttributedString(content)
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
        window.titlebarAppearsTransparent = true
        window.standardWindowButton(.closeButton)?.isHidden = true
        window.standardWindowButton(.miniaturizeButton)?.isHidden = true
        window.standardWindowButton(.zoomButton)?.isHidden = true
    }
}
