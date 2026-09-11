using OzunuInkisaf.Application.Common.Interfaces;

namespace OzunuInkisaf.Infrastructure.Storage;

/// <summary>
/// Saves uploaded PDFs to a local folder on the API server's disk. This is
/// the simplest thing that works for a first deployment; swap this class
/// for one backed by Azure Blob Storage (or any other object store) later
/// without touching Application or WebApi — they only know IFileStorageService.
/// </summary>
public class LocalFileStorageService : IFileStorageService
{
    private readonly string _rootPath;

    public LocalFileStorageService(string rootPath)
    {
        _rootPath = rootPath;
        Directory.CreateDirectory(_rootPath);
    }

    public async Task<(string RelativePath, long SizeBytes)> SavePdfAsync(Stream content, string originalFileName, CancellationToken cancellationToken = default)
    {
        var safeExtension = Path.GetExtension(originalFileName) is ".pdf" ? ".pdf" : ".pdf";
        var fileName = $"{Guid.NewGuid():N}{safeExtension}";
        var fullPath = Path.Combine(_rootPath, fileName);

        await using (var fileStream = File.Create(fullPath))
        {
            await content.CopyToAsync(fileStream, cancellationToken);
        }

        var sizeBytes = new FileInfo(fullPath).Length;
        return (fileName, sizeBytes);
    }

    public Task<Stream> OpenReadAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_rootPath, relativePath);
        Stream stream = File.OpenRead(fullPath);
        return Task.FromResult(stream);
    }

    public void Delete(string relativePath)
    {
        var fullPath = Path.Combine(_rootPath, relativePath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }
}
