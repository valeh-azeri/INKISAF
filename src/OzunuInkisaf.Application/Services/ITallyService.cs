using OzunuInkisaf.Contracts.Tally;

namespace OzunuInkisaf.Application.Services;

public interface ITallyService
{
    Task<TallyTemplateDto> CreateAsync(CreateTallyTemplateRequest request, Guid createdByUserId, CancellationToken cancellationToken = default);

    Task<TallyTemplateDto> PublishAsync(Guid templateId, CancellationToken cancellationToken = default);

    Task<TallyTemplateDto?> GetActiveForUserAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<TallyTemplateDto> SetEntryAsync(Guid userId, SetTallyEntryRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TallyTemplateSummaryDto>> GetSummariesAsync(CancellationToken cancellationToken = default);

    Task<TallyTemplateDto> GetByIdForAdminAsync(Guid templateId, CancellationToken cancellationToken = default);
}
