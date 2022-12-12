using System;

namespace Flagrum.Core.Utilities.Exceptions;

public class FormatVersionException : Exception
{
    public FormatVersionException(string message) : base(message) { }
}