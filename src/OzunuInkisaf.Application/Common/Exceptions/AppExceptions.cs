namespace OzunuInkisaf.Application.Common.Exceptions;

/// <summary>Requested entity does not exist. WebApi maps this to HTTP 404.</summary>
public class NotFoundException : Exception
{
    public NotFoundException(string entityName, object key)
        : base($"\"{entityName}\" ({key}) tapılmadı.")
    {
    }

    public NotFoundException(string message) : base(message)
    {
    }
}

/// <summary>Input failed a business rule. WebApi maps this to HTTP 400.</summary>
public class ValidationAppException : Exception
{
    public ValidationAppException(string message) : base(message)
    {
    }
}

/// <summary>Caller is authenticated but not allowed to perform this action. WebApi maps this to HTTP 403.</summary>
public class ForbiddenAccessException : Exception
{
    public ForbiddenAccessException(string message = "Bu əməliyyat üçün icazəniz yoxdur.") : base(message)
    {
    }
}

/// <summary>Username already taken, juz' already claimed, etc. WebApi maps this to HTTP 409.</summary>
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message)
    {
    }
}

/// <summary>Username/password did not match. WebApi maps this to HTTP 401.</summary>
public class AuthenticationFailedException : Exception
{
    public AuthenticationFailedException(string message = "İstifadəçi adı və ya şifrə yanlışdır.") : base(message)
    {
    }
}
