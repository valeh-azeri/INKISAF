namespace OzunuInkisaf.Application.Common.Interfaces;

public interface IFileStorageService
{
    /// <summary>
    /// Persists an uploaded PDF stream under a generated, collision-free
    /// name and returns the relative path/URL to store on the Book entity.
    /// </summary>
    Task<(string RelativePath, long SizeBytes)> SavePdfAsync(Stream content, string originalFileName, CancellationToken cancellationToken = default);

    Task<Stream> OpenReadAsync(string relativePath, CancellationToken cancellationToken = default);

    void Delete(string relativePath);
}
