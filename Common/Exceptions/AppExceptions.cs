namespace Common.Exceptions;

/// <summary>Base application exception that maps to an HTTP status code.</summary>
public class AppException : Exception
{
    public int StatusCode { get; }

    public AppException(int statusCode, string message)
        : base(message)
    {
        StatusCode = statusCode;
    }
}

/// <summary>400 Bad Request</summary>
public class BadRequestException : AppException
{
    public BadRequestException(string message = "Bad request.")
        : base(400, message) { }
}

/// <summary>401 Unauthorized</summary>
public class UnauthorizedException : AppException
{
    public UnauthorizedException(string message = "Unauthorized.")
        : base(401, message) { }
}

/// <summary>403 Forbidden</summary>
public class ForbiddenException : AppException
{
    public ForbiddenException(string message = "Access forbidden.")
        : base(403, message) { }
}

/// <summary>404 Not Found</summary>
public class NotFoundException : AppException
{
    public NotFoundException(string message = "Resource not found.")
        : base(404, message) { }
}

/// <summary>409 Conflict</summary>
public class ConflictException : AppException
{
    public ConflictException(string message = "Resource already exists.")
        : base(409, message) { }
}

/// <summary>500 Internal Server Error</summary>
public class InternalServerException : AppException
{
    public InternalServerException(string message = "An unexpected error occurred.")
        : base(500, message) { }
}
