using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OzunuInkisaf.Application.Services;
using OzunuInkisaf.Application.Common.Interfaces;
using OzunuInkisaf.Contracts.Books;
using OzunuInkisaf.Contracts.Common;

namespace OzunuInkisaf.WebApi.Controllers;

/// <summary>
/// Kitab/Quran PDF-ləri: admin yükləməsi, istifadəçi siyahısı və oxuma
/// (səhifə qaldığı yerdən davam) əməliyyatları.
/// </summary>
[ApiController]
[Route("api/books")]
[Authorize]
public class BooksController : ControllerBase
{
    private readonly IBookService _bookService;
    private readonly ICurrentUserService _currentUser;

    public BooksController(IBookService bookService, ICurrentUserService currentUser)
    {
        _bookService = bookService;
        _currentUser = currentUser;
    }

    /// <summary>Bütün kitabların siyahısı (cari istifadəçinin oxuma irəliləyişi ilə birgə).</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BookSummaryDto>>> GetAll([FromQuery] string? category, CancellationToken cancellationToken)
    {
        var result = await _bookService.GetAllAsync(_currentUser.UserId!.Value, category, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Admin tərəfindən tək (və ya bulk üçün təkrar-təkrar çağırılan) PDF yükləməsi.
    /// multipart/form-data formatında: fayl + metadata sahələri.
    /// </summary>
    [Authorize(Policy = "AdminOnly")]
    [HttpPost("upload")]
    [RequestSizeLimit(200_000_000)]
    public async Task<ActionResult<BookSummaryDto>> Upload([FromForm] UploadBookFormRequest form, CancellationToken cancellationToken)
    {
        if (form.File is null || form.File.Length == 0)
        {
            return BadRequest(new ApiError { Message = "PDF faylı boşdur." });
        }

        var meta = new UploadBookRequest(
            form.Title,
            form.Author,
            form.Category,
            form.AssignedMonthLabel,
            form.IsCurrentMonthPick,
            form.NotifyAllUsers,
            form.TotalPages);

        await using var stream = form.File.OpenReadStream();
        var result = await _bookService.UploadAsync(stream, form.File.FileName, meta, _currentUser.UserId!.Value, cancellationToken);
        return Ok(result);
    }

    /// <summary>PDF-in faktiki faylını axın (stream) olaraq qaytarır.</summary>
    [HttpGet("{bookId:guid}/pdf")]
    public async Task<IActionResult> OpenPdf(Guid bookId, CancellationToken cancellationToken)
    {
        var (content, fileName) = await _bookService.OpenPdfAsync(bookId, cancellationToken);
        return File(content, "application/pdf", fileName);
    }

    /// <summary>Cari istifadəçinin bu kitabdakı oxuma mövqeyini (səhifə) yadda saxlayır.</summary>
    [HttpPut("{bookId:guid}/progress")]
    public async Task<ActionResult<ReadingProgressDto>> SaveProgress(Guid bookId, [FromBody] SaveReadingProgressRequest request, CancellationToken cancellationToken)
    {
        var result = await _bookService.SaveProgressAsync(bookId, _currentUser.UserId!.Value, request, cancellationToken);
        return Ok(result);
    }

    /// <summary>Kitabı arxivləşdirir (siyahıdan gizlədir, silmir).</summary>
    [Authorize(Policy = "AdminOnly")]
    [HttpPost("{bookId:guid}/archive")]
    public async Task<IActionResult> Archive(Guid bookId, CancellationToken cancellationToken)
    {
        await _bookService.ArchiveAsync(bookId, cancellationToken);
        return NoContent();
    }
}

/// <summary>
/// multipart/form-data ilə kitab yükləmək üçün forma modeli. <see cref="UploadBookRequest"/>
/// bir record olduğundan (və PDF faylı özü DTO-nun bir hissəsi olmadığından)
/// bunun üçün ayrıca, [FromForm] ilə uyğun bir sinif istifadə olunur.
/// </summary>
public class UploadBookFormRequest
{
    public IFormFile? File { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Author { get; set; }

    public BookCategoryDto Category { get; set; }

    public string? AssignedMonthLabel { get; set; }

    public bool IsCurrentMonthPick { get; set; }

    public bool NotifyAllUsers { get; set; }

    public int? TotalPages { get; set; }
}
