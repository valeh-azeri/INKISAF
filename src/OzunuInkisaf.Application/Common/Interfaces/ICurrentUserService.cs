using OzunuInkisaf.Domain.Enums;

namespace OzunuInkisaf.Application.Common.Interfaces;

/// <summary>
/// Reads the authenticated caller's identity out of the current HTTP
/// request (populated by the WebApi from the validated JWT). Implemented in
/// the WebApi project itself since it needs IHttpContextAccessor.
/// </summary>
public interface ICurrentUserService
{
    Guid? UserId { get; }

    UserRole? Role { get; }

    bool IsAuthenticated { get; }

    bool IsAdmin { get; }
}
