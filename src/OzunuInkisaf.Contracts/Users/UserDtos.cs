using OzunuInkisaf.Contracts.Common;

namespace OzunuInkisaf.Contracts.Users;

public record UserSummaryDto(
    Guid Id,
    string FullName,
    string Username,
    string? Email,
    UserRoleDto Role,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? LastActiveAt,
    int TotalPoints);

public record CreateUserRequest(
    string FullName,
    string? Email,
    string? PhoneNumber,
    string? UsernameOverride,
    string? PasswordOverride,
    string? DeliveryEmail);

public record CreateUserResult(
    Guid UserId,
    string FullName,
    string Username,
    string GeneratedPassword);

public record BulkCreateUsersRequest(int Count);

public record BulkCreateUsersRowRequest(string FullName, string? Email, string? PhoneNumber);

public record BulkCreateUsersRequestFromList(IReadOnlyList<BulkCreateUsersRowRequest> Users);

public record BulkCreateUsersResult(IReadOnlyList<CreateUserResult> CreatedUsers);
