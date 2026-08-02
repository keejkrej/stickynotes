import SwiftUI

@MainActor
final class NoteStore: ObservableObject {
    @Published private(set) var notes: [Note] = []
    @Published var selection: Note.ID?
    @Published var searchText = ""

    private let fileURL: URL
    private var saveTask: Task<Void, Never>?

    init() {
        let directory = FileManager.default.urls(for: .applicationSupportDirectory, in: .userDomainMask)[0]
            .appendingPathComponent("StickyNotes", isDirectory: true)
        try? FileManager.default.createDirectory(at: directory, withIntermediateDirectories: true)
        fileURL = directory.appendingPathComponent("notes.json")
        load()
    }

    var filteredNotes: [Note] {
        guard !searchText.isEmpty else { return notes }
        return notes.filter { $0.title.localizedCaseInsensitiveContains(searchText) ||
            $0.content.localizedCaseInsensitiveContains(searchText) }
    }

    func binding(for id: Note.ID) -> Binding<Note>? {
        guard let index = notes.firstIndex(where: { $0.id == id }) else { return nil }
        return Binding(get: { self.notes[index] }, set: { value in
            var changed = value
            changed.updatedAt = Date()
            self.notes[index] = changed
            self.scheduleSave()
        })
    }

    func addNote() {
        let note = Note()
        notes.insert(note, at: 0)
        selection = note.id
        scheduleSave()
    }

    func deleteSelection() {
        guard let selection else { return }
        notes.removeAll { $0.id == selection }
        self.selection = notes.first?.id
        scheduleSave()
    }

    private func load() {
        if let data = try? Data(contentsOf: fileURL),
           let decoded = try? JSONDecoder().decode([Note].self, from: data) {
            notes = decoded.sorted { $0.updatedAt > $1.updatedAt }
        }
        if notes.isEmpty {
            notes = [Note(title: "Welcome", content: "# Welcome to Sticky Notes\n\nWrite with **Markdown**, then choose Preview.\n\n- Notes save automatically\n- Search by title or text\n- Pick a color for every idea")]
            save()
        }
        selection = notes.first?.id
    }

    private func scheduleSave() {
        saveTask?.cancel()
        saveTask = Task {
            try? await Task.sleep(for: .milliseconds(400))
            guard !Task.isCancelled else { return }
            save()
        }
    }

    private func save() {
        guard let data = try? JSONEncoder().encode(notes) else { return }
        try? data.write(to: fileURL, options: .atomic)
    }
}
