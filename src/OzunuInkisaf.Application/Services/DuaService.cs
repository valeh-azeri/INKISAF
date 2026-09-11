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
    private readonly IFileStorageService _fileStorage;

    public DuaService(IApplicationDbContext db, IFileStorageService fileStorage)
    {
        _db = db;
        _fileStorage = fileStorage;
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
        ValidateTitle(request.Title);

        var dua = new Dua
        {
            Title = request.Title.Trim(),
            ArabicText = string.IsNullOrWhiteSpace(request.ArabicText) ? null : request.ArabicText.Trim(),
            Translation = string.IsNullOrWhiteSpace(request.Translation) ? null : request.Translation.Trim(),
            Transliteration = string.IsNullOrWhiteSpace(request.Transliteration) ? null : request.Transliteration.Trim(),
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
        ValidateTitle(request.Title);

        var dua = await _db.Duas.SingleOrDefaultAsync(d => d.Id == duaId, cancellationToken)
            ?? throw new NotFoundException(nameof(Dua), duaId);

        dua.Title = request.Title.Trim();
        dua.ArabicText = string.IsNullOrWhiteSpace(request.ArabicText) ? null : request.ArabicText.Trim();
        dua.Translation = string.IsNullOrWhiteSpace(request.Translation) ? null : request.Translation.Trim();
        dua.Transliteration = string.IsNullOrWhiteSpace(request.Transliteration) ? null : request.Transliteration.Trim();
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

    public async Task<DuaDto> UploadPdfAsync(Guid duaId, Stream content, string originalFileName, CancellationToken cancellationToken = default)
    {
        var dua = await _db.Duas.SingleOrDefaultAsync(d => d.Id == duaId, cancellationToken)
            ?? throw new NotFoundException(nameof(Dua), duaId);

        var (relativePath, sizeBytes) = await _fileStorage.SavePdfAsync(content, originalFileName, cancellationToken);

        dua.PdfPath = relativePath;
        dua.PdfFileSizeBytes = sizeBytes;

        await _db.SaveChangesAsync(cancellationToken);

        return ToDto(dua);
    }

    public async Task<(Stream Content, string FileName)> OpenPdfAsync(Guid duaId, CancellationToken cancellationToken = default)
    {
        var dua = await _db.Duas.SingleOrDefaultAsync(d => d.Id == duaId, cancellationToken)
            ?? throw new NotFoundException(nameof(Dua), duaId);

        if (string.IsNullOrEmpty(dua.PdfPath))
        {
            throw new NotFoundException(nameof(Dua), duaId);
        }

        var stream = await _fileStorage.OpenReadAsync(dua.PdfPath, cancellationToken);
        return (stream, $"{dua.Title}.pdf");
    }

    private static void ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ValidationAppException("Duanın adı tələb olunur.");
        }
    }

    private static DuaDto ToDto(Dua d) => new(
        d.Id, d.Title, d.ArabicText, d.Translation, d.Transliteration, (DuaCategoryDto)d.Category, d.IsActive, !string.IsNullOrEmpty(d.PdfPath));
}
