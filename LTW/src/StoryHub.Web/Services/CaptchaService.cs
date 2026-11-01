using System.Security;
using System.Text;
using StoryHub.Application.Common.Interfaces;

namespace StoryHub.Web.Services;

public class CaptchaService : ICaptchaService
{
    private const string CharacterPool = "23456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz";
    private static readonly string[] FontFamilies = ["Arial", "Verdana", "Impact", "Trebuchet MS", "Courier New", "Georgia", "Tahoma"];
    private static readonly string[] TextColors = ["#0f172a", "#1e293b", "#09090b", "#18181b", "#172554", "#1f2937"];
    private static readonly string[] ScratchColors = ["#0f172a", "#1e293b", "#334155", "#475569", "#64748b"];

    public string GenerateRandomCode(int length = 6)
    {
        var buffer = new char[length];
        for (int i = 0; i < length; i++)
        {
            buffer[i] = CharacterPool[Random.Shared.Next(CharacterPool.Length)];
        }
        return new string(buffer);
    }

    public byte[] GenerateCaptchaSvg(string code, int width = 170, int height = 54)
    {
        var sb = new StringBuilder();

        sb.Append($"""<svg xmlns="http://www.w3.org/2000/svg" width="{width}" height="{height}" viewBox="0 0 {width} {height}">""");

        // Subtle gradient background & border
        sb.Append("""
            <defs>
                <linearGradient id="bgGrad" x1="0%" y1="0%" x2="100%" y2="100%">
                    <stop offset="0%" stop-color="#f8fafc" />
                    <stop offset="100%" stop-color="#e2e8f0" />
                </linearGradient>
                <filter id="distort" x="-10%" y="-10%" width="120%" height="120%">
                    <feTurbulence type="fractalNoise" baseFrequency="0.04 0.04" numOctaves="2" result="noise" />
                    <feDisplacementMap in="SourceGraphic" in2="noise" scale="2.5" xChannelSelector="R" yChannelSelector="G" />
                </filter>
            </defs>
            <rect width="100%" height="100%" rx="6" fill="url(#bgGrad)" stroke="#cbd5e1" stroke-width="1.2"/>
        """);

        // Background dots
        int dotCount = Random.Shared.Next(25, 40);
        for (int i = 0; i < dotCount; i++)
        {
            int cx = Random.Shared.Next(4, width - 4);
            int cy = Random.Shared.Next(4, height - 4);
            double r = 0.8 + Random.Shared.NextDouble() * 1.5;
            string dotColor = ScratchColors[Random.Shared.Next(ScratchColors.Length)];
            double opacity = 0.3 + Random.Shared.NextDouble() * 0.4;
            sb.Append($"""<circle cx="{cx}" cy="{cy}" r="{r:0.#}" fill="{dotColor}" opacity="{opacity:0.##}" />""");
        }

        // Background lines & bezier scratches
        int bgLineCount = Random.Shared.Next(6, 10);
        for (int i = 0; i < bgLineCount; i++)
        {
            int x1 = Random.Shared.Next(0, width);
            int y1 = Random.Shared.Next(0, height);
            int x2 = Random.Shared.Next(0, width);
            int y2 = Random.Shared.Next(0, height);
            double strokeWidth = 1.0 + Random.Shared.NextDouble() * 1.5;
            string lineColor = ScratchColors[Random.Shared.Next(ScratchColors.Length)];
            double opacity = 0.4 + Random.Shared.NextDouble() * 0.4;
            sb.Append($"""<line x1="{x1}" y1="{y1}" x2="{x2}" y2="{y2}" stroke="{lineColor}" stroke-width="{strokeWidth:0.#}" opacity="{opacity:0.##}" />""");
        }

        // Background bezier curves
        int curveCount = Random.Shared.Next(2, 4);
        for (int i = 0; i < curveCount; i++)
        {
            int x1 = Random.Shared.Next(0, 30);
            int y1 = Random.Shared.Next(10, height - 10);
            int cx = Random.Shared.Next(width / 4, width * 3 / 4);
            int cy = Random.Shared.Next(5, height - 5);
            int x2 = Random.Shared.Next(width - 30, width);
            int y2 = Random.Shared.Next(10, height - 10);
            string strokeColor = ScratchColors[Random.Shared.Next(ScratchColors.Length)];
            double strokeWidth = 1.2 + Random.Shared.NextDouble() * 1.2;
            sb.Append($"""<path d="M {x1} {y1} Q {cx} {cy} {x2} {y2}" stroke="{strokeColor}" stroke-width="{strokeWidth:0.#}" fill="none" opacity="0.6" />""");
        }

        // Characters group with filter distortion
        sb.Append("""<g filter="url(#distort)">""");

        double charStep = (double)(width - 28) / Math.Max(code.Length, 1);
        for (int i = 0; i < code.Length; i++)
        {
            char c = code[i];
            string safeChar = SecurityElement.Escape(c.ToString());
            string font = FontFamilies[Random.Shared.Next(FontFamilies.Length)];
            string color = TextColors[Random.Shared.Next(TextColors.Length)];
            int fontSize = Random.Shared.Next(28, 34);
            int rot = Random.Shared.Next(-20, 21);

            double posX = 16 + i * charStep + Random.Shared.Next(-3, 4);
            double posY = height * 0.7 + Random.Shared.Next(-3, 4);

            sb.Append($"""<text x="{posX:0.#}" y="{posY:0.#}" font-family="{font}, sans-serif" font-size="{fontSize}" font-weight="bold" fill="{color}" transform="rotate({rot}, {posX:0.#}, {posY:0.#})">{safeChar}</text>""");
        }

        sb.Append("</g>");

        // Foreground scratches directly cutting through letters (matching Image 2)
        int fgScratches = Random.Shared.Next(4, 7);
        for (int i = 0; i < fgScratches; i++)
        {
            int x1 = Random.Shared.Next(5, width - 10);
            int y1 = Random.Shared.Next(10, height - 10);
            int cx1 = Random.Shared.Next(20, width - 20);
            int cy1 = Random.Shared.Next(5, height - 5);
            int cx2 = Random.Shared.Next(20, width - 20);
            int cy2 = Random.Shared.Next(5, height - 5);
            int x2 = Random.Shared.Next(5, width - 10);
            int y2 = Random.Shared.Next(10, height - 10);
            double sw = 1.2 + Random.Shared.NextDouble() * 1.5;
            string color = ScratchColors[Random.Shared.Next(ScratchColors.Length)];
            double opacity = 0.65 + Random.Shared.NextDouble() * 0.3;
            sb.Append($"""<path d="M {x1} {y1} C {cx1} {cy1}, {cx2} {cy2}, {x2} {y2}" stroke="{color}" stroke-width="{sw:0.#}" fill="none" opacity="{opacity:0.##}" stroke-linecap="round" />""");
        }

        sb.Append("</svg>");

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    public bool Validate(string? inputCode, string? storedCode)
    {
        if (string.IsNullOrWhiteSpace(inputCode) || string.IsNullOrWhiteSpace(storedCode))
        {
            return false;
        }

        return string.Equals(inputCode.Trim(), storedCode.Trim(), StringComparison.OrdinalIgnoreCase);
    }
}
