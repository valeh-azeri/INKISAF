using Microsoft.EntityFrameworkCore;
using OzunuInkisaf.Application.Common.Exceptions;
using OzunuInkisaf.Application.Common.Interfaces;
using OzunuInkisaf.Contracts.Common;
using OzunuInkisaf.Contracts.Tally;
using OzunuInkisaf.Domain.Entities;
using OzunuInkisaf.Domain.Enums;

namespace OzunuInkisaf.Application.Services;

public class TallyService : ITallyService
{
    private readonly IApplicationDbContext _db;

    public TallyService(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<TallyTemplateDto> CreateAsync(CreateTallyTemplateRequest request, Guid createdByUserId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new ValidationAppException("Başlıq tələb olunur.");
        }

        if (request.Items.Count == 0)
        {
            throw new ValidationAppException("Ən azı bir bənd əlavə edin.");
        }

        var template = new TallyTemplate
        {
            Title = request.Title.Trim(),
            WeekStartDate = request.WeekStartDate,
            WeekEndDate = request.WeekEndDate,
            DueDate = request.DueDate,
            Status = TallyStatus.Draft,
            CreatedByUserId = createdByUserId,
        };

        var order = 0;
        foreach (var item in request.Items)
        {
            template.Items.Add(new TallyItem
            {
                Title = item.Title.Trim(),
                Frequency = (TallyItemFrequency)item.Frequency,
                SpecificDaysCsv = item.SpecificDays.Count > 0 ? string.Join(',', item.SpecificDays) : null,
                PointsPerCompletion = item.PointsPerCompletion,
                DisplayOrder = order++,
            });
        }

        _db.TallyTemplates.Add(template);
        await _db.SaveChangesAsync(cancellationToken);

        return await BuildDtoAsync(template, null, cancellationToken);
    }

    public async Task<TallyTemplateDto> PublishAsync(Guid templateId, CancellationToken cancellationToken = default)
    {
        var template = await _db.TallyTemplates
            .Include(t => t.Items)
            .SingleOrDefaultAsync(t => t.Id == templateId, cancellationToken)
            ?? throw new NotFoundException(nameof(TallyTemplate), templateId);

        template.Status = TallyStatus.Published;
        template.PublishedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        return await BuildDtoAsync(template, null, cancellationToken);
    }

    public async Task<TallyTemplateDto?> GetActiveForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var template = await _db.TallyTemplates
            .Include(t => t.Items)
            .Where(t => t.Status == TallyStatus.Published)
            .OrderByDescending(t => t.WeekStartDate)
            .FirstOrDefaultAsync(cancellationToken);

        return template is null ? null : await BuildDtoAsync(template, userId, cancellationToken);
    }

    public async Task<TallyTemplateDto> SetEntryAsync(Guid userId, SetTallyEntryRequest request, CancellationToken cancellationToken = default)
    {
        var item = await _db.TallyItems
            .Include(i => i.TallyTemplate)
            .SingleOrDefaultAsync(i => i.Id == request.TallyItemId, cancellationToken)
            ?? throw new NotFoundException(nameof(TallyItem), request.TallyItemId);

        if (item.TallyTemplate!.Status != TallyStatus.Published)
        {
            throw new ValidationAppException("Bu çətələ artıq aktiv deyil.");
        }

        var entry = await _db.TallyEntries
            .SingleOrDefaultAsync(e => e.TallyItemId == request.TallyItemId && e.UserId == userId && e.Date == request.Date, cancellationToken);

        if (entry is null)
        {
            entry = new TallyEntry { TallyItemId = request.TallyItemId, UserId = userId, Date = request.Date };
            _db.TallyEntries.Add(entry);
        }

        var wasCompleted = entry.IsCompleted;
        entry.IsCompleted = request.IsCompleted;
        entry.CompletedAt = request.IsCompleted ? DateTime.UtcNow : null;

        await _db.SaveChangesAsync(cancellationToken);

        if (request.IsCompleted && !wasCompleted)
        {
            _db.PointsTransactions.Add(new PointsTransaction
            {
                UserId = userId,
                Source = PointsSource.TallyEntry,
                SourceId = entry.Id,
                Points = item.PointsPerCompletion,
            });
            await _db.SaveChangesAsync(cancellationToken);
        }
        else if (!request.IsCompleted && wasCompleted)
        {
            var existingTransaction = await _db.PointsTransactions
                .SingleOrDefaultAsync(p => p.Source == PointsSource.TallyEntry && p.SourceId == entry.Id, cancellationToken);
            if (existingTransaction is not null)
            {
                _db.PointsTransactions.Remove(existingTransaction);
                await _db.SaveChangesAsync(cancellationToken);
            }
        }

        var template = await _db.TallyTemplates
            .Include(t => t.Items)
            .SingleAsync(t => t.Id == item.TallyTemplateId, cancellationToken);

        return await BuildDtoAsync(template, userId, cancellationToken);
    }

    public async Task<IReadOnlyList<TallyTemplateSummaryDto>> GetSummariesAsync(CancellationToken cancellationToken = default)
    {
        var templates = await _db.TallyTemplates
            .Include(t => t.Items)
            .OrderByDescending(t => t.WeekStartDate)
            .ToListAsync(cancellationToken);

        var activeUserCount = Math.Max(1, await _db.Users.CountAsync(u => u.Role == UserRole.User && u.IsActive, cancellationToken));

        var result = new List<TallyTemplateSummaryDto>();
        foreach (var template in templates)
        {
            var itemIds = template.Items.Select(i => i.Id).ToList();
            var participantCount = await _db.TallyEntries
                .Where(e => itemIds.Contains(e.TallyItemId) && e.IsCompleted)
                .Select(e => e.UserId)
                .Distinct()
                .CountAsync(cancellationToken);

            result.Add(new TallyTemplateSummaryDto(
                template.Id,
                template.Title,
                template.WeekStartDate,
                template.WeekEndDate,
                (TallyStatusDto)template.Status,
                Math.Round(participantCount * 100.0 / activeUserCount, 1)));
        }

        return result;
    }

    public async Task<TallyTemplateDto> GetByIdForAdminAsync(Guid templateId, CancellationToken cancellationToken = default)
    {
        var template = await _db.TallyTemplates
            .Include(t => t.Items)
            .SingleOrDefaultAsync(t => t.Id == templateId, cancellationToken)
            ?? throw new NotFoundException(nameof(TallyTemplate), templateId);

        return await BuildDtoAsync(template, null, cancellationToken);
    }

    private async Task<TallyTemplateDto> BuildDtoAsync(TallyTemplate template, Guid? userId, CancellationToken cancellationToken)
    {
        var itemIds = template.Items.Select(i => i.Id).ToList();

        var entries = userId is null
            ? new List<TallyEntry>()
            : await _db.TallyEntries
                .Where(e => itemIds.Contains(e.TallyItemId) && e.UserId == userId)
                .ToListAsync(cancellationToken);

        var totalPoints = entries.Where(e => e.IsCompleted).Sum(e =>
            template.Items.Single(i => i.Id == e.TallyItemId).PointsPerCompletion);

        var itemDtos = template.Items.OrderBy(i => i.DisplayOrder).Select(item =>
        {
            var dates = BuildApplicableDates(template.WeekStartDate, template.WeekEndDate, item);
            var entriesForItem = entries.Where(e => e.TallyItemId == item.Id).ToDictionary(e => e.Date);

            var dayEntries = dates.Select(d => new TallyDayEntryDto(d, entriesForItem.TryGetValue(d, out var e) && e.IsCompleted)).ToList();

            var specificDays = string.IsNullOrEmpty(item.SpecificDaysCsv)
                ? Array.Empty<int>()
                : item.SpecificDaysCsv.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray();

            return new TallyItemWithEntriesDto(
                new TallyItemDto(item.Id, item.Title, (TallyItemFrequencyDto)item.Frequency, specificDays, item.PointsPerCompletion),
                dayEntries);
        }).ToList();

        return new TallyTemplateDto(
            template.Id,
            template.Title,
            template.WeekStartDate,
            template.WeekEndDate,
            template.DueDate,
            (TallyStatusDto)template.Status,
            totalPoints,
            itemDtos);
    }

    private static List<DateOnly> BuildApplicableDates(DateOnly weekStart, DateOnly weekEnd, TallyItem item)
    {
        var dates = new List<DateOnly>();
        var specificDays = string.IsNullOrEmpty(item.SpecificDaysCsv)
            ? null
            : item.SpecificDaysCsv.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToHashSet();

        for (var date = weekStart; date <= weekEnd; date = date.AddDays(1))
        {
            var isoDayOfWeek = (int)date.DayOfWeek == 0 ? 7 : (int)date.DayOfWeek; // 1=Monday..7=Sunday

            if (item.Frequency == TallyItemFrequency.Daily || (specificDays?.Contains(isoDayOfWeek) ?? false))
            {
                dates.Add(date);
            }
        }

        return dates;
    }
}
