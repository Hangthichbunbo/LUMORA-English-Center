using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StoryHub.Application.Common.Exceptions;
using StoryHub.Application.Common.Interfaces;
using StoryHub.Application.Common.Options;

namespace StoryHub.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _environment;
    private readonly StorageSettings _settings;
    private readonly ILogger<LocalFileStorageService> _logger;

    // Magic numbers for JPEG, PNG, WEBP
    private static readonly byte[] JpegHeader = [0xFF, 0xD8, 0xFF];
    private static readonly byte[] PngHeader = [0x89, 0x50, 0x4E, 0x47];
    private static readonly byte[] RiffHeader = [0x52, 0x49, 0x46, 0x46]; // "RIFF"
    private static readonly byte[] WebpHeader = [0x57, 0x45, 0x42, 0x50]; // "WEBP"

    public LocalFileStorageService(
        IWebHostEnvironment environment,
        IOptions<StorageSettings> settings,
        ILogger<LocalFileStorageService> logger)
    {
        _environment = environment;
        _settings = settings.Value;
        _logger = logger;
    }

    public bool IsAllowedExtension(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return _settings.AllowedExtensions.Contains(ext);
    }

    public bool IsAllowedMimeType(string contentType)
    {
        return contentType.ToLowerInvariant() switch
        {
            "image/jpeg" or "image/jpg" or "image/png" or "image/webp" => true,
            _ => false
        };
    }

    public async Task<string> SaveFileAsync(
        Stream stream,
        string originalFileName,
        string subFolder,
        CancellationToken cancellationToken = default)
    {
        var ext = Path.GetExtension(originalFileName).ToLowerInvariant();
        if (!IsAllowedExtension(originalFileName))
        {
            throw new BusinessRuleException($"Phần mở rộng tệp tin '{ext}' không được phép tải lên. Chỉ chấp nhận: {string.Join(", ", _settings.AllowedExtensions)}.");
        }

        // Validate File Size (<= 1MB by default)
        if (stream.CanSeek && stream.Length > _settings.MaxFileSizeInBytes)
        {
            var maxMb = (double)_settings.MaxFileSizeInBytes / (1024 * 1024);
            throw new BusinessRuleException($"Dung lượng tệp tin ({stream.Length / 1024.0 / 1024.0:F2} MB) vượt quá giới hạn tối đa cho phép ({maxMb:F0} MB).");
        }

        // Validate File Signature (Magic Numbers)
        byte[] headerBuffer = new byte[12];
        var bytesRead = await stream.ReadAsync(headerBuffer.AsMemory(0, 12), cancellationToken);
        stream.Position = 0; // Reset stream position

        if (!IsValidImageSignature(headerBuffer, bytesRead))
        {
            throw new BusinessRuleException("Nội dung tệp tin không khớp với định dạng ảnh hợp lệ (Phát hiện giả mạo phần mở rộng file).");
        }

        // Generate cryptographically unique safe file name
        var safeFileName = $"{Guid.NewGuid():N}{ext}";
        var webRoot = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");

        string targetDirectory;
        string urlPrefix;

        if (Path.IsPathRooted(_settings.UploadFolder))
        {
            targetDirectory = Path.Combine(_settings.UploadFolder, subFolder);
            urlPrefix = "/uploads";
        }
        else
        {
            var cleanFolder = _settings.UploadFolder.TrimStart('/', '\\');
            targetDirectory = Path.Combine(webRoot, cleanFolder, subFolder);
            urlPrefix = $"/{cleanFolder}";
        }

        if (!Directory.Exists(targetDirectory))
        {
            Directory.CreateDirectory(targetDirectory);
        }

        var fullPath = Path.Combine(targetDirectory, safeFileName);

        using (var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await stream.CopyToAsync(fileStream, cancellationToken);
            if (fileStream.Length > _settings.MaxFileSizeInBytes)
            {
                fileStream.Close();
                if (File.Exists(fullPath)) File.Delete(fullPath);
                var maxMb = (double)_settings.MaxFileSizeInBytes / (1024 * 1024);
                throw new BusinessRuleException($"Dung lượng tệp tin vượt quá giới hạn tối đa cho phép ({maxMb:F0} MB).");
            }
        }

        _logger.LogInformation("Đã lưu tệp an toàn: {SafeFileName} tại thư mục {SubFolder}", safeFileName, subFolder);

        return $"{urlPrefix}/{subFolder}/{safeFileName}".Replace("\\", "/");
    }

    public Task DeleteFileAsync(string? relativePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(relativePath)) return Task.CompletedTask;

        var clean = relativePath.Split('?')[0].Split('#')[0].Trim().Replace('\\', '/');
        while (clean.StartsWith("/"))
        {
            clean = clean[1..];
        }

        // Never delete seed/system images or assets
        if (clean.StartsWith("images/", StringComparison.OrdinalIgnoreCase) ||
            clean.StartsWith("lib/", StringComparison.OrdinalIgnoreCase) ||
            clean.StartsWith("css/", StringComparison.OrdinalIgnoreCase) ||
            clean.StartsWith("js/", StringComparison.OrdinalIgnoreCase) ||
            clean.EndsWith("default-cover.webp", StringComparison.OrdinalIgnoreCase) ||
            clean.EndsWith("default-cover.svg", StringComparison.OrdinalIgnoreCase) ||
            clean.EndsWith("default-avatar.webp", StringComparison.OrdinalIgnoreCase) ||
            clean.EndsWith("default-avatar.svg", StringComparison.OrdinalIgnoreCase))
        {
            return Task.CompletedTask;
        }

        try
        {
            // Normalize sub-path after uploads folder (e.g., "covers/abc.jpg" or "ads/abc.png" or "chapters/abc.jpg")
            var uploadSubPath = clean;
            if (uploadSubPath.StartsWith("app/uploads/", StringComparison.OrdinalIgnoreCase))
            {
                uploadSubPath = uploadSubPath["app/uploads/".Length..];
            }
            else if (uploadSubPath.StartsWith("uploads/", StringComparison.OrdinalIgnoreCase))
            {
                uploadSubPath = uploadSubPath["uploads/".Length..];
            }

            var webRoot = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
            var cleanFolder = (_settings.UploadFolder ?? "uploads").TrimStart('/', '\\');

            var candidatePaths = new List<string>();

            // 1. Rooted UploadFolder if configured
            if (!string.IsNullOrWhiteSpace(_settings.UploadFolder) && Path.IsPathRooted(_settings.UploadFolder))
            {
                candidatePaths.Add(Path.Combine(_settings.UploadFolder, uploadSubPath.Replace('/', Path.DirectorySeparatorChar)));
            }

            // 2. Relative to webRoot/uploads
            candidatePaths.Add(Path.Combine(webRoot, cleanFolder, uploadSubPath.Replace('/', Path.DirectorySeparatorChar)));
            candidatePaths.Add(Path.Combine(webRoot, clean.Replace('/', Path.DirectorySeparatorChar)));

            // 3. Relative to ContentRootPath/uploads
            candidatePaths.Add(Path.Combine(_environment.ContentRootPath, cleanFolder, uploadSubPath.Replace('/', Path.DirectorySeparatorChar)));

            // 4. Container /app/uploads fallback
            if (Directory.Exists("/app/uploads"))
            {
                candidatePaths.Add(Path.Combine("/app/uploads", uploadSubPath.Replace('/', Path.DirectorySeparatorChar)));
            }

            bool deleted = false;
            foreach (var candidate in candidatePaths)
            {
                try
                {
                    var fullPath = Path.GetFullPath(candidate);
                    if (File.Exists(fullPath))
                    {
                        File.Delete(fullPath);
                        _logger.LogInformation("Đã xóa tệp cũ thành công: {FullPath}", fullPath);
                        deleted = true;
                        break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Không thể xóa tệp tại đường dẫn kiểm tra: {Candidate}", candidate);
                }
            }

            if (!deleted)
            {
                _logger.LogDebug("Không tìm thấy tệp để xóa hoặc tệp đã bị xóa trước đó: {RelativePath}", relativePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi xảy ra khi xóa tệp: {RelativePath}", relativePath);
        }

        return Task.CompletedTask;
    }

    private static bool IsValidImageSignature(byte[] header, int bytesRead)
    {
        if (bytesRead < 4) return false;

        // Check JPEG: FF D8 FF
        if (header[0] == JpegHeader[0] && header[1] == JpegHeader[1] && header[2] == JpegHeader[2])
            return true;

        // Check PNG: 89 50 4E 47
        if (header.Take(4).SequenceEqual(PngHeader))
            return true;

        // Check WEBP: "RIFF" .... "WEBP"
        if (bytesRead >= 12 && header.Take(4).SequenceEqual(RiffHeader) &&
            header.Skip(8).Take(4).SequenceEqual(WebpHeader))
            return true;

        return false;
    }
}
