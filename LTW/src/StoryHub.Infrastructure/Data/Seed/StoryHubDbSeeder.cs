using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StoryHub.Application.Common.Options;
using StoryHub.Domain.Entities;
using StoryHub.Domain.Enums;

namespace StoryHub.Infrastructure.Data.Seed;

public static class StoryHubDbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<StoryHubDbContext>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var seedOptions = serviceProvider.GetRequiredService<IOptions<SeedSettings>>();
        var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(StoryHubDbSeeder));

        await SeedAsync(context, userManager, roleManager, seedOptions, logger);
    }

    public static async Task SeedAsync(
        StoryHubDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IOptions<SeedSettings> seedOptions,
        ILogger logger)
    {
        var settings = seedOptions.Value;

        // 1. Seed Roles
        string[] roles = ["Admin", "Moderator", "User"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
                logger.LogInformation("Đã khởi tạo Role: {Role}", role);
            }
        }

        // 2. Seed Users safely from options/secrets
        string adminId = string.Empty;
        if (settings.SeedDefaultUsers && !string.IsNullOrWhiteSpace(settings.AdminEmail) && !string.IsNullOrWhiteSpace(settings.AdminPassword))
        {
            var adminUser = await userManager.FindByEmailAsync(settings.AdminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = settings.AdminEmail,
                    Email = settings.AdminEmail,
                    DisplayName = "Quản trị viên tối cao",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTimeOffset.UtcNow
                };

                var result = await userManager.CreateAsync(adminUser, settings.AdminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                    adminId = adminUser.Id;
                    logger.LogInformation("Đã tạo tài khoản Quản trị viên: {Email}", settings.AdminEmail);
                }
                else
                {
                    logger.LogWarning("Không thể tạo Admin seed: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                adminId = adminUser.Id;
                if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
                var token = await userManager.GeneratePasswordResetTokenAsync(adminUser);
                await userManager.ResetPasswordAsync(adminUser, token, settings.AdminPassword);
                logger.LogInformation("Đã cập nhật vai trò và mật khẩu cho Quản trị viên: {Email}", settings.AdminEmail);
            }

            // Moderator
            if (!string.IsNullOrWhiteSpace(settings.ModeratorEmail) && !string.IsNullOrWhiteSpace(settings.ModeratorPassword))
            {
                var mod = await userManager.FindByEmailAsync(settings.ModeratorEmail);
                if (mod == null)
                {
                    mod = new ApplicationUser
                    {
                        UserName = settings.ModeratorEmail,
                        Email = settings.ModeratorEmail,
                        DisplayName = "Kiểm duyệt viên",
                        EmailConfirmed = true,
                        IsActive = true,
                        CreatedAt = DateTimeOffset.UtcNow
                    };
                    var res = await userManager.CreateAsync(mod, settings.ModeratorPassword);
                    if (res.Succeeded) await userManager.AddToRoleAsync(mod, "Moderator");
                }
            }

            // Reader User
            if (!string.IsNullOrWhiteSpace(settings.UserEmail) && !string.IsNullOrWhiteSpace(settings.UserPassword))
            {
                var reader = await userManager.FindByEmailAsync(settings.UserEmail);
                if (reader == null)
                {
                    reader = new ApplicationUser
                    {
                        UserName = settings.UserEmail,
                        Email = settings.UserEmail,
                        DisplayName = "Độc giả gương mẫu",
                        EmailConfirmed = true,
                        IsActive = true,
                        CreatedAt = DateTimeOffset.UtcNow
                    };
                    var res = await userManager.CreateAsync(reader, settings.UserPassword);
                    if (res.Succeeded) await userManager.AddToRoleAsync(reader, "User");
                }
            }
        }

        // 3. Seed Categories
        if (!await context.Categories.AnyAsync())
        {
            var categories = new List<Category>
            {
                new() { Name = "Tiên Hiệp", Slug = "tien-hiep", Description = "Tu chân luyện khí, đắc đạo thành tiên, phi thăng tiên giới.", IsActive = true },
                new() { Name = "Kiếm Hiệp", Slug = "kiem-hiep", Description = "Giang hồ ân oán, võ lâm tranh bá, hiệp khách trượng nghĩa.", IsActive = true },
                new() { Name = "Huyền Huyễn", Slug = "huyen-huan", Description = "Thế giới phép thuật kỳ ảo, đấu khí ma pháp, dị giới đại lục.", IsActive = true },
                new() { Name = "Đô Thị", Slug = "do-thi", Description = "Cuộc sống hiện đại, trọng sinh, thương nghiệp, dị năng đô thị.", IsActive = true },
                new() { Name = "Khoa Huyễn", Slug = "khoa-huan", Description = "Tương lai cơ giáp, du hành vũ trụ, công nghệ viễn tưởng.", IsActive = true },
                new() { Name = "Võng Du", Slug = "vong-du", Description = "Thực tế ảo, game online, thăng cấp săn boss đỉnh phong.", IsActive = true },
                new() { Name = "Ngôn Tình", Slug = "ngon-tinh", Description = "Chuyện tình cảm sâu sắc, thanh xuân vườn trường, ngọt ngào cảm động.", IsActive = true }
            };
            await context.Categories.AddRangeAsync(categories);
            await context.SaveChangesAsync();
            logger.LogInformation("Đã khởi tạo 7 thể loại truyện mẫu.");
        }

        // 4. Seed Authors
        if (!await context.Authors.AnyAsync())
        {
            var authors = new List<Author>
            {
                new() { Name = "Tiêu Đỉnh", Slug = "tieu-dinh", Biography = "Tác giả nổi tiếng với kiệt tác Tru Tiên, văn phong trầm hùng, giàu cảm xúc." },
                new() { Name = "Thiên Tằm Thổ Đậu", Slug = "thien-tam-tho-dau", Biography = "Đại thần văn học mạng, tác giả Đấu Phá Thương Khung, Vũ Động Càn Khôn, Đại Chúa Tể." },
                new() { Name = "Ngã Cật Tây Hồng Thị", Slug = "nga-cat-tay-hong-thi", Biography = "Tác giả của Tinh Thần Biến, Thôn Phệ Tinh Không, Bàn Long." }
            };
            await context.Authors.AddRangeAsync(authors);
            await context.SaveChangesAsync();
            logger.LogInformation("Đã khởi tạo 3 tác giả tiêu biểu.");
        }

        // 5. Seed Stories & Chapters
        bool isInitialStorySeed = false;
        if (!await context.Stories.IgnoreQueryFilters().AnyAsync())
        {
            isInitialStorySeed = true;
            var authors = await context.Authors.ToListAsync();
            var categories = await context.Categories.ToListAsync();

            var authorTieuDinh = authors.First(a => a.Slug == "tieu-dinh");
            var authorThoDau = authors.First(a => a.Slug == "thien-tam-tho-dau");
            var authorCaChua = authors.First(a => a.Slug == "nga-cat-tay-hong-thi");

            var catTienHiep = categories.First(c => c.Slug == "tien-hiep");
            var catHuyenHuan = categories.First(c => c.Slug == "huyen-huan");
            var catKiemHiep = categories.First(c => c.Slug == "kiem-hiep");
            var catKhoaHuan = categories.First(c => c.Slug == "khoa-huan");
            var catVongDu = categories.First(c => c.Slug == "vong-du");

            string creatorId = string.IsNullOrEmpty(adminId) ? "system-admin" : adminId;

            var stories = new List<Story>
            {
                new()
                {
                    Title = "Tru Tiên",
                    Slug = "tru-tien",
                    AlternativeTitle = "Chân Tình Vô Hối",
                    Description = "Thời cổ đại, trời đất hoang sơ, con người nhỏ bé trước thiên nhiên hùng vĩ. Trương Tiểu Phàm xuất thân bình phàm, nắm giữ Phệ Huyết Châu, bước vào con đường tu chân nghịch thiên cải mệnh.",
                    CoverImageUrl = "/images/covers/tru-tien.webp",
                    AuthorId = authorTieuDinh.Id,
                    Status = StoryStatus.Completed,
                    PublishStatus = PublishStatus.Published,
                    IsFeatured = true,
                    ViewCount = 158200,
                    AverageRating = 4.90m,
                    RatingCount = 120,
                    CreatedBy = creatorId,
                    PublishedAt = DateTimeOffset.UtcNow.AddMonths(-12)
                },
                new()
                {
                    Title = "Đấu Phá Thương Khung",
                    Slug = "dau-pha-thuong-khung",
                    AlternativeTitle = "Battle Through the Heavens",
                    Description = "Nơi đây không có ma pháp diễm lệ, chỉ có Đấu Khí phồn thịnh đến đỉnh phong! Tiêu Viêm từ một phế vật thiên tài, chịu sỉ nhục từ hôn, mang theo giới chỉ của Dược Lão quật khởi phong vân.",
                    CoverImageUrl = "/images/covers/dau-pha-thuong-khung.webp",
                    AuthorId = authorThoDau.Id,
                    Status = StoryStatus.Completed,
                    PublishStatus = PublishStatus.Published,
                    IsFeatured = true,
                    ViewCount = 245000,
                    AverageRating = 4.85m,
                    RatingCount = 210,
                    CreatedBy = creatorId,
                    PublishedAt = DateTimeOffset.UtcNow.AddMonths(-10)
                },
                new()
                {
                    Title = "Vũ Động Càn Khôn",
                    Slug = "vu-dong-can-khon",
                    AlternativeTitle = "Martial Universe",
                    Description = "Lâm Động - một thiếu niên bình thường của phân gia Lâm thị, vô tình nhặt được Thạch Phù thần bí trong động đá, từ đó một đường vượt mọi chông gai bước lên đỉnh cao thế giới.",
                    CoverImageUrl = "/images/covers/vu-dong-can-khon.webp",
                    AuthorId = authorThoDau.Id,
                    Status = StoryStatus.Completed,
                    PublishStatus = PublishStatus.Published,
                    IsFeatured = true,
                    ViewCount = 98500,
                    AverageRating = 4.70m,
                    RatingCount = 85,
                    CreatedBy = creatorId,
                    PublishedAt = DateTimeOffset.UtcNow.AddMonths(-8)
                },
                new()
                {
                    Title = "Đại Chúa Tể",
                    Slug = "dai-chua-te",
                    AlternativeTitle = "The Great Ruler",
                    Description = "Đại Thiên Thế Giới, nơi giao thoa của vạn đạo. Mục Trần từ Bắc Linh Cảnh bước ra, nắm giữ Thần Thú huyết mạch, sánh vai cùng Viêm Đế và Vũ Tổ tại đỉnh phong danh vọng.",
                    CoverImageUrl = "/images/covers/dai-chua-te.webp",
                    AuthorId = authorThoDau.Id,
                    Status = StoryStatus.Published,
                    PublishStatus = PublishStatus.Published,
                    IsFeatured = false,
                    ViewCount = 67300,
                    AverageRating = 4.60m,
                    RatingCount = 45,
                    CreatedBy = creatorId,
                    PublishedAt = DateTimeOffset.UtcNow.AddMonths(-6)
                },
                new()
                {
                    Title = "Tinh Thần Biến",
                    Slug = "tinh-than-bien",
                    AlternativeTitle = "Legend of Immortals",
                    Description = "Tần Vũ trời sinh đan điền khiếm khuyết không thể tu luyện nội công. Không chịu khuất phục, cậu khổ luyện ngoại công đạt cực hạn, dung hợp Lưu Tinh Lệ sáng tạo công pháp Tinh Thần Biến chấn động tam giới.",
                    CoverImageUrl = "/images/covers/tinh-than-bien.webp",
                    AuthorId = authorCaChua.Id,
                    Status = StoryStatus.Completed,
                    PublishStatus = PublishStatus.Published,
                    IsFeatured = true,
                    ViewCount = 112000,
                    AverageRating = 4.80m,
                    RatingCount = 92,
                    CreatedBy = creatorId,
                    PublishedAt = DateTimeOffset.UtcNow.AddMonths(-11)
                },
                new()
                {
                    Title = "Thôn Phệ Tinh Không",
                    Slug = "thon-phe-tinh-khong",
                    AlternativeTitle = "Swallowed Star",
                    Description = "Trái Đất sau thảm họa virus RR, quái thú hoành hành. Thiếu niên La Phong thức tỉnh tinh thần niệm sư, gia nhập võ quán cực hạn, chiến đấu bảo vệ nhân loại và bước ra vũ trụ vô tận.",
                    CoverImageUrl = "/images/covers/thon-phe-tinh-khong.webp",
                    AuthorId = authorCaChua.Id,
                    Status = StoryStatus.Completed,
                    PublishStatus = PublishStatus.Published,
                    IsFeatured = true,
                    ViewCount = 189000,
                    AverageRating = 4.92m,
                    RatingCount = 150,
                    CreatedBy = creatorId,
                    PublishedAt = DateTimeOffset.UtcNow.AddMonths(-9)
                },
                new()
                {
                    Title = "Bàn Long",
                    Slug = "ban-long",
                    AlternativeTitle = "Coiling Dragon",
                    Description = "Lâm Lôi Ba Lỗ Khắc - hậu nhân của gia tộc Thanh Long Chiến Sĩ suy tàn, tìm thấy Bàn Long giới chỉ trong đống đổ nát, bắt đầu truyền kỳ tu luyện Phong - Địa nguyên tố vĩ đại.",
                    CoverImageUrl = "/images/covers/ban-long.webp",
                    AuthorId = authorCaChua.Id,
                    Status = StoryStatus.Completed,
                    PublishStatus = PublishStatus.Published,
                    IsFeatured = false,
                    ViewCount = 84000,
                    AverageRating = 4.75m,
                    RatingCount = 68,
                    CreatedBy = creatorId,
                    PublishedAt = DateTimeOffset.UtcNow.AddMonths(-7)
                },
                new()
                {
                    Title = "Lục Tiên",
                    Slug = "luc-tien",
                    AlternativeTitle = "Six Immortals",
                    Description = "Tiếp nối tinh thần hào hùng của Tru Tiên, tác phẩm Lục Tiên của Tiêu Đỉnh khắc họa thế giới tu chân tàn khốc, nơi nhân tâm và ma đạo đan xen khôn lường.",
                    CoverImageUrl = "/images/covers/luc-tien.webp",
                    AuthorId = authorTieuDinh.Id,
                    Status = StoryStatus.Published,
                    PublishStatus = PublishStatus.Published,
                    IsFeatured = false,
                    ViewCount = 42100,
                    AverageRating = 4.50m,
                    RatingCount = 30,
                    CreatedBy = creatorId,
                    PublishedAt = DateTimeOffset.UtcNow.AddMonths(-5)
                },
                new()
                {
                    Title = "Nguyên Tôn",
                    Slug = "nguyen-ton",
                    AlternativeTitle = "Dragon Prince Yuan",
                    Description = "Chu Nguyên sinh ra với Bát Mạch Thánh Long, nhưng bị Đại Vũ vương triều cướp đoạt khí vận. Cậu mở lại bát mạch, tay cầm Thiên Nguyên Bút chém tan cừu địch đoạt lại vinh quang.",
                    CoverImageUrl = "/images/covers/nguyen-ton.webp",
                    AuthorId = authorThoDau.Id,
                    Status = StoryStatus.Published,
                    PublishStatus = PublishStatus.Published,
                    IsFeatured = true,
                    ViewCount = 76000,
                    AverageRating = 4.65m,
                    RatingCount = 55,
                    CreatedBy = creatorId,
                    PublishedAt = DateTimeOffset.UtcNow.AddMonths(-4)
                },
                new()
                {
                    Title = "Mãng Hoang Kỷ",
                    Slug = "mang-hoang-ky",
                    AlternativeTitle = "Desolate Era",
                    Description = "Kỷ Ninh mắc bệnh hiểm nghèo qua đời, chuyển sinh về thời thượng cổ Mãng Hoang thế giới. Nắm giữ Lạc Thần Đồ, lĩnh ngộ Kiếm Đạo tối cao, cậu từng bước trở thành Hỗn Độn Chúa Tể.",
                    CoverImageUrl = "/images/covers/mang-hoang-ky.webp",
                    AuthorId = authorCaChua.Id,
                    Status = StoryStatus.Completed,
                    PublishStatus = PublishStatus.Published,
                    IsFeatured = false,
                    ViewCount = 93200,
                    AverageRating = 4.82m,
                    RatingCount = 77,
                    CreatedBy = creatorId,
                    PublishedAt = DateTimeOffset.UtcNow.AddMonths(-3)
                }
            };

            await context.Stories.AddRangeAsync(stories);
            await context.SaveChangesAsync();

            // Link categories
            foreach (var s in stories)
            {
                if (s.Slug is "tru-tien" or "luc-tien")
                {
                    context.StoryCategories.Add(new StoryCategory { StoryId = s.Id, CategoryId = catTienHiep.Id });
                    context.StoryCategories.Add(new StoryCategory { StoryId = s.Id, CategoryId = catKiemHiep.Id });
                }
                else if (s.Slug is "dau-pha-thuong-khung" or "vu-dong-can-khon" or "dai-chua-te" or "nguyen-ton" or "ban-long")
                {
                    context.StoryCategories.Add(new StoryCategory { StoryId = s.Id, CategoryId = catHuyenHuan.Id });
                }
                else if (s.Slug is "thon-phe-tinh-khong")
                {
                    context.StoryCategories.Add(new StoryCategory { StoryId = s.Id, CategoryId = catKhoaHuan.Id });
                    context.StoryCategories.Add(new StoryCategory { StoryId = s.Id, CategoryId = catHuyenHuan.Id });
                }
                else
                {
                    context.StoryCategories.Add(new StoryCategory { StoryId = s.Id, CategoryId = catTienHiep.Id });
                    context.StoryCategories.Add(new StoryCategory { StoryId = s.Id, CategoryId = catHuyenHuan.Id });
                }

                // Add sample chapters for each story
                for (int chNum = 1; chNum <= 3; chNum++)
                {
                    context.Chapters.Add(new Chapter
                    {
                        StoryId = s.Id,
                        ChapterNumber = chNum,
                        Title = $"Chương {chNum}: Khởi đầu hành trình phong vân {chNum}",
                        Slug = $"chuong-{chNum}-khoi-dau-hanh-trinh-phong-van-{chNum}",
                        PublishStatus = PublishStatus.Published,
                        ViewCount = 1000 + (chNum * 350),
                        PublishedAt = DateTimeOffset.UtcNow.AddDays(-30 + chNum),
                        Content = $@"<p>Gió đêm thổi qua rặng núi mờ sương, bóng đêm tĩnh mịch bao trùm toàn bộ không gian.</p>
<p>Nhân vật chính đứng trên đỉnh đồi, ánh mắt nhìn xa xăm về phương trời vô tận. Trong lòng cậu hiểu rõ, con đường phía trước đầy rẫy hiểm nguy và chông gai, nhưng một khi đã chọn bước đi, tuyệt đối không có đường lui.</p>
<p>Khí tức trong cơ thể bắt đầu vận chuyển theo quỹ đạo huyền bí, kinh mạch như sông lớn cuồn cuộn chảy xiết. Từng đạo linh quang lóe sáng quanh người, báo hiệu một bước ngoặt định mệnh sắp sửa khai mở...</p>
<p>Hành trình của một bậc chí tôn, bắt đầu từ chính giây phút này!</p>"
                    });
                }
            }

            await context.SaveChangesAsync();
            logger.LogInformation("Đã khởi tạo 10 truyện và các chương mẫu.");
        }

        // 6. Seed Sample Advertisements
        if (!await context.Advertisements.AnyAsync())
        {
            var ads = new List<Advertisement>
            {
                new()
                {
                    Name = "Popup Chào Mừng - Đọc Truyện VIP TruyenVietSub",
                    AdType = AdType.Image,
                    Placement = AdPlacement.Popup,
                    ImageUrl = "/images/ads/popup-welcome.svg",
                    DestinationUrl = "#",
                    StartAt = DateTimeOffset.UtcNow.AddDays(-1),
                    EndAt = DateTimeOffset.UtcNow.AddYears(1),
                    Priority = 10,
                    IsEnabled = true,
                    OpenInNewTab = false,
                    CreatedBy = "system-admin"
                },
                new()
                {
                    Name = "Banner Header - Đọc truyện bản quyền TruyenVietSub",
                    AdType = AdType.Image,
                    Placement = AdPlacement.Header,
                    ImageUrl = "/images/ads/banner-header.webp",
                    DestinationUrl = "#",
                    StartAt = DateTimeOffset.UtcNow.AddDays(-10),
                    EndAt = DateTimeOffset.UtcNow.AddMonths(3),
                    Priority = 10,
                    IsEnabled = true,
                    OpenInNewTab = false,
                    CreatedBy = "system-admin"
                },
                new()
                {
                    Name = "Quảng cáo Trang chủ Top - App TruyenVietSub Mobile",
                    AdType = AdType.Image,
                    Placement = AdPlacement.HomeTop,
                    ImageUrl = "/images/ads/banner-hometop.webp",
                    DestinationUrl = "#",
                    StartAt = DateTimeOffset.UtcNow.AddDays(-5),
                    EndAt = DateTimeOffset.UtcNow.AddMonths(6),
                    Priority = 8,
                    IsEnabled = true,
                    OpenInNewTab = false,
                    CreatedBy = "system-admin"
                },
                new()
                {
                    Name = "Quảng cáo Chi tiết truyện - Khuyến mãi nạp tích lũy",
                    AdType = AdType.SafeHtml,
                    Placement = AdPlacement.StoryDetail,
                    HtmlContent = "<div class='alert alert-warning p-3 text-center'><strong>Ưu đãi thành viên:</strong> Tặng 50% linh thạch khi mở rương huyền thoại trong tháng này!</div>",
                    DestinationUrl = "#",
                    StartAt = DateTimeOffset.UtcNow.AddDays(-1),
                    EndAt = DateTimeOffset.UtcNow.AddMonths(1),
                    Priority = 5,
                    IsEnabled = true,
                    OpenInNewTab = false,
                    CreatedBy = "system-admin"
                },
                new()
                {
                    Name = "Quảng cáo Giữa chương - Trải nghiệm đọc mượt mà",
                    AdType = AdType.Text,
                    Placement = AdPlacement.ChapterMiddle,
                    DestinationUrl = "#",
                    HtmlContent = "Tham gia Hội Viên VIP để đọc truyện không quảng cáo và mở khóa trước 10 chương!",
                    StartAt = DateTimeOffset.UtcNow.AddDays(-2),
                    EndAt = DateTimeOffset.UtcNow.AddMonths(2),
                    Priority = 7,
                    IsEnabled = true,
                    OpenInNewTab = true,
                    CreatedBy = "system-admin"
                }
            };

            await context.Advertisements.AddRangeAsync(ads);
            await context.SaveChangesAsync();
            logger.LogInformation("Đã khởi tạo các quảng cáo mẫu ở các vị trí chính (bao gồm ô quảng cáo Popup).");
        }
        else if (!await context.Advertisements.AnyAsync(a => a.Placement == AdPlacement.Popup))
        {
            await context.Advertisements.AddAsync(new Advertisement
            {
                Name = "Popup Chào Mừng - Đọc Truyện VIP TruyenVietSub",
                AdType = AdType.Image,
                Placement = AdPlacement.Popup,
                ImageUrl = "/images/ads/popup-welcome.svg",
                DestinationUrl = "#",
                StartAt = DateTimeOffset.UtcNow.AddDays(-1),
                EndAt = DateTimeOffset.UtcNow.AddYears(1),
                Priority = 10,
                IsEnabled = true,
                OpenInNewTab = false,
                CreatedBy = "system-admin"
            });
            await context.SaveChangesAsync();
            logger.LogInformation("Đã tự động bổ sung ô quảng cáo Popup Chào Mừng.");
        }

        // 7. Seed Sample Short Stories (Truyện ngắn)
        // Chỉ chạy khi khởi tạo dữ liệu ban đầu cho database hoàn toàn mới.
        // Tuyệt đối không tự động chèn lại các truyện mẫu nếu admin đã chủ động xóa hoặc quản lý nội dung.
        if (isInitialStorySeed)
        {
            await SeedShortStoriesAsync(context, adminId, logger);
        }
    }

    private static async Task SeedShortStoriesAsync(StoryHubDbContext context, string adminId, ILogger logger)
    {
        string creatorId = string.IsNullOrEmpty(adminId) ? "system-admin" : adminId;

        // Ensure "Truyện Ngắn" category
        var catTruyenNgan = await context.Categories.FirstOrDefaultAsync(c => c.Slug == "truyen-ngan");
        if (catTruyenNgan == null)
        {
            catTruyenNgan = new Category
            {
                Name = "Truyện Ngắn",
                Slug = "truyen-ngan",
                Description = "Những tác phẩm truyện ngắn đặc sắc, súc tích, mang ý nghĩa nhân văn sâu sắc.",
                IsActive = true
            };
            await context.Categories.AddAsync(catTruyenNgan);
            await context.SaveChangesAsync();
        }

        var catDoThi = await context.Categories.FirstOrDefaultAsync(c => c.Slug == "do-thi");
        var catKhoaHuan = await context.Categories.FirstOrDefaultAsync(c => c.Slug == "khoa-huan");
        var catHuyenHuan = await context.Categories.FirstOrDefaultAsync(c => c.Slug == "huyen-huan");
        var catNgonTinh = await context.Categories.FirstOrDefaultAsync(c => c.Slug == "ngon-tinh");

        // Ensure Authors
        var authorThachLam = await context.Authors.FirstOrDefaultAsync(a => a.Slug == "thach-lam");
        if (authorThachLam == null)
        {
            authorThachLam = new Author
            {
                Name = "Thạch Lam",
                Slug = "thach-lam",
                Biography = "Cây bút tiêu biểu của Tự Lực Văn Đoàn, nổi tiếng với văn phong nhẹ nhàng, tinh tế, đượm chất thơ và tình người ấm áp."
            };
            await context.Authors.AddAsync(authorThachLam);
        }

        var authorNamCao = await context.Authors.FirstOrDefaultAsync(a => a.Slug == "nam-cao");
        if (authorNamCao == null)
        {
            authorNamCao = new Author
            {
                Name = "Nam Cao",
                Slug = "nam-cao",
                Biography = "Nhà văn hiện thực xuất sắc của văn học Việt Nam thế kỷ 20, bậc thầy phân tích tâm lý nhân vật."
            };
            await context.Authors.AddAsync(authorNamCao);
        }

        var authorStoryHub = await context.Authors.FirstOrDefaultAsync(a => a.Slug == "truyenvietsub-authors" || a.Slug == "storyhub-authors");
        if (authorStoryHub == null)
        {
            authorStoryHub = new Author
            {
                Name = "TruyenVietSub Authors",
                Slug = "truyenvietsub-authors",
                Biography = "Nhóm tác giả sáng tác và biên tập viên độc quyền của nền tảng đọc truyện TruyenVietSub."
            };
            await context.Authors.AddAsync(authorStoryHub);
        }
        else
        {
            authorStoryHub.Name = "TruyenVietSub Authors";
            authorStoryHub.Biography = "Nhóm tác giả sáng tác và biên tập viên độc quyền của nền tảng đọc truyện TruyenVietSub.";
        }
        await context.SaveChangesAsync();

        // 1. Gió Đầu Mùa (Thạch Lam)
        if (!await context.Stories.AnyAsync(s => s.Slug == "gio-dau-mua"))
        {
            var story = new Story
            {
                Title = "Gió Đầu Mùa",
                Slug = "gio-dau-mua",
                AlternativeTitle = "Early Winter Wind",
                Description = "Kiệt tác truyện ngắn bất hủ của nhà văn Thạch Lam, khắc họa tình người ấm áp giữa những ngày đông lạnh giá của phố huyện nghèo. Chiếc áo bông cũ và sự sẻ chia chân thành của những đứa trẻ làm ấm lòng người đọc mọi thế hệ.",
                CoverImageUrl = "/images/covers/gio-dau-mua.webp",
                AuthorId = authorThachLam.Id,
                Status = StoryStatus.Completed,
                PublishStatus = PublishStatus.Published,
                IsFeatured = true,
                ViewCount = 45200,
                AverageRating = 4.95m,
                RatingCount = 88,
                CreatedBy = creatorId,
                PublishedAt = DateTimeOffset.UtcNow.AddMonths(-2)
            };
            await context.Stories.AddAsync(story);
            await context.SaveChangesAsync();

            context.StoryCategories.Add(new StoryCategory { StoryId = story.Id, CategoryId = catTruyenNgan.Id });
            if (catDoThi != null) context.StoryCategories.Add(new StoryCategory { StoryId = story.Id, CategoryId = catDoThi.Id });

            context.Chapters.AddRange(
                new Chapter
                {
                    StoryId = story.Id,
                    ChapterNumber = 1,
                    Title = "Tiết trời chuyển đông và ký ức chiếc áo bông",
                    Slug = "chuong-1-tiet-troi-chuyen-dong-va-ky-uc-chiec-ao-bong",
                    PublishStatus = PublishStatus.Published,
                    ViewCount = 15200,
                    PublishedAt = DateTimeOffset.UtcNow.AddMonths(-2),
                    Content = @"<p>Buổi sáng hôm nay, mùa đông đột nhiên đến, không báo trước. Vừa mới hôm qua, trời còn nắng ấm và hanh nồng, cái nắng của mùa thu ngọt ngào, thế mà chỉ qua một đêm mưa rào, sáng nay thức dậy đã thấy gió vi vu thổi qua khe cửa.</p>
<p>Sơn co ro trong chăn ấm, hé mắt nhìn ra ngoài sân. Trời xám xịt một màu u ám, gió bấc từng cơn thổi mạnh làm những chiếc lá bàng khô rụng lả tả, cuốn bay xào xạc trên mặt đất lát gạch rêu phong. Chị Lan đã dậy từ sớm, đang xúng xính trong chiếc áo len màu hạt dẻ.</p>
<p>Mẹ mở tráp đồng, lấy ra chiếc áo bông trần chỉ màu hoa đào đã hơi sờn ở nẹp cổ. Đó là chiếc áo của em Duyên, đứa em gái nhỏ xấu số đã mất năm ngoái. Nhìn chiếc áo, mắt mẹ thoáng rơm rớm lệ: <em>""Chiếc áo này ngày trước cái Duyên nó thích lắm, mỗi lần rét là nó đòi mặc cho bằng được...""</em></p>
<p>Sơn mặc áo ấm vào, cảm nhận được hơi ấm len lỏi khắp cơ thể. Cậu cùng chị Lan xin phép mẹ chạy ra ngoài chợ chơi cùng lũ trẻ con trong xóm nghèo.</p>"
                },
                new Chapter
                {
                    StoryId = story.Id,
                    ChapterNumber = 2,
                    Title = "Cuộc gặp gỡ nơi xóm chợ gió rét",
                    Slug = "chuong-2-cuoc-gap-go-noi-xom-cho-gio-ret",
                    PublishStatus = PublishStatus.Published,
                    ViewCount = 14800,
                    PublishedAt = DateTimeOffset.UtcNow.AddMonths(-2).AddDays(1),
                    Content = @"<p>Gió bấc càng về trưa càng thổi mạnh, buốt như kim châm vào da thịt. Khu chợ phố huyện vắng vẻ, những lều quán đóng cửa im ỉm dưới bầu trời xám xịt.</p>
<p>Ở góc chợ, lũ trẻ con xóm nghèo tụ tập bên đống rác khô để tìm chút hơi ấm. Chúng nó đều mặc những manh áo tơi tả, rách nát, da thịt tím tái vì lạnh. Thấy chị em Sơn đến trong những bộ quần áo ấm áp, sạch sẽ, lũ trẻ nhìn với ánh mắt vừa thèm thuồng, vừa e dè, giữ một khoảng cách kính cẩn.</p>
<p>Sơn nhìn thấy cái Hiên - con bé con chị dâu ở cạnh nhà. Hiên đang đứng co ro bên cột quán, chỉ mặc độc một manh áo rách tả tơi, hở cả lưng và tay. Thân hình nó gầy gò, môi thâm sì, hai hàm răng đập vào nhau cành cạch vì rét.</p>
<p>Sơn thấy động lòng trắc ẩn. Cậu lại gần hỏi khẽ: <em>""Sao mày không mặc áo ấm vào?""</em></p>
<p>Hiên cúi gằm mặt xuống đất, ngập ngừng đáp: <em>""Mẹ em chưa có tiền mua áo... áo cũ rách hết rồi.""</em></p>
<p>Sơn quay sang nhìn chị Lan. Trong đầu cậu bỗng nảy ra một ý nghĩ táo bạo nhưng tràn đầy tình thương: đem chiếc áo bông trần của em Duyên cho cái Hiên mượn.</p>"
                },
                new Chapter
                {
                    StoryId = story.Id,
                    ChapterNumber = 3,
                    Title = "Tấm lòng của mẹ và hơi ấm tình người",
                    Slug = "chuong-3-tam-long-cua-me-va-hoi-am-tinh-nguoi",
                    PublishStatus = PublishStatus.Published,
                    ViewCount = 15200,
                    PublishedAt = DateTimeOffset.UtcNow.AddMonths(-2).AddDays(2),
                    Content = @"<p>Chị Lan nghe Sơn nói thì đồng ý ngay. Hai chị em tất tả chạy về nhà, lén mẹ lấy chiếc áo bông ấm trong ngăn tủ rồi vội vã mang ra cho cái Hiên.</p>
<p>Cầm chiếc áo bông dày dặn trong tay, cái Hiên rưng rưng nước mắt, miệng cười mà hai hàng mi ướt đẫm. Nó vội mặc áo vào, hơi ấm lập tức xua tan đi cơn giá buốt của mùa đông đầu mùa.</p>
<p>Thế nhưng khi về đến nhà, hai chị em lại lo sợ bị mẹ mắng vì đã tự tiện đem cho kỷ vật của em Duyên. Chiều hôm ấy, mẹ cái Hiên dắt con bé sang trả lại chiếc áo cho mẹ Sơn, vừa cúi đầu vừa cảm ơn rối rít: <em>""Bác tha lỗi cho, hai cháu nó thương cái Hiên rét mướt nên đem áo sang cho mượn, chứ tôi nghèo hèn đâu dám nhận của quý.""</em></p>
<p>Mẹ Sơn mỉm cười hiền từ. Mẹ không mắng hai con, trái lại còn mở túi lấy ra mấy hào bạc dúi vào tay mẹ con cái Hiên để may áo mới. Trong căn nhà nhỏ ấm áp mùi khói trầm, gió mùa đông bắc ngoài kia dường như bớt lạnh hơn rất nhiều bởi ngọn lửa của tình người sưởi ấm.</p>"
                }
            );
            await context.SaveChangesAsync();
            logger.LogInformation("Đã thêm truyện ngắn: Gió Đầu Mùa");
        }

        // 2. Chí Phèo (Nam Cao)
        if (!await context.Stories.AnyAsync(s => s.Slug == "chi-pheo"))
        {
            var story = new Story
            {
                Title = "Chí Phèo",
                Slug = "chi-pheo",
                AlternativeTitle = "The Story of Chi Pheo",
                Description = "Kiệt tác truyện ngắn hiện thực phê phán đỉnh cao của văn học Việt Nam. Bi kịch của người nông dân lương thiện bị xã hội phong kiến tha hóa đến tột cùng và khát vọng hoàn lương đau đớn qua bát cháo hành của Thị Nở.",
                CoverImageUrl = "/images/covers/chi-pheo.webp",
                AuthorId = authorNamCao.Id,
                Status = StoryStatus.Completed,
                PublishStatus = PublishStatus.Published,
                IsFeatured = true,
                ViewCount = 68400,
                AverageRating = 4.98m,
                RatingCount = 145,
                CreatedBy = creatorId,
                PublishedAt = DateTimeOffset.UtcNow.AddMonths(-3)
            };
            await context.Stories.AddAsync(story);
            await context.SaveChangesAsync();

            context.StoryCategories.Add(new StoryCategory { StoryId = story.Id, CategoryId = catTruyenNgan.Id });

            context.Chapters.AddRange(
                new Chapter
                {
                    StoryId = story.Id,
                    ChapterNumber = 1,
                    Title = "Tiếng chửi của Chí và con đường tha hóa",
                    Slug = "chuong-1-tieng-chui-cua-chi-va-con-duong-tha-hoa",
                    PublishStatus = PublishStatus.Published,
                    ViewCount = 23000,
                    PublishedAt = DateTimeOffset.UtcNow.AddMonths(-3),
                    Content = @"<p>Hắn vừa đi vừa chửi. Bao giờ cũng thế, cứ rượu xong là hắn chửi. Bắt đầu hắn chửi trời. Có hề gì? Trời có của riêng nhà nào? Rồi hắn chửi đời. Thế cũng chẳng sao: đời là tất cả nhưng chẳng là ai.</p>
<p>Tức mình hắn chửi ngay tất cả làng Vũ Đại. Nhưng cả làng Vũ Đại ai cũng nhủ thầm: <em>""Chắc nó trừ mình ra!""</em> Không ai lên tiếng cả. Tức thật! Hắn lại chửi đứa chết mẹ nào đẻ ra thân hắn cho hắn khổ thế này!</p>
<p>Chí Phèo vốn là một đứa trẻ mồ côi bị bỏ rơi nơi lò gạch cũ, lớn lên hiền lành như cục đất, đi làm canh điền cho nhà Bá Kiến. Nhưng lòng ghen tuông vô cớ của cụ Bá đã đẩy người thanh niên hiền lành vào chốn lao tù tăm tối.</p>
<p>Bảy tám năm sau, Chí trở về làng với diện mạo hoàn toàn biến đổi: cái đầu trọc lốc, răng cạo trắng hớn, mặt đầy sẹo và hai mắt gườm gườm. Hắn đã biến thành một con quỷ dữ của làng Vũ Đại, sống bằng rượu, chém thuê và rạch mặt ăn vạ.</p>"
                },
                new Chapter
                {
                    StoryId = story.Id,
                    ChapterNumber = 2,
                    Title = "Bát cháo hành của Thị Nở và thức tỉnh lương tri",
                    Slug = "chuong-2-bat-chao-hanh-cua-thi-no-va-thuc-tinh-luong-tri",
                    PublishStatus = PublishStatus.Published,
                    ViewCount = 22800,
                    PublishedAt = DateTimeOffset.UtcNow.AddMonths(-3).AddDays(1),
                    Content = @"<p>Một đêm trăng sáng vằng vặc trên bãi sông, Chí Phèo uống rượu say khướt rồi loạng choạng bước về túp lều bên bờ sông. Nơi đó, hắn tình cờ gặp Thị Nở đang ngủ say bên bụi chuối rậm rạp.</p>
<p>Sáng hôm sau thức dậy, Chí thấy đầu đau nhức và miệng đắng ngắt. Hắn lắng nghe được những âm thanh bình dị của cuộc đời mà bấy lâu nay hắn đã lãng quên: tiếng chim hót ngoài bờ sông, tiếng cười nói của những người đi chợ về, tiếng anh thuyền chài gõ mái chèo đuổi cá...</p>
<p>Chí bàng hoàng nhận ra mình đã già, cô độc và đáng thương. Đúng lúc ấy, Thị Nở bước vào, tay bưng một bát cháo hành còn bốc khói nghi ngút.</p>
<p>Hương cháo hành thơm phức ngào ngạt. Đó là lần đầu tiên trong đời Chí được một người đàn bà chăm sóc mà không phải đi cướp đoạt hay dọa dẫm. Vị cháo nóng hổi trôi xuống cổ họng, Chí thấy mắt mình ươn ướt. Hắn khao khát được trở lại làm người lương thiện, sống một cuộc đời bình yên giản dị bên Thị Nở.</p>"
                },
                new Chapter
                {
                    StoryId = story.Id,
                    ChapterNumber = 3,
                    Title = "Ai cho tao lương thiện",
                    Slug = "chuong-3-ai-cho-tao-luong-thien",
                    PublishStatus = PublishStatus.Published,
                    ViewCount = 22600,
                    PublishedAt = DateTimeOffset.UtcNow.AddMonths(-3).AddDays(2),
                    Content = @"<p>Nhưng cánh cửa hoàn lương vừa mới hé mở đã tàn nhẫn khép lại. Bà cô của Thị Nở kiên quyết ngăn cấm: <em>""Ai lại đi lấy một thằng không cha không mẹ, chỉ biết rạch mặt ăn vạ!""</em></p>
<p>Thị Nở chạy sang túp lều trút tất cả những lời cay độc vào mặt Chí rồi quay lưng bỏ đi. Chí Phèo như rơi xuống vực thẳm tuyệt vọng. Hắn lại lôi chai rượu ra uống, nhưng càng uống lại càng tỉnh, càng tỉnh lại càng ngửi thấy thoang thoảng mùi cháo hành ấm áp.</p>
<p>Uất hận và đau đớn tột cùng, Chí giắt con dao nhọn vào cạp quần, bước thẳng tới nhà Bá Kiến. Trước mặt tên địa chủ gian xảo, Chí gầm lên lời thét bi tráng xé toạc màn đêm của xã hội cũ:</p>
<p><strong>""Tao muốn làm người lương thiện! Ai cho tao lương thiện? Làm thế nào cho mất được những vết mảnh chai trên mặt này?...""</strong></p>
<p>Nhát dao oan nghiệt đã kết thúc số phận của tên cáo già Bá Kiến và cũng chấm dứt kiếp đời cay đắng của Chí Phèo, để lại nỗi ám ảnh khôn nguôi về quyền được làm người.</p>"
                }
            );
            await context.SaveChangesAsync();
            logger.LogInformation("Đã thêm truyện ngắn: Chí Phèo");
        }

        // 3. Chuyến Tàu Đêm Không Dừng Lại (TruyenVietSub Authors)
        if (!await context.Stories.AnyAsync(s => s.Slug == "chuyen-tau-dem-khong-dung-lai"))
        {
            var story = new Story
            {
                Title = "Chuyến Tàu Đêm Không Dừng Lại",
                Slug = "chuyen-tau-dem-khong-dung-lai",
                AlternativeTitle = "The Nonstop Midnight Express",
                Description = "Một truyện ngắn giả tưởng kỳ bí và sâu sắc về chuyến tàu số 04 xuất hiện tại sân ga ngầm lúc 2 giờ sáng. Chuyến tàu chở những hành khách đi tìm lại những ký ức trân quý nhất mà họ đã từng vô tình đánh mất trong dòng chảy tất bật của cuộc đời.",
                CoverImageUrl = "/images/covers/chuyen-tau-dem-khong-dung-lai.webp",
                AuthorId = authorStoryHub.Id,
                Status = StoryStatus.Completed,
                PublishStatus = PublishStatus.Published,
                IsFeatured = true,
                ViewCount = 31200,
                AverageRating = 4.88m,
                RatingCount = 62,
                CreatedBy = creatorId,
                PublishedAt = DateTimeOffset.UtcNow.AddMonths(-1)
            };
            await context.Stories.AddAsync(story);
            await context.SaveChangesAsync();

            context.StoryCategories.Add(new StoryCategory { StoryId = story.Id, CategoryId = catTruyenNgan.Id });
            if (catKhoaHuan != null) context.StoryCategories.Add(new StoryCategory { StoryId = story.Id, CategoryId = catKhoaHuan.Id });
            if (catHuyenHuan != null) context.StoryCategories.Add(new StoryCategory { StoryId = story.Id, CategoryId = catHuyenHuan.Id });

            context.Chapters.AddRange(
                new Chapter
                {
                    StoryId = story.Id,
                    ChapterNumber = 1,
                    Title = "Sân ga không tồn tại trên bản đồ",
                    Slug = "chuong-1-san-ga-khong-ton-tai-tren-ban-do",
                    PublishStatus = PublishStatus.Published,
                    ViewCount = 10500,
                    PublishedAt = DateTimeOffset.UtcNow.AddMonths(-1),
                    Content = @"<p>Đồng hồ điểm đúng 2 giờ sáng. Hà Nội chìm trong màn sương mù ẩm ướt và giá lạnh của đêm cuối đông.</p>
<p>Minh - một lập trình viên kiệt sức sau chuỗi ngày tăng ca triền miên - bước xuống bậc thang dẫn vào ga tàu điện ngầm. Mọi cửa ngõ bình thường đều đã khóa chặt, nhưng ở góc tối chân cầu thang lại hiện ra một hành lang dài hun hút, phát ra thứ ánh sáng vàng dịu kỳ lạ.</p>
<p>Cuối hành lang là một sân ga cổ kính lát gạch hoa cổ điển, không hề có biển chỉ dẫn hay camera an ninh. Trên đường ray rỉ sét, một đoàn tàu hơi nước thân sơn màu xanh thẫm đang lặng lẽ phả khói trắng, hoàn toàn không phát ra một tiếng động cơ nào.</p>
<p>Cửa toa xe số 04 trượt mở êm ái. Một giọng nói trầm ấm vang lên bên tai cậu: <em>""Hành khách cuối cùng, mời lên tàu trước khi thời gian đảo chiều.""</em></p>"
                },
                new Chapter
                {
                    StoryId = story.Id,
                    ChapterNumber = 2,
                    Title = "Người soát vé mang chiếc đồng hồ cát",
                    Slug = "chuong-2-nguoi-soat-ve-mang-chiec-dong-ho-cat",
                    PublishStatus = PublishStatus.Published,
                    ViewCount = 10400,
                    PublishedAt = DateTimeOffset.UtcNow.AddMonths(-1).AddDays(1),
                    Content = @"<p>Bên trong toa tàu ấm cúng lạ thường, thoang thoảng hương cà phê và giấy báo cũ. Nhưng điều kỳ lạ là ngoài cửa sổ, cảnh vật không phải là những đường ray ngầm quen thuộc, mà là những thước phim cuộc đời đang chạy lùi với tốc độ chóng mặt.</p>
<p>Một người đàn ông mặc âu phục cổ điển, tay cầm một chiếc đồng hồ cát bằng đồng sáng bóng bước tới. Cát trong đồng hồ đang chảy ngược từ dưới lên trên.</p>
<p><em>""Vé của quý khách không mua bằng tiền,""</em> người soát vé mỉm cười, mắt ánh lên vẻ thông thái. <em>""Quý khách chỉ có thể thanh toán bằng một mẩu ký ức chân thật nhất mà quý khách từng mong muốn giữ lại nhất nhưng đã lãng quên.""</em></p>
<p>Minh sững sờ. Cậu nhớ lại những năm tháng thanh xuân, nhớ về lời hứa đưa bố mẹ đi du lịch biển đã bị hoãn lại suốt mười năm qua bởi những guồng quay công việc không hồi kết.</p>"
                },
                new Chapter
                {
                    StoryId = story.Id,
                    ChapterNumber = 3,
                    Title = "Chiếc vé khứ hồi về miền ký ức",
                    Slug = "chuong-3-chiec-ve-khu-hoi-ve-mien-ky-uc",
                    PublishStatus = PublishStatus.Published,
                    ViewCount = 10300,
                    PublishedAt = DateTimeOffset.UtcNow.AddMonths(-1).AddDays(2),
                    Content = @"<p>Khi giọt nước mắt hối hận rơi xuống lòng bàn tay, người soát vé khẽ gật đầu. Một chiếc vé bằng giấy da mạ vàng hiện ra, trên đó ghi rõ điểm đến: <strong>""Hiện Tại""</strong>.</p>
<p>Đoàn tàu hú một hồi còi ngân vang trong cõi hư vô rồi dần tan biến vào làn sương sớm. Khi Minh bừng tỉnh, cậu nhận ra mình đang ngồi trên băng ghế đá công viên ven hồ Hoàn Kiếm, ánh bình minh ban mai dịu dàng rọi qua kẽ lá rực rỡ.</p>
<p>Chuông điện thoại reo vang. Là mẹ gọi hỏi thăm con trai đã ăn sáng chưa. Minh mỉm cười, giọng run run nghẹn ngào nhưng ngập tràn hạnh phúc: <em>""Con đang về nhà đây mẹ ơi, cuối tuần này cả nhà mình cùng đi chơi biển nhé!""</em></p>
<p>Chuyến tàu đêm có thể không bao giờ dừng lại, nhưng nó đã kịp trả lại cho một con người điều quý giá nhất: tình yêu thương và sự hiện diện của gia đình.</p>"
                }
            );
            await context.SaveChangesAsync();
            logger.LogInformation("Đã thêm truyện ngắn: Chuyến Tàu Đêm Không Dừng Lại");
        }

        // 4. Quán Trà Lúc Nửa Đêm (TruyenVietSub Authors)
        if (!await context.Stories.AnyAsync(s => s.Slug == "quan-tra-luc-nua-dem"))
        {
            var story = new Story
            {
                Title = "Quán Trà Lúc Nửa Đêm",
                Slug = "quan-tra-luc-nua-dem",
                AlternativeTitle = "Midnight Tea Sanctuary",
                Description = "Nằm sâu trong một con ngõ nhỏ của phố cổ Hà Nội, quán trà chỉ mở cửa từ 12 giờ đêm đến 4 giờ sáng. Nơi mỗi vị khách ghé qua đều để lại một mẩu tâm sự chất chứa và mang về một tách trà hoa nhài xoa dịu vết thương lòng.",
                CoverImageUrl = "/images/covers/quan-tra-luc-nua-dem.webp",
                AuthorId = authorStoryHub.Id,
                Status = StoryStatus.Completed,
                PublishStatus = PublishStatus.Published,
                IsFeatured = true,
                ViewCount = 28900,
                AverageRating = 4.90m,
                RatingCount = 52,
                CreatedBy = creatorId,
                PublishedAt = DateTimeOffset.UtcNow.AddDays(-20)
            };
            await context.Stories.AddAsync(story);
            await context.SaveChangesAsync();

            context.StoryCategories.Add(new StoryCategory { StoryId = story.Id, CategoryId = catTruyenNgan.Id });
            if (catDoThi != null) context.StoryCategories.Add(new StoryCategory { StoryId = story.Id, CategoryId = catDoThi.Id });
            if (catNgonTinh != null) context.StoryCategories.Add(new StoryCategory { StoryId = story.Id, CategoryId = catNgonTinh.Id });

            context.Chapters.AddRange(
                new Chapter
                {
                    StoryId = story.Id,
                    ChapterNumber = 1,
                    Title = "Mùi hương hoa nhài trong ngõ vắng",
                    Slug = "chuong-1-mui-huong-hoa-nhai-trong-ngo-vang",
                    PublishStatus = PublishStatus.Published,
                    ViewCount = 9800,
                    PublishedAt = DateTimeOffset.UtcNow.AddDays(-20),
                    Content = @"<p>Ở số nhà 42 ngõ Tạm Thương, có một quán trà không biển hiệu, chỉ treo một ngọn đèn lồng bằng tre màu cam nhạt trước hiên. Quán chỉ mở cửa khi cả thành phố đã say ngủ, từ nửa đêm cho đến khi chuyến tàu sớm đầu ngày chạy qua đầu phố.</p>
<p>Chủ quán là một ông lão tóc bạc phơ tên Định. Ông luôn mặc áo chàm giản dị, đôi tay thoăn thoắt pha những ấm trà bằng nước giếng ngọc và hoa nhài hái từ vườn sớm.</p>
<p>Quán không có thực đơn cố định. Mỗi vị khách bước vào, ông chỉ nhìn vào ánh mắt họ rồi chọn một loại trà thích hợp. Người mang tâm sự bồn chồn uống trà sen dịu mát; người chất chứa nỗi buồn được thưởng tách trà gừng mật ong đằm thắm.</p>
<p>Đêm nay, cánh cửa gỗ khẽ cót két mở. Một cô gái trẻ ướt đẫm nước mưa, đôi mắt đỏ hoe bước vào...</p>"
                },
                new Chapter
                {
                    StoryId = story.Id,
                    ChapterNumber = 2,
                    Title = "Lời xin lỗi muộn màng mười năm",
                    Slug = "chuong-2-loi-xin-loi-muon-mang-muoi-nam",
                    PublishStatus = PublishStatus.Published,
                    ViewCount = 9600,
                    PublishedAt = DateTimeOffset.UtcNow.AddDays(-19),
                    Content = @"<p>Cô gái ngồi xuống chiếc bàn gỗ mộc, hai vai vẫn còn run lên vì lạnh. Ông Định không hỏi gì, chỉ nhẹ nhàng đặt trước mặt cô một chén trà hoa nhài khói bốc nghi ngút và một chiếc đĩa gốm đựng mứt gừng cay nồng.</p>
<p><em>""Uống ngụm trà ấm đi cháu, người ướt lạnh dễ ốm lắm,""</em> ông Định ôn tồn nói.</p>
<p>Hơi ấm từ chén trà lan tỏa vào lòng bàn tay. Cô gái - tên Hà - rốt cuộc không kìm được tiếng khóc nghẹn ngào. Cô kể về mối tình đầu dang dở mười năm trước, về sự bồng bột của tuổi trẻ và một lời xin lỗi mà suốt một thập kỷ qua cô chưa từng có đủ dũng khí để nói ra.</p>
<p>Hôm nay, cô tình cờ gặp lại người ấy trên phố, nhưng người ấy đã có gia đình êm ấm. Nỗi nuối tiếc và mặc cảm tội lỗi như tảng đá đè nặng ngực cô bấy lâu nay bỗng vỡ òa.</p>"
                },
                new Chapter
                {
                    StoryId = story.Id,
                    ChapterNumber = 3,
                    Title = "Ánh bình minh sau tách trà ấm",
                    Slug = "chuong-3-anh-binh-minh-sau-tach-tra-am",
                    PublishStatus = PublishStatus.Published,
                    ViewCount = 9500,
                    PublishedAt = DateTimeOffset.UtcNow.AddDays(-18),
                    Content = @"<p>Ông Định lắng nghe chăm chú, thỉnh thoảng châm thêm nước sôi vào ấm trà. Khi Hà dứt lời, ông khẽ đẩy chén trà thứ hai về phía cô.</p>
<p><em>""Cháu à, lá trà khi ở trên cành xanh tươi, phải trải qua bao lần phơi nắng, sao lửa và hãm trong nước sôi bỏng rát mới tỏa ra được hương thơm thanh khiết nhất. Con người cũng vậy, những vấp ngã và tiếc nuối của quá khứ không phải để dằn vặt mình suốt đời, mà để dạy ta biết trân trọng những điều tốt đẹp đang ở trước mắt.""</em></p>
<p>Hà nhấp ngụm trà. Vị chát nhẹ đầu lưỡi nhanh chóng tan đi, nhường chỗ cho vị ngọt hậu sâu lắng lan tỏa khắp vòm họng. Lòng cô bỗng nhẹ bẫng như trút được ngàn cân đá tảng.</p>
<p>Tiếng chuông xe điện sớm ngoài phố leng keng báo hiệu trời đã rạng sáng. Ánh nắng đầu tiên của ngày mới chiếu qua khung cửa gỗ, rọi lên chén trà trong veo. Hà đứng dậy cúi đầu cảm ơn ông lão, bước ra phố với nụ cười thanh thản và một trái tim đã được chữa lành trọn vẹn.</p>"
                }
            );
            await context.SaveChangesAsync();
            logger.LogInformation("Đã thêm truyện ngắn: Quán Trà Lúc Nửa Đêm");
        }
    }
}
