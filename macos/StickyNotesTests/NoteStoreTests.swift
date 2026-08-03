import XCTest
@testable import StickyNotes

@MainActor
final class NoteStoreTests: XCTestCase {
    private var directory: URL!
    private var fileURL: URL { directory.appendingPathComponent("notes.json") }

    override func setUpWithError() throws {
        directory = FileManager.default.temporaryDirectory
            .appendingPathComponent(UUID().uuidString, isDirectory: true)
        try FileManager.default.createDirectory(at: directory, withIntermediateDirectories: true)
    }

    override func tearDownWithError() throws {
        try FileManager.default.removeItem(at: directory)
    }

    func testFirstLaunchCreatesWelcomeNoteAndPersistsIt() throws {
        let store = NoteStore(fileURL: fileURL)

        XCTAssertEqual(store.notes.count, 1)
        XCTAssertEqual(store.notes.first?.title, "Welcome")
        XCTAssertEqual(store.selection, store.notes.first?.id)
        XCTAssertTrue(FileManager.default.fileExists(atPath: fileURL.path))
        XCTAssertEqual(try decodedNotes().count, 1)
    }

    func testLoadSortsNotesNewestFirst() throws {
        let older = Note(title: "Older", updatedAt: Date(timeIntervalSince1970: 10))
        let newer = Note(title: "Newer", updatedAt: Date(timeIntervalSince1970: 20))
        try JSONEncoder().encode([older, newer]).write(to: fileURL)

        let store = NoteStore(fileURL: fileURL)

        XCTAssertEqual(store.notes.map(\.title), ["Newer", "Older"])
        XCTAssertEqual(store.selection, newer.id)
    }

    func testSearchMatchesTitleAndContentIgnoringCase() throws {
        let first = Note(title: "Shopping", content: "Milk")
        let second = Note(title: "Work", content: "SHIP release")
        try JSONEncoder().encode([first, second]).write(to: fileURL)
        let store = NoteStore(fileURL: fileURL)

        store.searchText = "shop"
        XCTAssertEqual(store.filteredNotes.map(\.id), [first.id])
        store.searchText = "ship"
        XCTAssertEqual(store.filteredNotes.map(\.id), [second.id])
        store.searchText = ""
        XCTAssertEqual(store.filteredNotes.count, 2)
    }

    func testSearchIgnoresSurroundingWhitespace() throws {
        let note = Note(title: "Shopping", content: "Milk")
        try JSONEncoder().encode([note]).write(to: fileURL)
        let store = NoteStore(fileURL: fileURL)

        store.searchText = "  SHOP  "

        XCTAssertEqual(store.filteredNotes.map(\.id), [note.id])
    }

    func testAddAndDeleteUpdateSelectionAndPersistence() throws {
        let store = NoteStore(fileURL: fileURL)
        let originalID = try XCTUnwrap(store.selection)

        store.addNote()
        let addedID = try XCTUnwrap(store.selection)
        XCTAssertNotEqual(addedID, originalID)
        XCTAssertEqual(store.notes.first?.id, addedID)

        store.deleteSelection()
        XCTAssertEqual(store.selection, originalID)
        XCTAssertFalse(store.notes.contains { $0.id == addedID })
        store.flushPendingSave()
        XCTAssertEqual(try decodedNotes().map(\.id), [originalID])
    }

    func testBindingContinuesToEditSameNoteAfterInsertion() throws {
        let store = NoteStore(fileURL: fileURL)
        let originalID = try XCTUnwrap(store.selection)
        let binding = try XCTUnwrap(store.binding(for: originalID))
        let previousDate = binding.wrappedValue.updatedAt

        store.addNote()
        var edited = binding.wrappedValue
        edited.title = "Edited original"
        binding.wrappedValue = edited

        XCTAssertEqual(store.notes.first(where: { $0.id == originalID })?.title, "Edited original")
        XCTAssertNotEqual(store.notes.first?.title, "Edited original")
        XCTAssertGreaterThan(store.notes.first(where: { $0.id == originalID })!.updatedAt, previousDate)
        XCTAssertEqual(store.statusText, "Saving…")
        XCTAssertEqual(store.selectedNote?.id, store.selection)
    }

    private func decodedNotes() throws -> [Note] {
        try JSONDecoder().decode([Note].self, from: Data(contentsOf: fileURL))
    }
}
