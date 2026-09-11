using Microsoft.EntityFrameworkCore;
using OzunuInkisaf.Application.Common.Exceptions;
using OzunuInkisaf.Application.Common.Interfaces;
using OzunuInkisaf.Contracts.Books;
using OzunuInkisaf.Contracts.Common;
using OzunuInkisaf.Domain.Entities;
using OzunuInkisaf.Domain.Enums;

namespace OzunuInkisaf.Application.Services;

public class BookService : IBookService
{
    private readonly IApplicationDbContext _db;
    private readonly IFileStorageService _fileStorage;

    public BookService(IApplicationDbContext db, IFileStorageService fileStorage)
    {
        _db = db;
        _fileStorage = fileStorage;
    }

    public async Task<IReadOnlyList<BookSummaryDto>> GetAllAsync(Guid currentUserId, string? category, CancellationToken cancellationToken = default)
    {
        var query = _db.Books.Where(b => b.Status == BookStatus.Active).AsQueryable();

        if (!string.IsNullOrWhiteSpace(category) && Enum.TryParse<BookCategory>(category, true, out var parsedCategory))
        {
            query = query.Where(b => b.Category == parsedCategory);
        }

        var books = await query.OrderByDescending(b => b.IsCurrentMonthPick).ThenByDescending(b => b.CreatedAt).ToListAsync(cancellationToken);
        var bookIds = books.Select(b => b.Id).ToList();

        var progresses = await _db.ReadingProgresses
            .Where(p => p.UserId == currentUserId && bookIds.Contains(p.BookId))
            .ToDictionaryAsync(p => p.BookId, cancellationToken);

        return books.Select(b =>
        {
            progresses.TryGetValue(b.Id, out var progress);
            return new BookSummaryDto(
                b.Id,
                b.Title,
                b.Author,
                (BookCategoryDto)b.Category,
                (BookStatusDto)b.Status,
                b.TotalPages,
                b.AssignedMonthLabel,
                b.IsCurrentMonthPick,
                b.CreatedAt,
                progress?.CurrentPage,
                progress?.IsCompleted);
        }).ToList();
    }

    public async Task<BookSummaryDto> UploadAsync(Stream content, string originalFileName, UploadBookRequest meta, Guid uploadedByUserId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(meta.Title))
        {
            throw new ValidationAppException("Kitabın adı tələb olunur.");
        }

        var (relativePath, sizeBytes) = await _fileStorage.SavePdfAsync(content, originalFileName, cancellationToken);

        if (meta.IsCurrentMonthPick && meta.Category == Contracts.Common.BookCategoryDto.General)
        {
            // Only one "current month" pick among general books at a time.
            var currentPicks = await _db.Books
                .Where(b => b.IsCurrentMonthPick && b.Category == BookCategory.General)
                .ToListAsync(cancellationToken);

            foreach (var pick in currentPicks)
            {
                pick.IsCurrentMonthPick = false;
            }
        }

        var book = new Book
        {
            Title = meta.Title.Trim(),
            Author = meta.Author,
            Category = (BookCategory)meta.Category,
            Status = BookStatus.Active,
            FilePath = relativePath,
            FileSizeBytes = sizeBytes,
            TotalPages = meta.TotalPages ?? 0,
            AssignedMonthLabel = meta.AssignedMonthLabel,
            IsCurrentMonthPick = meta.IsCurrentMonthPick,
            UploadedByUserId = uploadedByUserId,
        };

        _db.Books.Add(book);
        await _db.SaveChangesAsync(cancellationToken);

        return new BookSummaryDto(
            book.Id, book.Title, book.Author, (BookCategoryDto)book.Category, (BookStatusDto)book.Status,
            book.TotalPages, book.AssignedMonthLabel, book.IsCurrentMonthPick, book.CreatedAt, null, null);
    }

    public async Task<(Stream Content, string FileName)> OpenPdfAsync(Guid bookId, CancellationToken cancellationToken = default)
    {
        var book = await _db.Books.SingleOrDefaultAsync(b => b.Id == bookId, cancellationToken)
            ?? throw new NotFoundException(nameof(Book), bookId);

        var stream = await _fileStorage.OpenReadAsync(book.FilePath, cancellationToken);
        return (stream, $"{book.Title}.pdf");
    }

    public async Task<ReadingProgressDto> SaveProgressAsync(Guid bookId, Guid userId, SaveReadingProgressRequest request, CancellationToken cancellationToken = default)
    {
        var book = await _db.Books.SingleOrDefaultAsync(b => b.Id == bookId, cancellationToken)
            ?? throw new NotFoundException(nameof(Book), bookId);

        var progress = await _db.ReadingProgresses
            .SingleOrDefaultAsync(p => p.BookId == bookId && p.UserId == userId, cancellationToken);

        var wasCompleted = progress?.IsCompleted ?? false;

        if (progress is null)
        {
            progress = new ReadingProgress { BookId = bookId, UserId = userId };
            _db.ReadingProgresses.Add(progress);
        }

        progress.CurrentPage = Math.Max(1, request.CurrentPage);
        progress.LastReadAt = DateTime.UtcNow;

        var reachedLastPage = book.TotalPages > 0 && progress.CurrentPage >= book.TotalPages;
        if (reachedLastPage && !progress.IsCompleted)
        {
            progress.IsCompleted = true;
            progress.CompletedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(cancellationToken);

        if (progress.IsCompleted && !wasCompleted)
        {
            _db.PointsTransactions.Add(new PointsTransaction
            {
                UserId = userId,
                Source = PointsSource.BookCompleted,
                SourceId = bookId,
                Points = 20,
            });
            await _db.SaveChangesAsync(cancellationToken);
        }

        return new ReadingProgressDto(bookId, progress.CurrentPage, book.TotalPages, progress.IsCompleted, progress.LastReadAt);
    }

    public async Task ArchiveAsync(Guid bookId, CancellationToken cancellationToken = default)
    {
        var book = await _db.Books.SingleOrDefaultAsync(b => b.Id == bookId, cancellationToken)
            ?? throw new NotFoundException(nameof(Book), bookId);

        book.Status = BookStatus.Archived;
        book.IsCurrentMonthPick = false;
        await _db.SaveChangesAsync(cancellationToken);
    }
}
