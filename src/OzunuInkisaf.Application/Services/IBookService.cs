using OzunuInkisaf.Contracts.Books;

namespace OzunuInkisaf.Application.Services;

public interface IBookService
{
    Task<IReadOnlyList<BookSummaryDto>> GetAllAsync(Guid currentUserId, string? category, CancellationToken cancellationToken = default);

    Task<BookSummaryDto> UploadAsync(Stream content, string originalFileName, UploadBookRequest meta, Guid uploadedByUserId, CancellationToken cancellationToken = default);

    Task<(Stream Content, string FileName)> OpenPdfAsync(Guid bookId, CancellationToken cancellationToken = default);

    Task<ReadingProgressDto> SaveProgressAsync(Guid bookId, Guid userId, SaveReadingProgressRequest request, CancellationToken cancellationToken = default);

    Task ArchiveAsync(Guid bookId, CancellationToken cancellationToken = default);
}
