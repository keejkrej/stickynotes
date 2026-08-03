import SwiftUI

@main
struct StickyNotesApp: App {
    @StateObject private var store = NoteStore()

    var body: some Scene {
        WindowGroup {
            ContentView().environmentObject(store)
        }
        .windowStyle(.hiddenTitleBar)
        .defaultSize(width: 440, height: 580)
        .commands {
            CommandGroup(replacing: .newItem) {
                Button("New Note") { store.addNote() }
                    .keyboardShortcut("n", modifiers: .command)
            }
        }
    }
}
