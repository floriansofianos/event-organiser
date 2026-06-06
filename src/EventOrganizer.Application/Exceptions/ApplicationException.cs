namespace EventOrganizer.Application.Exceptions;

public class ApplicationException : Exception
{
    public int StatusCode { get; }

    public ApplicationException(string message, int statusCode = 400) : base(message)
    {
        StatusCode = statusCode;
    }
}

public class NotFoundException : ApplicationException
{
    public NotFoundException(string message) : base(message, 404) { }
}

public class ConflictException : ApplicationException
{
    public ConflictException(string message) : base(message, 409) { }
}

public class UnauthorizedException : ApplicationException
{
    public UnauthorizedException(string message) : base(message, 401) { }
}

public class ForbiddenException : ApplicationException
{
    public ForbiddenException(string message) : base(message, 403) { }
}
