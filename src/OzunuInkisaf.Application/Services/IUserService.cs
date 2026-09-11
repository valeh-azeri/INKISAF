using OzunuInkisaf.Contracts.Users;

namespace OzunuInkisaf.Application.Services;

public interface IUserService
{
    Task<IReadOnlyList<UserSummaryDto>> GetAllAsync(string? search, CancellationToken cancellationToken = default);

    Task<CreateUserResult> CreateAsync(CreateUserRequest request, Guid createdByUserId, CancellationToken cancellationToken = default);

    Task<BulkCreateUsersResult> BulkCreateAsync(int count, Guid createdByUserId, CancellationToken cancellationToken = default);

    Task<BulkCreateUsersResult> BulkCreateFromListAsync(BulkCreateUsersRequestFromList request, Guid createdByUserId, CancellationToken cancellationToken = default);

    Task SetActiveAsync(Guid userId, bool isActive, CancellationToken cancellationToken = default);

    Task<CreateUserResult> ResetPasswordAsync(Guid userId, CancellationToken cancellationToken = default);
}
