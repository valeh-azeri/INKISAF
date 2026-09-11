using OzunuInkisaf.Domain.Entities;

namespace OzunuInkisaf.Application.Common.Interfaces;

public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAtUtc) GenerateToken(User user);
}
