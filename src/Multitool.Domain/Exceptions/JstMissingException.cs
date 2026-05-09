namespace Multitool.Domain.Exceptions;

public class JwtMissingException(string message) : Exception(message) { }