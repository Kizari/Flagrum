using System;

namespace Flagrum.Core.Utilities.Exceptions;

public class MissingFixidException : Exception
{
    public MissingFixidException(string message) : base(message)
    {
    }
}