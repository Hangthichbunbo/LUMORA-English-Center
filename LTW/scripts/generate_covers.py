import os
import math
from PIL import Image, ImageDraw, ImageFont, ImageFilter

output_dir = r"d:\Software\Web_Truyen\src\StoryHub.Web\wwwroot\images\covers"
os.makedirs(output_dir, exist_ok=True)

stories = [
    {
        "filename": "tru-tien.webp",
        "title": "TRU TIÊN",
        "author": "Tiêu Đỉnh",
        "genre": "TIÊN HIỆP",
        "color1": (15, 32, 39),
        "color2": (32, 58, 67),
        "color3": (44, 115, 105),
        "accent": (78, 205, 196)
    },
    {
        "filename": "dau-pha-thuong-khung.webp",
        "title": "ĐẤU PHÁ\nTHƯƠNG KHUNG",
        "author": "Thiên Tằm Thổ Đậu",
        "genre": "HUYỀN HUYỄN",
        "color1": (40, 10, 10),
        "color2": (120, 20, 20),
        "color3": (210, 70, 20),
        "accent": (255, 180, 50)
    },
    {
        "filename": "vu-dong-can-khon.webp",
        "title": "VŨ ĐỘNG\nCÀN KHÔN",
        "author": "Thiên Tằm Thổ Đậu",
        "genre": "HUYỀN HUYỄN",
        "color1": (20, 10, 45),
        "color2": (50, 25, 95),
        "color3": (100, 50, 160),
        "accent": (200, 120, 255)
    },
    {
        "filename": "dai-chua-te.webp",
        "title": "ĐẠI CHÚA TỂ",
        "author": "Thiên Tằm Thổ Đậu",
        "genre": "HUYỀN HUYỄN",
        "color1": (10, 20, 45),
        "color2": (25, 45, 90),
        "color3": (180, 130, 40),
        "accent": (255, 215, 0)
    },
    {
        "filename": "tinh-than-bien.webp",
        "title": "TINH THẦN BIẾN",
        "author": "Ngã Cật Tây Hồng Thị",
        "genre": "TIÊN HIỆP",
        "color1": (5, 15, 35),
        "color2": (15, 40, 80),
        "color3": (40, 90, 160),
        "accent": (100, 210, 255)
    },
    {
        "filename": "thon-phe-tinh-khong.webp",
        "title": "THÔN PHỆ\nTINH KHÔNG",
        "author": "Ngã Cật Tây Hồng Thị",
        "genre": "KHOA HUYỄN",
        "color1": (8, 12, 24),
        "color2": (20, 30, 55),
        "color3": (0, 130, 180),
        "accent": (0, 230, 255)
    },
    {
        "filename": "ban-long.webp",
        "title": "BÀN LONG",
        "author": "Ngã Cật Tây Hồng Thị",
        "genre": "HUYỀN HUYỄN",
        "color1": (15, 30, 20),
        "color2": (35, 60, 40),
        "color3": (70, 110, 60),
        "accent": (140, 220, 120)
    },
    {
        "filename": "luc-tien.webp",
        "title": "LỤC TIÊN",
        "author": "Tiêu Đỉnh",
        "genre": "TIÊN HIỆP",
        "color1": (25, 10, 25),
        "color2": (60, 20, 50),
        "color3": (130, 30, 80),
        "accent": (255, 100, 150)
    },
    {
        "filename": "nguyen-ton.webp",
        "title": "NGUYÊN TÔN",
        "author": "Thiên Tằm Thổ Đậu",
        "genre": "HUYỀN HUYỄN",
        "color1": (30, 15, 50),
        "color2": (70, 25, 80),
        "color3": (160, 50, 70),
        "accent": (255, 140, 80)
    },
    {
        "filename": "mang-hoang-ky.webp",
        "title": "MÃNG HOANG KỶ",
        "author": "Ngã Cật Tây Hồng Thị",
        "genre": "TIÊN HIỆP",
        "color1": (35, 25, 15),
        "color2": (70, 50, 25),
        "color3": (140, 95, 45),
        "accent": (240, 190, 90)
    }
]

W, H = 400, 560

try:
    font_genre = ImageFont.truetype(r"C:\Windows\Fonts\segoeuib.ttf", 16)
    font_title = ImageFont.truetype(r"C:\Windows\Fonts\segoeuib.ttf", 36)
    font_author = ImageFont.truetype(r"C:\Windows\Fonts\segoeui.ttf", 20)
    font_brand = ImageFont.truetype(r"C:\Windows\Fonts\segoeuib.ttf", 14)
except Exception:
    font_genre = ImageFont.load_default()
    font_title = ImageFont.load_default()
    font_author = ImageFont.load_default()
    font_brand = ImageFont.load_default()

for s in stories:
    img = Image.new("RGB", (W, H))
    draw = ImageDraw.Draw(img)

    c1 = s["color1"]
    c2 = s["color2"]
    c3 = s["color3"]
    accent = s["accent"]

    # 3-stop vertical gradient with slight diagonal shift
    for y in range(H):
        t = y / (H - 1)
        if t < 0.5:
            local_t = t * 2.0
            r = int(c1[0] + (c2[0] - c1[0]) * local_t)
            g = int(c1[1] + (c2[1] - c1[1]) * local_t)
            b = int(c1[2] + (c2[2] - c1[2]) * local_t)
        else:
            local_t = (t - 0.5) * 2.0
            r = int(c2[0] + (c3[0] - c2[0]) * local_t)
            g = int(c2[1] + (c3[1] - c2[1]) * local_t)
            b = int(c2[2] + (c3[2] - c2[2]) * local_t)
        draw.line([(0, y), (W, y)], fill=(r, g, b))

    # Decorative geometric patterns
    # Subtle concentric circles in center
    cx, cy = W // 2, H // 2 - 30
    for rad in range(160, 40, -30):
        alpha_color = tuple(int(accent[i] * 0.25) for i in range(3))
        draw.ellipse([cx - rad, cy - rad, cx + rad, cy + rad], outline=alpha_color, width=1)

    # Frame border
    draw.rectangle([18, 18, W - 18, H - 18], outline=accent, width=2)
    draw.rectangle([24, 24, W - 24, H - 24], outline=(255, 255, 255, 80), width=1)

    # Corner decorations
    cw = 18
    for ox, oy in [(24, 24), (W - 24, 24), (24, H - 24), (W - 24, H - 24)]:
        dx = -cw if ox > W // 2 else cw
        dy = -cw if oy > H // 2 else cw
        draw.line([(ox, oy), (ox + dx, oy)], fill=accent, width=2)
        draw.line([(ox, oy), (ox, oy + dy)], fill=accent, width=2)

    # Genre badge at top
    genre_text = s["genre"]
    draw.text((W // 2, 55), genre_text, font=font_genre, fill=accent, anchor="mm")
    draw.line([(W // 2 - 40, 72), (W // 2 + 40, 72)], fill=accent, width=1)

    # Title in middle
    title_text = s["title"]
    # Shadow for readability
    for offset in [(-2, -2), (2, 2), (-2, 2), (2, -2), (0, 3)]:
        draw.multiline_text((W // 2 + offset[0], cy + offset[1]), title_text, font=font_title, fill=(0, 0, 0), anchor="mm", align="center", spacing=8)
    draw.multiline_text((W // 2, cy), title_text, font=font_title, fill=(255, 255, 255), anchor="mm", align="center", spacing=8)

    # Author below title
    author_text = f"Tác giả: {s['author']}"
    draw.text((W // 2 + 1, H - 110 + 1), author_text, font=font_author, fill=(0, 0, 0), anchor="mm")
    draw.text((W // 2, H - 110), author_text, font=font_author, fill=(220, 230, 242), anchor="mm")

    # Brand badge at bottom
    draw.line([(W // 2 - 60, H - 65), (W // 2 + 60, H - 65)], fill=accent, width=1)
    draw.text((W // 2, H - 45), "STORYHUB • ĐỌC TRUYỆN ONLINE", font=font_brand, fill=(180, 195, 210), anchor="mm")

    out_path = os.path.join(output_dir, s["filename"])
    img.save(out_path, "WEBP", quality=90)
    print(f"Generated: {out_path}")

print("All covers generated successfully!")
