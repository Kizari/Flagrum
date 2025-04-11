using System;

namespace Flagrum.Core.Utilities.Exceptions;

public class EarlyAccessException : Exception
{
    public EarlyAccessException(string message) : base(message) { }
}