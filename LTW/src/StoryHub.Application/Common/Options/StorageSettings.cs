namespace StoryHub.Application.Common.Options;

public class StorageSettings
{
    public const string SectionName = "StorageSettings";

    public string UploadFolder { get; set; } = "uploads";
    public long MaxFileSizeInBytes { get; set; } = 1 * 1024 * 1024; // 1MB
    public string[] AllowedExtensions { get; set; } = [".jpg", ".jpeg", ".png", ".webp"];
}
