namespace StoryHub.Application.Common.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(Stream stream, string originalFileName, string subFolder, CancellationToken cancellationToken = default);
    Task DeleteFileAsync(string? relativePath, CancellationToken cancellationToken = default);
    bool IsAllowedExtension(string fileName);
    bool IsAllowedMimeType(string contentType);
}
