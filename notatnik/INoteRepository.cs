using System;
using System.Collections.Generic;

namespace notatnik;

public interface INoteRepository
{
    string FilePath { get; }
    List<Note> GetAllNotes();
    void SaveAll(IEnumerable<Note> notes);
    Note SaveOrUpdate(Note note);
    bool DeleteNote(Guid id);
    void EnsureStorageExists();
}

