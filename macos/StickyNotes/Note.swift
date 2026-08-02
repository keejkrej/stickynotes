import Foundation

struct Note: Identifiable, Codable, Hashable {
    var id = UUID()
    var title = "Untitled note"
    var content = ""
    var color = NoteColor.yellow
    var updatedAt = Date()
}

enum NoteColor: String, Codable, CaseIterable, Identifiable {
    case yellow, pink, blue, green, gray
    var id: Self { self }
    var label: String { rawValue.capitalized }
}
