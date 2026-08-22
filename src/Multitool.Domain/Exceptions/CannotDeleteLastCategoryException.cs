namespace Multitool.Domain.Exceptions;

public class CannotDeleteLastCategoryException(string message) : Exception(message) { }