namespace StoryHub.Application.Common.Interfaces;

public interface ICaptchaService
{
    string GenerateRandomCode(int length = 6);
    byte[] GenerateCaptchaSvg(string code, int width = 170, int height = 54);
    bool Validate(string? inputCode, string? storedCode);
}
