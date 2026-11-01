import os
from PIL import Image, ImageDraw, ImageFont

COVERS_DIR = r"d:\Software\Web_Truyen\src\StoryHub.Web\wwwroot\images\covers"
os.makedirs(COVERS_DIR, exist_ok=True)

short_stories = [
    {
        "filename": "gio-dau-mua.webp",
        "title": "Gió Đầu Mùa",
        "author": "Thạch Lam",
        "genre": "TRUYỆN NGẮN • VĂN HỌC",
        "bg_top": (44, 62, 80),
        "bg_bot": (189, 195, 199),
        "accent": (243, 156, 18)
    },
    {
        "filename": "chi-pheo.webp",
        "title": "Chí Phèo",
        "author": "Nam Cao",
        "genre": "TRUYỆN NGẮN • HIỆN THỰC",
        "bg_top": (86, 36, 36),
        "bg_bot": (44, 62, 80),
        "accent": (231, 76, 60)
    },
    {
        "filename": "chuyen-tau-dem-khong-dung-lai.webp",
        "title": "Chuyến Tàu Đêm\nKhông Dừng Lại",
        "author": "StoryHub Authors",
        "genre": "TRUYỆN NGẮN • KHOA HUYỄN",
        "bg_top": (15, 23, 42),
        "bg_bot": (67, 56, 202),
        "accent": (56, 189, 248)
    },
    {
        "filename": "quan-tra-luc-nua-dem.webp",
        "title": "Quán Trà Lúc\nNửa Đêm",
        "author": "StoryHub Authors",
        "genre": "TRUYỆN NGẮN • ĐÔ THỊ CHỮA LÀNH",
        "bg_top": (20, 83, 45),
        "bg_bot": (15, 23, 42),
        "accent": (74, 222, 128)
    }
]

W, H = 600, 900

for item in short_stories:
    img = Image.new("RGB", (W, H))
    draw = ImageDraw.Draw(img)

    # Vertical gradient
    r1, g1, b1 = item["bg_top"]
    r2, g2, b2 = item["bg_bot"]
    for y in range(H):
        ratio = y / H
        r = int(r1 + (r2 - r1) * ratio)
        g = int(g1 + (g2 - g1) * ratio)
        b = int(b1 + (b2 - b1) * ratio)
        draw.line([(0, y), (W, y)], fill=(r, g, b))

    # Decorative frames & patterns
    draw.rectangle([25, 25, W - 25, H - 25], outline=item["accent"], width=3)
    draw.rectangle([35, 35, W - 35, H - 35], outline=(255, 255, 255, 80), width=1)

    # Header badge
    draw.rounded_rectangle([70, 70, W - 70, 115], radius=8, fill=(0, 0, 0, 140), outline=item["accent"], width=1)

    # Font setup
    try:
        font_genre = ImageFont.truetype("arialbd.ttf", 20)
        font_title = ImageFont.truetype("arialbd.ttf", 46)
        font_author = ImageFont.truetype("arial.ttf", 26)
        font_brand = ImageFont.truetype("arialbd.ttf", 22)
    except:
        font_genre = ImageFont.load_default()
        font_title = font_genre
        font_author = font_genre
        font_brand = font_genre

    # Text rendering
    draw.text((W / 2, 92), item["genre"], fill=item["accent"], font=font_genre, anchor="mm")
    draw.multiline_text((W / 2, 430), item["title"], fill=(255, 255, 255), font=font_title, anchor="mm", align="center", spacing=15)
    
    # Accent line
    draw.line([(W/2 - 80, 520), (W/2 + 80, 520)], fill=item["accent"], width=3)

    draw.text((W / 2, 570), f"Tác giả: {item['author']}", fill=(230, 230, 230), font=font_author, anchor="mm")
    
    # Bottom watermark
    draw.text((W / 2, H - 65), "STORYHUB.VN • BẢN QUYỀN", fill=(180, 180, 180), font=font_brand, anchor="mm")

    dest_path = os.path.join(COVERS_DIR, item["filename"])
    img.save(dest_path, "WEBP", quality=92)
    print(f"Created: {dest_path}")

print("All short story covers created successfully!")
