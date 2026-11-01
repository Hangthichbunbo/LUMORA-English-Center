using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StoryHub.Application.Common.Interfaces;
using StoryHub.Application.Common.Options;

namespace StoryHub.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> options, ILogger<EmailService> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    public async Task<bool> SendEmailAsync(string toEmail, string subject, string htmlMessage, CancellationToken cancellationToken = default)
    {
        if (!_settings.Enabled)
        {
            _logger.LogInformation("Tính năng gửi email đang tắt (EmailSettings.Enabled = false). Giả lập gửi email đến: {ToEmail}, Tiêu đề: {Subject}", toEmail, subject);
            return true;
        }

        if (string.IsNullOrWhiteSpace(_settings.SenderEmail) || string.IsNullOrWhiteSpace(_settings.AppPassword))
        {
            _logger.LogWarning("EmailSettings chưa được cấu hình đầy đủ SenderEmail hoặc AppPassword. Không thể gửi email đến {ToEmail}.", toEmail);
            return false;
        }

        try
        {
            using var client = new SmtpClient(_settings.SmtpServer, _settings.SmtpPort)
            {
                Credentials = new NetworkCredential(_settings.SenderEmail, _settings.AppPassword),
                EnableSsl = _settings.EnableSsl
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_settings.SenderEmail, _settings.SenderName),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };
            mailMessage.To.Add(toEmail);

            await client.SendMailAsync(mailMessage, cancellationToken);
            _logger.LogInformation("Đã gửi email thành công đến: {ToEmail}", toEmail);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi gửi email đến {ToEmail} qua SMTP server {Server}:{Port}", toEmail, _settings.SmtpServer, _settings.SmtpPort);
            return false;
        }
    }

    public async Task<bool> SendConfirmationEmailAsync(string toEmail, string displayName, string confirmationLink, CancellationToken cancellationToken = default)
    {
        var subject = "Xác thực tài khoản TruyenVietSub của bạn";
        var safeDisplayName = WebUtility.HtmlEncode(displayName);
        var html = $$"""
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset="utf-8" />
            <meta name="viewport" content="width=device-width, initial-scale=1.0" />
            <title>{{subject}}</title>
            <style>
                body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; background-color: #f4f6f8; margin: 0; padding: 20px; color: #333; }
                .container { max-width: 580px; margin: 0 auto; background: #ffffff; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 15px rgba(0,0,0,0.06); }
                .header { background: linear-gradient(135deg, #0d6efd 0%, #0a58ca 100%); color: #ffffff; padding: 30px 24px; text-align: center; }
                .header h1 { margin: 0; font-size: 24px; font-weight: 700; letter-spacing: 0.5px; }
                .content { padding: 32px 28px; line-height: 1.6; font-size: 15px; }
                .btn-container { text-align: center; margin: 32px 0; }
                .btn { display: inline-block; background-color: #0d6efd; color: #ffffff !important; text-decoration: none; padding: 14px 32px; border-radius: 8px; font-weight: 600; font-size: 16px; box-shadow: 0 2px 6px rgba(13,110,253,0.3); }
                .link-box { background-color: #f8f9fa; border: 1px solid #e9ecef; border-radius: 6px; padding: 12px; word-break: break-all; font-size: 13px; color: #6c757d; margin-top: 20px; }
                .footer { background-color: #f8f9fa; padding: 20px; text-align: center; font-size: 12px; color: #6c757d; border-top: 1px solid #e9ecef; }
            </style>
        </head>
        <body>
            <div class="container">
                <div class="header">
                    <h1>TruyenVietSub</h1>
                    <p style="margin: 6px 0 0; opacity: 0.9; font-size: 14px;">Thế giới truyện online đặc sắc</p>
                </div>
                <div class="content">
                    <p>Xin chào <strong>{{safeDisplayName}}</strong>,</p>
                    <p>Cảm ơn bạn đã đăng ký tài khoản tại <strong>TruyenVietSub</strong>! Để hoàn tất việc kích hoạt tài khoản và trải nghiệm đầy đủ các tính năng đọc truyện, bình luận, và theo dõi, bạn vui lòng xác nhận địa chỉ email của mình.</p>
                    <div class="btn-container">
                        <a href="{{confirmationLink}}" class="btn" target="_blank">KÍCH HOẠT TÀI KHOẢN</a>
                    </div>
                    <p style="font-size: 13px; color: #6c757d;">Hoặc bạn có thể sao chép liên kết dưới đây và dán vào thanh địa chỉ của trình duyệt:</p>
                    <div class="link-box">
                        <a href="{{confirmationLink}}" style="color: #0d6efd; text-decoration: underline;">{{confirmationLink}}</a>
                    </div>
                    <p style="font-size: 13px; color: #6c757d; margin-top: 24px;"><em>Lưu ý: Liên kết xác nhận này sẽ hết hạn sau 24 giờ. Nếu bạn không yêu cầu tạo tài khoản trên TruyenVietSub, vui lòng bỏ qua thư này.</em></p>
                </div>
                <div class="footer">
                    &copy; 2026 TruyenVietSub. Mọi quyền được bảo lưu.<br />
                    Đây là email tự động, vui lòng không trả lời thư này.
                </div>
            </div>
        </body>
        </html>
        """;

        return await SendEmailAsync(toEmail, subject, html, cancellationToken);
    }
}
