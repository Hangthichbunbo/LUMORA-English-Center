import os

svg_content = """<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 512 512" width="100%" height="100%">
  <defs>
    <linearGradient id="bgGrad" x1="0%" y1="0%" x2="100%" y2="100%">
      <stop offset="0%" style="stop-color:#4338ca;stop-opacity:1" />
      <stop offset="50%" style="stop-color:#4f46e5;stop-opacity:1" />
      <stop offset="100%" style="stop-color:#2563eb;stop-opacity:1" />
    </linearGradient>
    <linearGradient id="goldGrad" x1="0%" y1="0%" x2="100%" y2="100%">
      <stop offset="0%" style="stop-color:#fbbf24;stop-opacity:1" />
      <stop offset="100%" style="stop-color:#f59e0b;stop-opacity:1" />
    </linearGradient>
  </defs>

  <rect width="512" height="512" rx="112" fill="url(#bgGrad)"/>
  <rect x="8" y="8" width="496" height="496" rx="104" fill="none" stroke="#ffffff" stroke-opacity="0.15" stroke-width="4"/>

  <g>
    <!-- Left Page -->
    <path d="M256 368 C220 348 160 344 116 354 C104 357 96 348 96 336 L96 172 C96 160 106 150 118 148 C164 138 220 144 256 168 Z" fill="#f8fafc" />
    <!-- Right Page -->
    <path d="M256 368 C292 348 352 344 396 354 C408 357 416 348 416 336 L416 172 C416 160 406 150 394 148 C348 138 292 144 256 168 Z" fill="#ffffff" />
    <!-- Center Spine -->
    <path d="M256 168 L256 372" stroke="#94a3b8" stroke-width="4" stroke-linecap="round"/>

    <!-- Text Lines Left -->
    <line x1="136" y1="196" x2="224" y2="196" stroke="#cbd5e1" stroke-width="8" stroke-linecap="round" />
    <line x1="136" y1="236" x2="224" y2="236" stroke="#cbd5e1" stroke-width="8" stroke-linecap="round" />
    <line x1="136" y1="276" x2="198" y2="276" stroke="#cbd5e1" stroke-width="8" stroke-linecap="round" />

    <!-- Text Lines Right -->
    <line x1="288" y1="196" x2="376" y2="196" stroke="#cbd5e1" stroke-width="8" stroke-linecap="round" />
    <line x1="288" y1="236" x2="376" y2="236" stroke="#cbd5e1" stroke-width="8" stroke-linecap="round" />
    <line x1="288" y1="276" x2="346" y2="276" stroke="#cbd5e1" stroke-width="8" stroke-linecap="round" />

    <!-- Golden Bookmark Ribbon -->
    <path d="M240 140 L272 140 L272 226 L256 210 L240 226 Z" fill="url(#goldGrad)" />

    <!-- Top Right Star Badge (VietSub) -->
    <g transform="translate(376, 120)">
      <circle cx="0" cy="0" r="34" fill="#ef4444" stroke="#ffffff" stroke-width="5"/>
      <polygon points="0,-20 6,-6 20,-6 9,3 13,17 0,9 -13,17 -9,3 -20,-6 -6,-6" fill="#fbbf24"/>
    </g>
  </g>
</svg>"""

manifest_content = """{
  "name": "TruyenVietSub - Đọc Truyện Online Trực Tuyến Đỉnh Cao",
  "short_name": "TruyenVietSub",
  "description": "Nền tảng đọc truyện chữ trực tuyến cập nhật nhanh nhất, truyện tiên hiệp, kiếm hiệp, huyền huyễn, đô thị, ngôn tình hấp dẫn.",
  "icons": [
    {
      "src": "/android-chrome-192x192.png",
      "sizes": "192x192",
      "type": "image/png"
    },
    {
      "src": "/android-chrome-512x512.png",
      "sizes": "512x512",
      "type": "image/png"
    }
  ],
  "theme_color": "#4f46e5",
  "background_color": "#ffffff",
  "display": "standalone",
  "start_url": "/"
}"""

for path in [r"src\StoryHub.Web\wwwroot\favicon.svg", r"src\StoryHub.Web\wwwroot\images\logo-icon.svg"]:
    with open(path, "w", encoding="utf-8") as f:
        f.write(svg_content)
    print("Wrote", path, "size:", len(svg_content))

with open(r"src\StoryHub.Web\wwwroot\site.webmanifest", "w", encoding="utf-8") as f:
    f.write(manifest_content)
print("Wrote manifest size:", len(manifest_content))

