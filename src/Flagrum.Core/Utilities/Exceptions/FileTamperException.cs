using System;

namespace Flagrum.Core.Utilities.Exceptions;

public class FileTamperException : Exception
{
    public FileTamperException(string message) : base(message) { }
}