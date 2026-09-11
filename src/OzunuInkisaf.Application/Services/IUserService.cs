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

    /// <summary>
    /// İstifadəçini və ona aid şəxsi qeydləri (oxuma mövqeyi, çətələ, bal
    /// tarixçəsi) tamamilə silir. Xətimdə götürdüyü cüz(lər) isə silinmir —
    /// yenidən "boş" (götürülməmiş) vəziyyətinə qaytarılır ki, başqası
    /// götürə bilsin. Yalnız adi istifadəçilər üçündür (admin silinə bilməz).
    /// </summary>
    Task DeleteAsync(Guid userId, CancellationToken cancellationToken = default);
}
