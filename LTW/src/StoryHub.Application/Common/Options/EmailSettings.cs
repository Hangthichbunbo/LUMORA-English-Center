namespace StoryHub.Application.Common.Options;

public class EmailSettings
{
    public const string SectionName = "EmailSettings";

    public bool Enabled { get; set; } = false;
    public string SmtpServer { get; set; } = "smtp.gmail.com";
    public int SmtpPort { get; set; } = 587;
    public string SenderEmail { get; set; } = "truyenvietsub.web@gmail.com";
    public string SenderName { get; set; } = "TruyenVietSub";
    public string AppPassword { get; set; } = string.Empty;
    public bool EnableSsl { get; set; } = true;
}
