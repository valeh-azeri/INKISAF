using OzunuInkisaf.Contracts.Duas;

namespace OzunuInkisaf.Application.Services;

public interface IDuaService
{
    Task<IReadOnlyList<DuaDto>> GetAllAsync(string? category, string? search, CancellationToken cancellationToken = default);

    Task<DuaDto> CreateAsync(UpsertDuaRequest request, Guid createdByUserId, CancellationToken cancellationToken = default);

    Task<DuaDto> UpdateAsync(Guid duaId, UpsertDuaRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid duaId, CancellationToken cancellationToken = default);

    /// <summary>Bu duaya PDF fayl əlavə edir (yenisi ilə əvəz edir, mətn sahələrinə toxunmur).</summary>
    Task<DuaDto> UploadPdfAsync(Guid duaId, Stream content, string originalFileName, CancellationToken cancellationToken = default);

    Task<(Stream Content, string FileName)> OpenPdfAsync(Guid duaId, CancellationToken cancellationToken = default);
}
