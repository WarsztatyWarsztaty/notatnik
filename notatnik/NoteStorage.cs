namespace notatnik;

public class NoteStorage : JsonNoteRepository
{
    public NoteStorage(string? filePath = null) : base(filePath)
    {
    }
}
