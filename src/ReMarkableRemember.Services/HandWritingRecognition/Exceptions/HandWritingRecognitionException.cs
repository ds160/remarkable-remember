using System;

namespace ReMarkableRemember.Services.HandWritingRecognition.Exceptions;

public class HandWritingRecognitionException : Exception
{
    public HandWritingRecognitionException() { }

    public HandWritingRecognitionException(String message) : base(message) { }

    public HandWritingRecognitionException(String message, Exception innerException) : base(message, innerException) { }
}
