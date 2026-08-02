import SwiftUI

@main
struct StickyNotesApp: App {
    @StateObject private var store = NoteStore()

    var body: some Scene {
        WindowGroup {
            ContentView().environmentObject(store)
        }
        .defaultSize(width: 980, height: 650)
        .commands {
            CommandGroup(replacing: .newItem) {
                Button("New Note") { store.addNote() }
                    .keyboardShortcut("n", modifiers: .command)
            }
        }

        WindowGroup("Sticky Note", for: UUID.self) { $noteID in
            if let noteID, let note = store.binding(for: noteID) {
                StickyWidget(note: note)
                    .environmentObject(store)
            } else {
                Text("Note not found")
            }
        }
        .windowStyle(.hiddenTitleBar)
        .windowResizability(.contentSize)
        .defaultSize(width: 360, height: 420)
    }
}
