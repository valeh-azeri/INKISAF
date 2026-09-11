using Microsoft.EntityFrameworkCore;
using OzunuInkisaf.Application.Common.Exceptions;
using OzunuInkisaf.Application.Common.Interfaces;
using OzunuInkisaf.Contracts.Common;
using OzunuInkisaf.Contracts.Duas;
using OzunuInkisaf.Domain.Entities;
using OzunuInkisaf.Domain.Enums;

namespace OzunuInkisaf.Application.Services;

public class DuaService : IDuaService
{
    private readonly IApplicationDbContext _db;

    public DuaService(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<DuaDto>> GetAllAsync(string? category, string? search, CancellationToken cancellationToken = default)
    {
        var query = _db.Duas.Where(d => d.IsActive).AsQueryable();

        if (!string.IsNullOrWhiteSpace(category) && Enum.TryParse<DuaCategory>(category, true, out var parsedCategory))
        {
            query = query.Where(d => d.Category == parsedCategory);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLowerInvariant();
            query = query.Where(d => d.Title.ToLower().Contains(term));
        }

        var duas = await query.OrderBy(d => d.DisplayOrder).ThenBy(d => d.Title).ToListAsync(cancellationToken);

        return duas.Select(ToDto).ToList();
    }

    public async Task<DuaDto> CreateAsync(UpsertDuaRequest request, Guid createdByUserId, CancellationToken cancellationToken = default)
    {
        Validate(request);

        var dua = new Dua
        {
            Title = request.Title.Trim(),
            ArabicText = request.ArabicText.Trim(),
            Translation = request.Translation.Trim(),
            Transliteration = request.Transliteration,
            Category = (DuaCategory)request.Category,
            CreatedByUserId = createdByUserId,
            IsActive = true,
        };

        _db.Duas.Add(dua);
        await _db.SaveChangesAsync(cancellationToken);

        return ToDto(dua);
    }

    public async Task<DuaDto> UpdateAsync(Guid duaId, UpsertDuaRequest request, CancellationToken cancellationToken = default)
    {
        Validate(request);

        var dua = await _db.Duas.SingleOrDefaultAsync(d => d.Id == duaId, cancellationToken)
            ?? throw new NotFoundException(nameof(Dua), duaId);

        dua.Title = request.Title.Trim();
        dua.ArabicText = request.ArabicText.Trim();
        dua.Translation = request.Translation.Trim();
        dua.Transliteration = request.Transliteration;
        dua.Category = (DuaCategory)request.Category;

        await _db.SaveChangesAsync(cancellationToken);

        return ToDto(dua);
    }

    public async Task DeleteAsync(Guid duaId, CancellationToken cancellationToken = default)
    {
        var dua = await _db.Duas.SingleOrDefaultAsync(d => d.Id == duaId, cancellationToken)
            ?? throw new NotFoundException(nameof(Dua), duaId);

        dua.IsActive = false;
        await _db.SaveChangesAsync(cancellationToken);
    }

    private static void Validate(UpsertDuaRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new ValidationAppException("Duanın adı tələb olunur.");
        }

        if (string.IsNullOrWhiteSpace(request.Translation))
        {
            throw new ValidationAppException("Tərcümə mətni tələb olunur.");
        }
    }

    private static DuaDto ToDto(Dua d) => new(
        d.Id, d.Title, d.ArabicText, d.Translation, d.Transliteration, (DuaCategoryDto)d.Category, d.IsActive);
}
