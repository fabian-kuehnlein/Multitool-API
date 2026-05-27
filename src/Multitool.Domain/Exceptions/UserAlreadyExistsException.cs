namespace Multitool.Domain.Exceptions;

public class UserAlreadyExistsException(string message) : Exception(message) { }