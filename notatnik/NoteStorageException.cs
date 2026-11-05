using System;

namespace notatnik;

public class NoteStorageException : Exception
{
    public NoteStorageException(string message) : base(message)
    {
    }

    public NoteStorageException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

