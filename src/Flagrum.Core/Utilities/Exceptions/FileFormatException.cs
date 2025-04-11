using System;

namespace Flagrum.Core.Utilities.Exceptions;

public class FileFormatException : Exception
{
    public FileFormatException(string message) : base(message) { }
}