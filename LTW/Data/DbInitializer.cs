using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using LTW.Models;
using System.Text.Json;

namespace LTW.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            // Ensure Database schema is created
            await context.Database.EnsureCreatedAsync();

            // 1. Seed Roles
            string[] roles = { "Admin", "Teacher", "Student" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 2. Seed Admin
            var adminEmail = "admin@lumora.edu.vn";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = "admin",
                    Email = adminEmail,
                    FullName = "Hệ Thống Quản Trị Viên (Admin)",
                    RoleName = "Admin",
                    EmailConfirmed = true,
                    IsActive = true,
                    AvatarUrl = "https://images.unsplash.com/photo-1534528741775-53994a69daeb?auto=format&fit=crop&w=250&q=80",
                    Bio = "Quản trị viên trưởng nền tảng học trực tuyến LUMORA English Center."
                };
                var result = await userManager.CreateAsync(adminUser, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // 3. Seed Teachers
            var teacher1Email = "teacher.sarah@lumora.edu.vn";
            var teacher1 = await userManager.FindByEmailAsync(teacher1Email);
            if (teacher1 == null)
            {
                teacher1 = new ApplicationUser
                {
                    UserName = "sarah.jenkins",
                    Email = teacher1Email,
                    FullName = "Ms. Sarah Jenkins (IELTS 8.5)",
                    RoleName = "Teacher",
                    EmailConfirmed = true,
                    IsActive = true,
                    AvatarUrl = "https://images.unsplash.com/photo-1573496359142-b8d87734a5a2?auto=format&fit=crop&w=250&q=80",
                    Bio = "Cựu giám khảo chấm thi British Council, 8 năm kinh nghiệm giảng dạy Writing & Speaking chuyên sâu."
                };
                var result = await userManager.CreateAsync(teacher1, "Teacher@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(teacher1, "Teacher");
                }
            }

            var teacher2Email = "teacher.james@lumora.edu.vn";
            var teacher2 = await userManager.FindByEmailAsync(teacher2Email);
            if (teacher2 == null)
            {
                teacher2 = new ApplicationUser
                {
                    UserName = "james.watson",
                    Email = teacher2Email,
                    FullName = "Mr. James Watson (MA TESOL)",
                    RoleName = "Teacher",
                    EmailConfirmed = true,
                    IsActive = true,
                    AvatarUrl = "https://images.unsplash.com/photo-1560250097-0b93528c311a?auto=format&fit=crop&w=250&q=80",
                    Bio = "Thạc sĩ Giảng dạy Ngôn ngữ Anh Đại học Cambridge, chuyên gia luyện phát âm và phản xạ giao tiếp tự nhiên."
                };
                var result = await userManager.CreateAsync(teacher2, "Teacher@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(teacher2, "Teacher");
                }
            }

            // Teacher 3: Dr. David Miller
            var teacher3Email = "teacher.david@lumora.edu.vn";
            var teacher3 = await userManager.FindByEmailAsync(teacher3Email);
            if (teacher3 == null)
            {
                teacher3 = new ApplicationUser
                {
                    UserName = "david.miller",
                    Email = teacher3Email,
                    FullName = "Dr. David Miller (IELTS 9.0)",
                    RoleName = "Teacher",
                    EmailConfirmed = true,
                    IsActive = true,
                    AvatarUrl = "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?auto=format&fit=crop&w=250&q=80",
                    Bio = "Tiến sĩ Ngôn ngữ học ứng dụng với 12 năm kinh nghiệm đào tạo học viên đạt chuẩn học thuật và du học quốc tế."
                };
                var result = await userManager.CreateAsync(teacher3, "Teacher@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(teacher3, "Teacher");
                }
            }

            // Teacher 4: Ms. Emma Richardson
            var teacher4Email = "teacher.emma@lumora.edu.vn";
            var teacher4 = await userManager.FindByEmailAsync(teacher4Email);
            if (teacher4 == null)
            {
                teacher4 = new ApplicationUser
                {
                    UserName = "emma.richardson",
                    Email = teacher4Email,
                    FullName = "Ms. Emma Richardson (CELTA)",
                    RoleName = "Teacher",
                    EmailConfirmed = true,
                    IsActive = true,
                    AvatarUrl = "https://images.unsplash.com/photo-1580489944761-15a19d654956?auto=format&fit=crop&w=250&q=80",
                    Bio = "Chuyên gia huấn luyện phát âm IPA chuẩn Anh - Mỹ và chiến thuật phản xạ giao tiếp trôi chảy tự nhiên."
                };
                var result = await userManager.CreateAsync(teacher4, "Teacher@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(teacher4, "Teacher");
                }
            }

            // Teacher 5: Mr. Daniel Evans
            var teacher5Email = "teacher.daniel@lumora.edu.vn";
            var teacher5 = await userManager.FindByEmailAsync(teacher5Email);
            if (teacher5 == null)
            {
                teacher5 = new ApplicationUser
                {
                    UserName = "daniel.evans",
                    Email = teacher5Email,
                    FullName = "Mr. Daniel Evans (IELTS 8.5)",
                    RoleName = "Teacher",
                    EmailConfirmed = true,
                    IsActive = true,
                    AvatarUrl = "https://images.unsplash.com/photo-1500648767791-00dcc994a43e?auto=format&fit=crop&w=250&q=80",
                    Bio = "Chuyên sâu phân tích cấu trúc bài viết Task 1 & Task 2, phát triển tư duy mạch lạc và vốn từ vựng học thuật C1-C2."
                };
                var result = await userManager.CreateAsync(teacher5, "Teacher@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(teacher5, "Teacher");
                }
            }

            // Teacher 6: Ms. Olivia Taylor
            var teacher6Email = "teacher.olivia@lumora.edu.vn";
            var teacher6 = await userManager.FindByEmailAsync(teacher6Email);
            if (teacher6 == null)
            {
                teacher6 = new ApplicationUser
                {
                    UserName = "olivia.taylor",
                    Email = teacher6Email,
                    FullName = "Ms. Olivia Taylor (IELTS 8.5)",
                    RoleName = "Teacher",
                    EmailConfirmed = true,
                    IsActive = true,
                    AvatarUrl = "https://images.unsplash.com/photo-1544005313-94ddf0286df2?auto=format&fit=crop&w=250&q=80",
                    Bio = "Kỹ thuật Skimming & Scanning đột phá, bẻ khóa các bẫy đề thi Cambridge và cải thiện tốc độ xử lý bài đọc."
                };
                var result = await userManager.CreateAsync(teacher6, "Teacher@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(teacher6, "Teacher");
                }
            }

            // Teacher 7: Mr. Robert Clark
            var teacher7Email = "teacher.robert@lumora.edu.vn";
            var teacher7 = await userManager.FindByEmailAsync(teacher7Email);
            if (teacher7 == null)
            {
                teacher7 = new ApplicationUser
                {
                    UserName = "robert.clark",
                    Email = teacher7Email,
                    FullName = "Mr. Robert Clark (IELTS 9.0)",
                    RoleName = "Teacher",
                    EmailConfirmed = true,
                    IsActive = true,
                    AvatarUrl = "https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?auto=format&fit=crop&w=250&q=80",
                    Bio = "Giám đốc học thuật LUMORA, cố vấn phương pháp học tích hợp cá nhân hóa và chuẩn hóa đề thi mô phỏng CDI."
                };
                var result = await userManager.CreateAsync(teacher7, "Teacher@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(teacher7, "Teacher");
                }
            }

            // 4. Seed Student
            var studentEmail = "student@lumora.edu.vn";
            var demoStudent = await userManager.FindByEmailAsync(studentEmail);
            if (demoStudent == null)
            {
                demoStudent = new ApplicationUser
                {
                    UserName = "student",
                    Email = studentEmail,
                    FullName = "Nguyễn Hoàng Minh",
                    RoleName = "Student",
                    EmailConfirmed = true,
                    IsActive = true,
                    AvatarUrl = "https://images.unsplash.com/photo-1539571696357-5a69c17a67c6?auto=format&fit=crop&w=250&q=80",
                    Bio = "Học viên mục tiêu IELTS 7.5 phục vụ du học Anh Quốc."
                };
                var result = await userManager.CreateAsync(demoStudent, "Student@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(demoStudent, "Student");
                }
            }

            // 5. Seed Courses
            if (!await context.Courses.AnyAsync())
            {
                var course1 = new Course
                {
                    Title = "IELTS Masterclass 7.5+ Toàn Diện (4 Kỹ Năng)",
                    ShortDescription = "Lộ trình bứt phá từ 6.0 lên 7.5+ cùng cựu giám khảo chấm thi British Council.",
                    Description = "Khóa học cao cấp được thiết kế chuẩn cấu trúc bài thi IELTS trên máy tính. Khóa học tập trung rèn luyện tư duy phản biện, kỹ năng xử lý dạng bài khó trong Reading & Listening, chiến lược viết bài Writing Task 1 & Task 2 đạt chuẩn Lexical Resource & Grammatical Range, và phương pháp trả lời Speaking mượt mà, tự nhiên.",
                    Thumbnail = "https://images.unsplash.com/photo-1523240795612-9a054b0db644?auto=format&fit=crop&w=800&q=80",
                    Level = "IELTS 7.0+",
                    Category = "IELTS",
                    Price = 2450000,
                    OriginalPrice = 3500000,
                    Duration = "16 tuần",
                    TeacherId = teacher1?.Id,
                    IsPublished = true,
                    IsFeatured = true,
                    CreatedAt = DateTime.Now.AddDays(-30)
                };

                var course2 = new Course
                {
                    Title = "IELTS Foundation 5.0 - 6.5 Bứt Phá Mục Tiêu",
                    ShortDescription = "Xây vững gốc rễ ngữ pháp, từ vựng học thuật và phản xạ 4 kỹ năng cơ bản.",
                    Description = "Khóa học xây dựng nền tảng vững chắc cho người học mất gốc hoặc đang ở mức 4.5 - 5.0 muốn đạt 6.5. Cung cấp phương pháp học Skimming & Scanning, nghe hiểu từ khóa, viết câu phức và diễn đạt quan điểm lưu loát.",
                    Thumbnail = "https://images.unsplash.com/photo-1434030216411-0b793f4b4173?auto=format&fit=crop&w=800&q=80",
                    Level = "Intermediate",
                    Category = "IELTS",
                    Price = 1890000,
                    OriginalPrice = 2700000,
                    Duration = "12 tuần",
                    TeacherId = teacher1?.Id,
                    IsPublished = true,
                    IsFeatured = true,
                    CreatedAt = DateTime.Now.AddDays(-25)
                };

                var course3 = new Course
                {
                    Title = "Tiếng Anh Giao Tiếp Thực Chiến & Phản Xạ 1-1",
                    ShortDescription = "Tự tin trò chuyện, đàm phán và thuyết trình tiếng Anh lưu loát trong môi trường quốc tế.",
                    Description = "Tập trung 100% vào ngữ điệu, phát âm chuẩn Mỹ và phản xạ tương tác nhanh mà không cần dịch nhẩm trong đầu. Phù hợp cho người đi làm và sinh viên chuẩn bị phỏng vấn.",
                    Thumbnail = "https://images.unsplash.com/photo-1577495508048-b635879837f1?auto=format&fit=crop&w=800&q=80",
                    Level = "Beginner",
                    Category = "Communication",
                    Price = 1200000,
                    OriginalPrice = 1800000,
                    Duration = "8 tuần",
                    TeacherId = teacher2?.Id,
                    IsPublished = true,
                    IsFeatured = true,
                    CreatedAt = DateTime.Now.AddDays(-20)
                };

                var course4 = new Course
                {
                    Title = "Luyện Phát Âm Chuẩn IPA & Speaking Fluency",
                    ShortDescription = "Làm chủ 44 âm IPA, nối âm, nuốt âm, trọng âm câu và ngữ điệu tự nhiên như người bản xứ.",
                    Description = "Khóa học chuyên sâu chỉnh âm từ gốc, sửa tật nói ngọng, thiếu ending sounds, giúp bạn tự tin đạt điểm Pronunciation 8.0 trong bài thi IELTS Speaking.",
                    Thumbnail = "https://images.unsplash.com/photo-1516321318423-f06f85e504b3?auto=format&fit=crop&w=800&q=80",
                    Level = "All Levels",
                    Category = "Speaking",
                    Price = 950000,
                    OriginalPrice = 1500000,
                    Duration = "6 tuần",
                    TeacherId = teacher2?.Id,
                    IsPublished = true,
                    IsFeatured = false,
                    CreatedAt = DateTime.Now.AddDays(-15)
                };

                var course5 = new Course
                {
                    Title = "Ngữ Pháp Chuyên Sâu & Viết Học Thuật Writing 7.0+",
                    ShortDescription = "Làm chủ các cấu trúc câu phức, câu điều kiện hỗn hợp, phân từ và mệnh đề quan hệ rút gọn.",
                    Description = "Giải pháp triệt để cho thí sinh kẹt ở Band 5.5 - 6.0 Writing do lỗi diễn đạt lặp từ, ngữ pháp đơn điệu và thiếu liên kết mạch lạc (Coherence & Cohesion).",
                    Thumbnail = "https://images.unsplash.com/photo-1455390582262-044cdead277a?auto=format&fit=crop&w=800&q=80",
                    Level = "Advanced",
                    Category = "Writing",
                    Price = 1450000,
                    OriginalPrice = 2100000,
                    Duration = "10 tuần",
                    TeacherId = teacher1?.Id,
                    IsPublished = true,
                    IsFeatured = true,
                    CreatedAt = DateTime.Now.AddDays(-10)
                };

                var course6 = new Course
                {
                    Title = "Chiến Lược Đột Phá Reading & Listening 8.0",
                    ShortDescription = "Bẻ khóa các bẫy đề thi Cambridge, rèn tốc độ đọc 250 từ/phút và khả năng bắt từ khóa chính xác.",
                    Description = "Tổng hợp toàn bộ kỹ thuật làm bài thi Listening Section 3 & 4 và các bài đọc Reading Passage 3 hóc búa nhất. Cung cấp bộ ngân hàng từ vựng paraphrasing theo chủ đề.",
                    Thumbnail = "https://images.unsplash.com/photo-1497633762265-9d179a990aa6?auto=format&fit=crop&w=800&q=80",
                    Level = "Intermediate",
                    Category = "Reading",
                    Price = 1150000,
                    OriginalPrice = 1900000,
                    Duration = "8 tuần",
                    TeacherId = teacher1?.Id,
                    IsPublished = true,
                    IsFeatured = false,
                    CreatedAt = DateTime.Now.AddDays(-5)
                };

                context.Courses.AddRange(course1, course2, course3, course4, course5, course6);
                await context.SaveChangesAsync();

                // 6. Seed Chapters & Lessons for Course 1
                var ch1 = new Chapter { CourseId = course1.CourseId, Title = "Chương 1: Chiến Thuật Nghe Hiểu & Đột Phá Listening Section 1-2", OrderIndex = 1 };
                var ch2 = new Chapter { CourseId = course1.CourseId, Title = "Chương 2: Đọc Nhanh & Xử Lý Dạng Bài Reading Hóc Búa", OrderIndex = 2 };
                var ch3 = new Chapter { CourseId = course1.CourseId, Title = "Chương 3: Viết Học Thuật Writing Task 1 & Task 2 Chuẩn 7.5+", OrderIndex = 3 };
                var ch4 = new Chapter { CourseId = course1.CourseId, Title = "Chương 4: Phản Xạ & Từ Vựng Cấp Cao Speaking Part 1-3", OrderIndex = 4 };
                context.Chapters.AddRange(ch1, ch2, ch3, ch4);
                await context.SaveChangesAsync();

                // Lessons in Ch1
                var les1 = new Lesson
                {
                    ChapterId = ch1.ChapterId,
                    Title = "Bài 1: Chiến Thuật Form & Note Completion & Bẫy Số/Tên Riêng",
                    OrderIndex = 1,
                    VideoUrl = "https://www.youtube.com/embed/fA5l2oXvN5o",
                    AudioUrl = "https://www.soundhelix.com/examples/mp3/SoundHelix-Song-1.mp3",
                    DocumentUrl = "https://lumora.edu.vn/materials/ielts-listening-guide-unit1.pdf",
                    Content = @"<p>Trong dạng bài <strong>Form Completion</strong> và <strong>Note Completion</strong> của Section 1, thí sinh thường gặp các bẫy thông tin về việc người nói đính chính lại câu trả lời (self-correction). Hãy luôn chú ý các liên từ chỉ sự tương phản như <em>'actually', 'sorry, wait', 'in fact'</em>.</p>
                    <h5>Nguyên tắc vàng khi làm bài:</h5>
                    <ul>
                        <li>Kiểm tra kỹ giới hạn số từ (ví dụ: NO MORE THAN TWO WORDS AND/OR A NUMBER).</li>
                        <li>Dự đoán trước loại từ cần điền (danh từ, số điện thoại, ngày tháng, tên đường).</li>
                        <li>Luyện nghe chính xác phát âm các chữ cái dễ nhầm lẫn như A-E-I, J-G, H-8, B-P.</li>
                    </ul>",
                    Vocabulary = "[{\"word\":\"Accommodation\",\"type\":\"noun\",\"phonetic\":\"/əˌkɑː.məˈdeɪ.ʃən/\",\"meaning\":\"Chỗ ở, phòng trọ\",\"example\":\"The student center provides help with university accommodation.\"},{\"word\":\"Reservation\",\"type\":\"noun\",\"phonetic\":\"/ˌrez.ɚˈveɪ.ʃən/\",\"meaning\":\"Sự đặt trước / giữ chỗ\",\"example\":\"I would like to make a hotel reservation for two nights.\"},{\"word\":\"Correction\",\"type\":\"noun\",\"phonetic\":\"/kəˈrek.ʃən/\",\"meaning\":\"Sự đính chính, sửa lỗi\",\"example\":\"There was a quick correction on the departure time.\"}]",
                    Grammar = "Quy tắc hòa hợp giữa chủ ngữ và động từ với danh từ không đếm được (Information, Furniture, Accommodation luôn đi với động từ số ít).",
                    IsFreePreview = true
                };

                var les2 = new Lesson
                {
                    ChapterId = ch1.ChapterId,
                    Title = "Bài 2: Phương Pháp Map & Diagram Labeling - Định Vị Phương Hướng",
                    OrderIndex = 2,
                    VideoUrl = "https://www.youtube.com/embed/B_m1C1Bf3aU",
                    AudioUrl = "https://www.soundhelix.com/examples/mp3/SoundHelix-Song-2.mp3",
                    Content = @"<p>Dạng bài bản đồ (Map Labeling) kiểm tra khả năng bám sát lộ trình di chuyển của người nói. Điểm xuất phát (Starting point) luôn là yếu tố quan trọng nhất.</p>
                    <h5>Các cụm từ chỉ vị trí cốt lõi:</h5>
                    <ul>
                        <li>Opposite / Across from: đối diện.</li>
                        <li>Adjacent to / Next to: nằm liền kề.</li>
                        <li>Turn left at the intersection / roundabout: rẽ trái tại ngã tư / vòng xuyến.</li>
                        <li>Clockwise / Anti-clockwise: theo chiều / ngược chiều kim đồng hồ.</li>
                    </ul>",
                    Vocabulary = "[{\"word\":\"Intersection\",\"type\":\"noun\",\"phonetic\":\"/ˌɪn.t̬ɚˈsek.ʃən/\",\"meaning\":\"Ngã tư, điểm giao cắt\",\"example\":\"Turn right immediately after the busy intersection.\"},{\"word\":\"Adjacent\",\"type\":\"adj\",\"phonetic\":\"/əˈdʒeɪ.sənt/\",\"meaning\":\"Nằm sát bên, liền kề\",\"example\":\"The library is adjacent to the science complex.\"}]",
                    Grammar = "Sử dụng giới từ chỉ vị trí và chỉ phương hướng (Prepositions of Place and Movement: straight ahead, through the tunnel, past the reception desk).",
                    IsFreePreview = false
                };

                // Lessons in Ch2
                var les3 = new Lesson
                {
                    ChapterId = ch2.ChapterId,
                    Title = "Bài 3: Đọc Hiểu Nâng Cao - Phân Biệt True / False / Not Given Chuẩn Xác",
                    OrderIndex = 1,
                    VideoUrl = "https://www.youtube.com/embed/xGf_yM79V5g",
                    Content = @"<p>Rất nhiều bạn nhầm lẫn giữa <strong>FALSE</strong> và <strong>NOT GIVEN</strong>. Hãy nhớ định nghĩa chuẩn:</p>
                    <ul>
                        <li><strong>TRUE:</strong> Bài đọc khẳng định thông tin trùng khớp hoàn toàn (paraphrase).</li>
                        <li><strong>FALSE:</strong> Bài đọc đưa ra thông tin đối lập, mâu thuẫn 100% với đề bài.</li>
                        <li><strong>NOT GIVEN:</strong> Bài đọc không đề cập, hoặc không đủ căn cứ để kết luận đúng hay sai.</li>
                    </ul>",
                    Vocabulary = "[{\"word\":\"Contradict\",\"type\":\"verb\",\"phonetic\":\"/ˌkɑːn.trəˈdɪkt/\",\"meaning\":\"Mâu thuẫn, trái ngược\",\"example\":\"The study results contradict previous assumptions.\"},{\"word\":\"Unambiguous\",\"type\":\"adj\",\"phonetic\":\"/ˌʌn.æmˈbɪɡ.ju.əs/\",\"meaning\":\"Rõ ràng, không mơ hồ\",\"example\":\"We need unambiguous evidence before drawing conclusions.\"}]",
                    Grammar = "Nhận diện từ mang tính tuyệt đối (Always, Never, Only, Solely) và từ mang tính tương đối (Frequently, Likely, Often, May) trong câu hỏi Reading.",
                    IsFreePreview = false
                };

                var les4 = new Lesson
                {
                    ChapterId = ch3.ChapterId,
                    Title = "Bài 4: Writing Task 2 - Cấu Trúc Bài Viết 4 Đoạn Đạt Band 7.5+",
                    OrderIndex = 1,
                    VideoUrl = "https://www.youtube.com/embed/z1gKq_H5c4A",
                    Content = @"<p>Cấu trúc bài viết chuẩn gồm 4 đoạn:</p>
                    <ol>
                        <li><strong>Introduction:</strong> Paraphrase đề bài (1 câu) + Thesis statement nêu quan điểm cá nhân (1 câu).</li>
                        <li><strong>Body 1:</strong> Topic sentence + Luận điểm 1 + Giải thích nguyên nhân + Ví dụ thực tế.</li>
                        <li><strong>Body 2:</strong> Topic sentence + Luận điểm 2 + Phân tích hậu quả/lợi ích + Ví dụ minh chứng.</li>
                        <li><strong>Conclusion:</strong> Tóm tắt lại 2 luận điểm chính + Khẳng định lại quan điểm tác giả.</li>
                    </ol>",
                    Vocabulary = "[{\"word\":\"Substantiate\",\"type\":\"verb\",\"phonetic\":\"/səbˈstæn.ʃi.eɪt/\",\"meaning\":\"Chứng minh bằng dẫn chứng\",\"example\":\"You must substantiate your arguments with factual data.\"},{\"word\":\"Detrimental\",\"type\":\"adj\",\"phonetic\":\"/ˌdet.rəˈmen.t̬əl/\",\"meaning\":\"Có hại, bất lợi\",\"example\":\"Excessive screen time is detrimental to mental health.\"}]",
                    Grammar = "Cấu trúc câu phức chỉ nguyên nhân - kết quả: As a consequence of X, Y occurs. In addition to boosting X, Y also fosters Z.",
                    IsFreePreview = false
                };

                context.Lessons.AddRange(les1, les2, les3, les4);
                await context.SaveChangesAsync();

                // 7. Seed Exercises in Lesson 1
                var ex1 = new Exercise
                {
                    LessonId = les1.LessonId,
                    Title = "Bài Tập Thực Hành: Listening Form Completion & Keyword Tracking",
                    Description = "Nghe đoạn audio ngắn và hoàn thành 4 câu hỏi bên dưới. Mỗi câu chọn 1 đáp án chính xác nhất.",
                    TimeLimitMinutes = 15,
                    PassingScore = 75,
                    CreatedAt = DateTime.Now
                };
                context.Exercises.Add(ex1);
                await context.SaveChangesAsync();

                var q1 = new ExerciseQuestion
                {
                    ExerciseId = ex1.ExerciseId,
                    Skill = SkillType.Listening,
                    Format = QuestionFormat.MultipleChoice,
                    Content = "Người gọi điện muốn đăng ký phòng ở ghép thuộc loại nào sau đây?",
                    OptionsJson = JsonSerializer.Serialize(new[] { "A. Studio Apartment đơn lập", "B. Căn hộ 2 phòng ngủ gần trung tâm", "C. Ký túc xá sinh viên quốc tế", "D. Homestay cùng gia đình người bản xứ" }),
                    CorrectAnswer = "B. Căn hộ 2 phòng ngủ gần trung tâm",
                    Explanation = "Trong đoạn ghi âm, người nói nhấn mạnh: 'I was thinking about a two-bedroom apartment near downtown with a study room'.",
                    Points = 2.5m
                };

                var q2 = new ExerciseQuestion
                {
                    ExerciseId = ex1.ExerciseId,
                    Skill = SkillType.Listening,
                    Format = QuestionFormat.MultipleChoice,
                    Content = "Số tiền đặt cọc (deposit fee) ban đầu được thông báo sau khi đính chính là bao nhiêu?",
                    OptionsJson = JsonSerializer.Serialize(new[] { "A. $350", "B. $420", "C. $500", "D. $600" }),
                    CorrectAnswer = "B. $420",
                    Explanation = "Ban đầu nhân viên báo $500 nhưng sau đó đính chính: 'Wait, the policy changed last week, so it is just $420 now'.",
                    Points = 2.5m
                };

                var q3 = new ExerciseQuestion
                {
                    ExerciseId = ex1.ExerciseId,
                    Skill = SkillType.Grammar,
                    Format = QuestionFormat.MultipleChoice,
                    Content = "Chọn câu có cách dùng từ 'Accommodation' chính xác theo ngữ pháp tiếng Anh:",
                    OptionsJson = JsonSerializer.Serialize(new[] { "A. There are many accommodations in this city.", "B. The university accommodation is very affordable.", "C. An accommodation are ready for booking.", "D. Accommodation were difficult to find." }),
                    CorrectAnswer = "B. The university accommodation is very affordable.",
                    Explanation = "Accommodation là danh từ không đếm được (uncountable noun), không thêm -s và đi kèm động từ số ít 'is'.",
                    Points = 2.5m
                };

                var q4 = new ExerciseQuestion
                {
                    ExerciseId = ex1.ExerciseId,
                    Skill = SkillType.Reading,
                    Format = QuestionFormat.TrueFalse,
                    Content = "Thông tin: Người thuê phòng được phép nuôi thú cưng nhỏ trong căn hộ mà không phải trả thêm phí phụ thu. (True/False/Not Given)",
                    OptionsJson = JsonSerializer.Serialize(new[] { "A. TRUE", "B. FALSE", "C. NOT GIVEN" }),
                    CorrectAnswer = "B. FALSE",
                    Explanation = "Trong hợp đồng ghi rõ có phụ phí thú cưng $30/tháng (pet fee), do đó câu khẳng định 'không phải trả thêm' là FALSE.",
                    Points = 2.5m
                };

                context.ExerciseQuestions.AddRange(q1, q2, q3, q4);
                await context.SaveChangesAsync();

                // 8. Seed Live Classes
                var today = DateTime.Today;
                var live1 = new LiveClass
                {
                    ClassName = "Webinar Miễn Phí: Chiến Thuật Giải Mã Matching Headings Trong Reading 8.0",
                    CourseId = null, // Public class
                    TeacherId = teacher1?.Id ?? "",
                    ScheduledDate = today.AddDays(1),
                    StartTime = new TimeSpan(19, 30, 0),
                    EndTime = new TimeSpan(21, 0, 0),
                    LiveRoomUrl = "https://meet.lumora.edu.vn/room-public-ielts-webinar",
                    Capacity = 200,
                    Description = "Lớp học trực tuyến công khai dành cho tất cả học viên và bạn đọc quan tâm. Hướng dẫn chi tiết cách đọc lướt Topic sentence và loại trừ tiêu đề gây nhiễu.",
                    Status = ClassStatus.Upcoming,
                    IsPublic = true
                };

                var live2 = new LiveClass
                {
                    ClassName = "Lớp Chuyên Đề: Chữa Đề Writing Task 2 Dự Đoán Quý 4/2026 (IELTS Masterclass)",
                    CourseId = course1.CourseId, // Private class
                    TeacherId = teacher1?.Id ?? "",
                    ScheduledDate = today.AddDays(2),
                    StartTime = new TimeSpan(20, 0, 0),
                    EndTime = new TimeSpan(21, 30, 0),
                    LiveRoomUrl = "https://meet.lumora.edu.vn/room-masterclass-private",
                    Capacity = 25,
                    Description = "Lớp học riêng dành riêng cho học viên đã đăng ký khóa học IELTS Masterclass 7.5+. Sửa bài trực tiếp trên màn hình, chấm điểm từng tiêu chí IELTS Band Descriptors.",
                    Status = ClassStatus.Upcoming,
                    IsPublic = false
                };

                var live3 = new LiveClass
                {
                    ClassName = "Lớp Thực Chiến: Speaking 1-on-1 Sửa Lỗi Ngữ Điệu & Phát Âm Cùng Giảng Viên",
                    CourseId = course2.CourseId, // Private class
                    TeacherId = teacher2?.Id ?? "",
                    ScheduledDate = today.AddDays(4),
                    StartTime = new TimeSpan(18, 0, 0),
                    EndTime = new TimeSpan(19, 30, 0),
                    LiveRoomUrl = "https://meet.lumora.edu.vn/room-foundation-speaking",
                    Capacity = 20,
                    Description = "Lớp học tương tác phản xạ Speaking dành cho học viên lớp Foundation. Luyện nói theo cặp (Breakout rooms) và nhận feedback trực tiếp từ Mr. James.",
                    Status = ClassStatus.Upcoming,
                    IsPublic = false
                };

                context.LiveClasses.AddRange(live1, live2, live3);
                await context.SaveChangesAsync();

                // 9. Seed Tests (Example Test & Placement Test)
                var exampleTest = new Test
                {
                    Title = "IELTS Quick Diagnostic Test (Free 4 Skills)",
                    Description = "A free diagnostic practice examination evaluating Listening, Reading, Writing, and Speaking with instantaneous performance assessment.",
                    Type = ExamType.ExampleTest,
                    Thumbnail = "https://images.unsplash.com/photo-1434030216411-0b793f4b4173?auto=format&fit=crop&w=600&q=80",
                    TimeLimitMinutes = 30,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };

                var placementTest = new Test
                {
                    Title = "LUMORA Official Placement Test (Comprehensive 4 Skills)",
                    Description = "An in-depth assessment establishing diagnostic baseline proficiency across CEFR levels A1 to C1 (IELTS Band 3.5 to 8.0+), providing detailed skill breakdown and targeted preparation pathways.",
                    Type = ExamType.PlacementTest,
                    Thumbnail = "https://images.unsplash.com/photo-1497633762265-9d179a990aa6?auto=format&fit=crop&w=600&q=80",
                    TimeLimitMinutes = 45,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };

                context.Tests.AddRange(exampleTest, placementTest);
                await context.SaveChangesAsync();

                // Questions for Example Test (4 skills)
                var eq1 = new TestQuestion
                {
                    TestId = exampleTest.TestId,
                    Skill = SkillType.Listening,
                    Format = QuestionFormat.MultipleChoice,
                    Content = "Listen to the recording and answer the question: What are the opening hours of the computer lab on Saturdays?",
                    AudioUrl = "https://www.soundhelix.com/examples/mp3/SoundHelix-Song-3.mp3",
                    OptionsJson = JsonSerializer.Serialize(new[] { "A. 8:00 AM - 12:00 PM", "B. 9:00 AM - 5:00 PM", "C. 10:00 AM - 4:00 PM", "D. Closed all day on Saturdays" }),
                    CorrectAnswer = "B. 9:00 AM - 5:00 PM",
                    Explanation = "The speaker states: 'On Saturdays, our computer labs open from nine in the morning until five in the afternoon.'",
                    Points = 2.5m
                };

                var eq2 = new TestQuestion
                {
                    TestId = exampleTest.TestId,
                    Skill = SkillType.Reading,
                    Format = QuestionFormat.MultipleChoice,
                    Passage = "The development of sustainable architecture has gained immense traction over the past two decades. Rather than merely reducing operational energy consumption, modern eco-buildings actively integrate renewable sources such as photovoltaic facades and rainwater harvesting systems. Architects now emphasize biophilic design—the deliberate incorporation of natural elements to enhance cognitive function and occupant well-being.",
                    Content = "According to the passage, what primary benefit does biophilic design offer to building occupants?",
                    OptionsJson = JsonSerializer.Serialize(new[] { "A. Saving 50% on essential structural costs", "B. Enhancing cognitive function and physical/mental well-being", "C. Fully replacing the municipal electrical grid", "D. Lowering industrial pollutant emissions in factories" }),
                    CorrectAnswer = "B. Enhancing cognitive function and physical/mental well-being",
                    Explanation = "The text clearly notes that architects emphasize biophilic design 'to enhance cognitive function and occupant well-being.'",
                    Points = 2.5m
                };

                var eq3 = new TestQuestion
                {
                    TestId = exampleTest.TestId,
                    Skill = SkillType.Writing,
                    Format = QuestionFormat.Writing,
                    Content = "Writing Task 2 Prompt:\n'Some people believe that university students should focus solely on academic subjects, while others argue that practical life skills should also be taught. Discuss both views and give your opinion.' (Write at least 150 words).",
                    CorrectAnswer = "Assessment Criteria: Task Achievement, Coherence & Cohesion, Lexical Resource, Grammatical Range & Accuracy.",
                    Explanation = "Responses must present a clear personal position, address both views with relevant support, and use appropriate academic vocabulary such as 'holistic education' and 'vocational competence'.",
                    Points = 2.5m
                };

                var eq4 = new TestQuestion
                {
                    TestId = exampleTest.TestId,
                    Skill = SkillType.Speaking,
                    Format = QuestionFormat.Speaking,
                    Content = "Speaking Part 2 Prompt:\n'Describe an artificial intelligence tool or website that has significantly helped you improve your study efficiency. You should say: what it is, how often you use it, what features you find most beneficial, and explain why it enhances your learning.' (Prepare for 1 minute and speak for 1-2 minutes).",
                    CorrectAnswer = "Assessment Criteria: Fluency & Coherence, Lexical Resource, Grammatical Range & Accuracy, Pronunciation.",
                    Explanation = "Candidates should speak fluently using discourse markers such as 'First and foremost', 'What distinguishes this tool is...', and 'In terms of usability...'.",
                    Points = 2.5m
                };

                // Questions for Placement Test (Comprehensive)
                var pq1 = new TestQuestion
                {
                    TestId = placementTest.TestId,
                    Skill = SkillType.Listening,
                    Format = QuestionFormat.MultipleChoice,
                    Content = "Listen to the conversation: What is the primary reason why the customer is requesting a refund?",
                    AudioUrl = "https://www.soundhelix.com/examples/mp3/SoundHelix-Song-4.mp3",
                    OptionsJson = JsonSerializer.Serialize(new[] { "A. The delivery was delayed by three weeks", "B. The internal components were damaged in transit", "C. The delivered product did not match the ordered model and specifications", "D. The customer changed their mind and preferred an alternative item" }),
                    CorrectAnswer = "C. The delivered product did not match the ordered model and specifications",
                    Explanation = "The customer clearly states: 'I received a completely different model and the dimensions are way off.'",
                    Points = 2.5m
                };

                var pq2 = new TestQuestion
                {
                    TestId = placementTest.TestId,
                    Skill = SkillType.Reading,
                    Format = QuestionFormat.MultipleChoice,
                    Passage = "Artificial intelligence in language education has shifted from automated rule-checking to sophisticated contextual reasoning. Modern Large Language Models can decipher subtle nuances, recognize figurative language, and provide personalized adaptive feedback that rivals human tutoring.",
                    Content = "In the passage, what does the phrase 'subtle nuances' mean?",
                    OptionsJson = JsonSerializer.Serialize(new[] { "A. Elementary spelling and formatting rules", "B. Delicate distinctions and fine shades of meaning", "C. Severe syntax and morphological errors", "D. Computational processing speeds" }),
                    CorrectAnswer = "B. Delicate distinctions and fine shades of meaning",
                    Explanation = "'Nuance' refers to a subtle difference in meaning, tone, or context.",
                    Points = 2.5m
                };

                var pq3 = new TestQuestion
                {
                    TestId = placementTest.TestId,
                    Skill = SkillType.Writing,
                    Format = QuestionFormat.Writing,
                    Content = "IELTS Academic Writing Task 2:\n'In many countries, online education is increasingly replacing traditional classroom settings. Do the advantages of this trend outweigh the disadvantages?' Write an introduction and at least one well-developed body paragraph (minimum 150 words).",
                    CorrectAnswer = "Assessment Criteria: Task Response, Coherence & Cohesion, Lexical Resource, Grammatical Range & Accuracy.",
                    Explanation = "Evaluate arguments logically, address advantages versus disadvantages, and maintain academic vocabulary and sentence structure.",
                    Points = 2.5m
                };

                var pq4 = new TestQuestion
                {
                    TestId = placementTest.TestId,
                    Skill = SkillType.Speaking,
                    Format = QuestionFormat.Speaking,
                    Content = "IELTS Speaking Part 3 Discussion:\n'How has digital technology changed the way younger generations acquire foreign languages compared to previous generations?' (Record or outline your 1-2 minute analytical response).",
                    CorrectAnswer = "Assessment Criteria: Fluency & Coherence, Lexical Resource, Grammatical Range & Accuracy, Pronunciation.",
                    Explanation = "Focus on abstract reasoning, balanced comparative analysis, idiomatic language, and natural pronunciation features.",
                    Points = 2.5m
                };

                context.TestQuestions.AddRange(eq1, eq2, eq3, eq4, pq1, pq2, pq3, pq4);
                await context.SaveChangesAsync();

                // 10. Seed IELTS Tips (Blog Articles)
                var tips = new List<IeltsTip>
                {
                    new IeltsTip
                    {
                        Title = "Top 7 Cụm Từ 'Vàng' Nâng Band Điểm Speaking Part 2 Lên 7.5+",
                        Summary = "Khám phá các Idiomatic Expressions và Collocations tự nhiên giúp bạn ghi điểm tuyệt đối trong tiêu chí Lexical Resource mà không bị gượng gạo.",
                        Category = "Speaking",
                        Thumbnail = "https://images.unsplash.com/photo-1544717305-2782549b5136?auto=format&fit=crop&w=600&q=80",
                        AuthorName = "Ms. Sarah Jenkins (Ex-Examiner)",
                        ViewsCount = 1420,
                        IsPublished = true,
                        CreatedAt = DateTime.Now.AddDays(-14),
                        Content = @"<p>Trong bài thi <strong>IELTS Speaking Part 2</strong>, giám khảo đánh giá rất cao khả năng sử dụng từ vựng tự nhiên (Less common lexical items). Dưới đây là 7 cụm từ cực kỳ hiệu quả:</p>
                        <ol>
                            <li><strong>Once in a blue moon:</strong> Rất hiếm khi làm điều gì đó (thay cho 'very rarely').</li>
                            <li><strong>Over the moon:</strong> Vô cùng hạnh phúc và phấn khởi (thay cho 'extremely happy').</li>
                            <li><strong>Steep learning curve:</strong> Một quá trình học hỏi đầy thử thách nhưng bổ ích.</li>
                            <li><strong>Broaden my horizons:</strong> Mở rộng tầm mắt và hiểu biết thế giới quan.</li>
                            <li><strong>Cost an arm and a leg:</strong> Cực kỳ đắt đỏ, tốn kém tiền bạc.</li>
                            <li><strong>Hit the nail on the head:</strong> Nói trúng tim đen, mô tả chính xác vấn đề.</li>
                            <li><strong>A blessing in disguise:</strong> Chuyện tưởng chừng xui xẻo nhưng hóa ra lại đem lại kết quả tốt đẹp.</li>
                        </ol>
                        <p><em>Lời khuyên:</em> Đừng cố nhồi nhét quá nhiều thành ngữ trong một câu, hãy dùng chúng một cách đúng ngữ cảnh và chuẩn xác ngữ điệu.</p>"
                    },
                    new IeltsTip
                    {
                        Title = "Bí Quyết Phân Biệt FALSE và NOT GIVEN Trong IELTS Reading Không Bao Giờ Sai",
                        Summary = "Phương pháp tư duy logic loại bỏ bẫy đề thi Cambridge Reading, giúp bạn tự tin xử lý trọn vẹn 100% câu hỏi dạng T/F/NG.",
                        Category = "Reading",
                        Thumbnail = "https://images.unsplash.com/photo-1456513080510-7bf3a84b82f8?auto=format&fit=crop&w=600&q=80",
                        AuthorName = "Mr. James Watson",
                        ViewsCount = 2850,
                        IsPublished = true,
                        CreatedAt = DateTime.Now.AddDays(-10),
                        Content = @"<p>Dạng bài <strong>True / False / Not Given</strong> là 'ác mộng' của rất nhiều sĩ tử IELTS. Để không bị nhầm lẫn, hãy áp dụng quy tắc 3 bước sau:</p>
                        <h4>Bước 1: Xác định từ khóa không thể thay thế (Anchor Keywords)</h4>
                        <p>Các danh từ riêng, con số, thuật ngữ khoa học là các neo tìm kiếm giúp bạn định vị chính xác vị trí thông tin trong bài đọc.</p>
                        <h4>Bước 2: Tìm từ khóa quyết định tính chân lý (Focus Keywords)</h4>
                        <p>Tìm các tính từ, trạng từ chỉ tần suất hoặc mức độ (e.g., all, some, never, substantially, significantly). Chỉ cần focus keywords bị sai lệch, câu đó sẽ chuyển từ TRUE thành FALSE.</p>
                        <h4>Bước 3: Nguyên tắc mâu thuẫn 100%</h4>
                        <p>Nếu thông tin đối lập hoàn toàn, chọn <strong>FALSE</strong>. Nếu bạn phải suy diễn hoặc bài đọc không khẳng định cũng không phủ định, dứt khoát chọn <strong>NOT GIVEN</strong>.</p>"
                    },
                    new IeltsTip
                    {
                        Title = "Cấu Trúc Câu Phức & Câu Đảo Ngữ Giúp Đạt Band 7.0+ Writing Task 2",
                        Summary = "Bộ công thức ngữ pháp nâng cao giúp bài viết của bạn trở nên mạch lạc, uyển chuyển và thỏa mãn tiêu chí Grammatical Range and Accuracy.",
                        Category = "Writing",
                        Thumbnail = "https://images.unsplash.com/photo-1455390582262-044cdead277a?auto=format&fit=crop&w=600&q=80",
                        AuthorName = "Ms. Sarah Jenkins",
                        ViewsCount = 3120,
                        IsPublished = true,
                        CreatedAt = DateTime.Now.AddDays(-7),
                        Content = @"<p>Giám khảo chấm thi IELTS luôn tìm kiếm sự đa dạng trong cấu trúc câu. Bài viết chỉ dùng câu đơn và câu ghép (and, but, so) sẽ khó vượt qua mức Band 6.0.</p>
                        <h4>1. Cấu trúc đảo ngữ với 'Not only... but also'</h4>
                        <p><em>Công thức:</em> Not only + Trợ động từ + S + V, but S + also + V.</p>
                        <p><em>Ví dụ:</em> Not only does renewable energy curb environmental degradation, but it also creates thousands of green jobs.</p>
                        <h4>2. Cấu trúc câu điều kiện loại 3 đảo ngữ (Had it not been for...)</h4>
                        <p><em>Ví dụ:</em> Had it not been for government subsidies, the green transit system would have failed.</p>
                        <h4>3. Phân từ hiện tại / quá khứ rút gọn (Participle Clauses)</h4>
                        <p><em>Ví dụ:</em> Facing unprecedented climate hazards, nations must coordinate globally.</p>"
                    },
                    new IeltsTip
                    {
                        Title = "Phương Pháp Nghe Chép Chính Tả (Dictation) Giúp Đạt 8.5 Listening",
                        Summary = "Lộ trình 30 ngày luyện tai nghe nhạy bén với âm thanh bản xứ, nhận diện nuốt âm, nối âm và biến âm trong bài thi IELTS Listening Section 3-4.",
                        Category = "Listening",
                        Thumbnail = "https://images.unsplash.com/photo-1511671782779-c97d3d27a1d4?auto=format&fit=crop&w=600&q=80",
                        AuthorName = "Mr. James Watson",
                        ViewsCount = 1980,
                        IsPublished = true,
                        CreatedAt = DateTime.Now.AddDays(-5),
                        Content = @"<p>Nghe chép chính tả (Transcribing / Dictation) là chìa khóa vàng giúp bạn chuyển đổi từ việc nghe thụ động sang nghe chủ động và nhận biết 100% âm thanh thực tế.</p>
                        <h5>Các bước thực hiện chuẩn khoa học:</h5>
                        <ul>
                            <li><strong>Lần 1:</strong> Nghe tổng quát cả đoạn không bấm dừng để hiểu ý chính.</li>
                            <li><strong>Lần 2:</strong> Nghe từng câu (ngắt sau 5-7 từ) và chép lại toàn bộ ra giấy. Chú ý các âm đuôi /s/, /ed/.</li>
                            <li><strong>Lần 3:</strong> Nghe lại lần cuối và đối chiếu với transcript chính thức để dùng bút đỏ khoanh các chỗ nghe sót.</li>
                        </ul>"
                    },
                    new IeltsTip
                    {
                        Title = "100 Academic Collocations Thường Xuất Hiện Trong Đề Thi IELTS 2026",
                        Summary = "Tổng hợp các cặp từ học thuật đi liền với nhau giúp bài viết và bài nói của bạn chuẩn văn phong bản ngữ và giàu tính học thuật.",
                        Category = "Vocabulary",
                        Thumbnail = "https://images.unsplash.com/photo-1497633762265-9d179a990aa6?auto=format&fit=crop&w=600&q=80",
                        AuthorName = "Lumora Academic Team",
                        ViewsCount = 4210,
                        IsPublished = true,
                        CreatedAt = DateTime.Now.AddDays(-3),
                        Content = @"<p>Sử dụng đúng Collocations là dấu hiệu rõ nhất phân biệt người dùng tiếng Anh tự nhiên và người dịch thô từng từ.</p>
                        <ul>
                            <li><strong>Pose a threat to:</strong> Gây ra mối đe dọa lớn đối với...</li>
                            <li><strong>Exert pressure on:</strong> Gây áp lực nặng nề lên...</li>
                            <li><strong>Pave the way for:</strong> Mở đường, tạo tiền đề thuận lợi cho...</li>
                            <li><strong>Draw conclusions from:</strong> Rút ra những kết luận sâu sắc từ...</li>
                            <li><strong>Have a profound impact on:</strong> Có ảnh hưởng sâu rộng đến...</li>
                        </ul>"
                    },
                    new IeltsTip
                    {
                        Title = "Mệnh Đề Quan Hệ Rút Gọn & Các Lỗi Ngữ Pháp Thường Gặp Cần Tránh",
                        Summary = "Cách làm chủ mệnh đề quan hệ dạng V-ing, V-ed và To-V mà không mắc lỗi sai câu thiếu vị ngữ (Sentence Fragments).",
                        Category = "Grammar",
                        Thumbnail = "https://images.unsplash.com/photo-1434030216411-0b793f4b4173?auto=format&fit=crop&w=600&q=80",
                        AuthorName = "Ms. Sarah Jenkins",
                        ViewsCount = 2150,
                        IsPublished = true,
                        CreatedAt = DateTime.Now.AddDays(-1),
                        Content = @"<p>Rút gọn mệnh đề quan hệ giúp câu văn súc tích và học thuật hơn rất nhiều trong IELTS Writing:</p>
                        <p><strong>1. Dạng chủ động -> V-ing:</strong><br/>
                        Gốc: The students who take the exam today are from Vietnam.<br/>
                        Rút gọn: The students <em>taking</em> the exam today are from Vietnam.</p>
                        <p><strong>2. Dạng bị động -> V-ed (V3):</strong><br/>
                        Gốc: The research which was conducted by Cambridge scientists revealed key insights.<br/>
                        Rút gọn: The research <em>conducted</em> by Cambridge scientists revealed key insights.</p>"
                    }
                };

                context.IeltsTips.AddRange(tips);
                await context.SaveChangesAsync();

                // 11. Seed Enrollment & Progress for demo student
                if (demoStudent != null)
                {
                    var enrollment = new Enrollment
                    {
                        StudentId = demoStudent.Id,
                        CourseId = course1.CourseId,
                        EnrolledAt = DateTime.Now.AddDays(-10),
                        PricePaid = course1.Price,
                        PaymentMethod = "VNPay",
                        PaymentStatus = "Completed",
                        TransactionId = "TXN" + DateTime.Now.Ticks.ToString().Substring(10)
                    };
                    context.Enrollments.Add(enrollment);
                    await context.SaveChangesAsync();

                    // Mark lesson 1 completed
                    var prog1 = new LessonProgress
                    {
                        StudentId = demoStudent.Id,
                        LessonId = les1.LessonId,
                        IsCompleted = true,
                        CompletedAt = DateTime.Now.AddDays(-5),
                        LastAccessedAt = DateTime.Now.AddHours(-2)
                    };
                    context.LessonProgresses.Add(prog1);

                    // Add submission for Exercise 1
                    var sub = new ExerciseSubmission
                    {
                        ExerciseId = ex1.ExerciseId,
                        StudentId = demoStudent.Id,
                        SubmittedAt = DateTime.Now.AddDays(-4),
                        AutoScore = 7.5m,
                        TeacherScore = 8.0m,
                        TotalScore = 8.0m,
                        TeacherComment = "Làm bài rất tốt! Cần chú ý thêm bẫy số đính chính trong phần Nghe.",
                        AnswersJson = "{\"Q1\":\"B. Căn hộ 2 phòng ngủ gần trung tâm\",\"Q2\":\"B. $420\",\"Q3\":\"B. The university accommodation is very affordable.\",\"Q4\":\"B. FALSE\"}"
                    };
                    context.ExerciseSubmissions.Add(sub);

                    // Add Notification for student
                    var notif1 = new Notification
                    {
                        UserId = demoStudent.Id,
                        Title = "Nhắc nhở lớp học trực tuyến sắp tới",
                        Content = "Bạn có lớp học 'Webinar: Chiến thuật giải mã Matching Headings' diễn ra vào 19:30 ngày mai. Hãy kiểm tra thiết bị sẵn sàng!",
                        Type = "UpcomingClass",
                        LinkUrl = "/Class/Room/" + live1.LiveClassId,
                        IsRead = false,
                        CreatedAt = DateTime.Now.AddHours(-3)
                    };

                    var notif2 = new Notification
                    {
                        UserId = demoStudent.Id,
                        Title = "Bài tập đã được chấm điểm",
                        Content = "Giảng viên Sarah Jenkins đã chấm điểm bài tập 'Listening Form Completion & Keyword Tracking' của bạn: 8.0/10.",
                        Type = "ExerciseDue",
                        LinkUrl = "/Student/Exercise/" + ex1.ExerciseId,
                        IsRead = false,
                        CreatedAt = DateTime.Now.AddHours(-1)
                    };

                    context.Notifications.AddRange(notif1, notif2);
                    await context.SaveChangesAsync();
                }
            }

            // 11. Ensure all 7 teachers have assigned courses and live classes synchronized
            var t1 = await userManager.FindByEmailAsync("teacher.sarah@lumora.edu.vn");
            var t2 = await userManager.FindByEmailAsync("teacher.james@lumora.edu.vn");
            var t3 = await userManager.FindByEmailAsync("teacher.david@lumora.edu.vn");
            var t4 = await userManager.FindByEmailAsync("teacher.emma@lumora.edu.vn");
            var t5 = await userManager.FindByEmailAsync("teacher.daniel@lumora.edu.vn");
            var t6 = await userManager.FindByEmailAsync("teacher.olivia@lumora.edu.vn");
            var t7 = await userManager.FindByEmailAsync("teacher.robert@lumora.edu.vn");

            // Add courses for Dr. David Miller & Mr. Robert Clark if missing
            if (!await context.Courses.AnyAsync(c => c.Title.Contains("IELTS Academic Writing Task 2 & Critical Thinking 8.0+")))
            {
                var course7 = new Course
                {
                    Title = "IELTS Academic Writing Task 2 & Critical Thinking 8.0+",
                    ShortDescription = "Làm chủ tư duy phản biện, lập luận logic và vốn từ học thuật C1-C2 cùng Dr. David Miller.",
                    Description = "Khóa học cao cấp hướng dẫn phương pháp bẻ khóa mọi chủ đề khó của IELTS Writing Task 2 (Education, Technology, Society, Crime...). Được chấm chữa chi tiết từng luận điểm.",
                    Thumbnail = "https://images.unsplash.com/photo-1455390582262-044cdead277a?auto=format&fit=crop&w=800&q=80",
                    Level = "Advanced",
                    Category = "Writing",
                    Price = 2850000,
                    OriginalPrice = 3900000,
                    Duration = "12 tuần",
                    TeacherId = t3?.Id,
                    IsPublished = true,
                    IsFeatured = true,
                    CreatedAt = DateTime.Now
                };
                context.Courses.Add(course7);
                await context.SaveChangesAsync();
            }

            if (!await context.Courses.AnyAsync(c => c.Title.Contains("IELTS Computer-Delivered CDI Master Strategy 8.0+")))
            {
                var course8 = new Course
                {
                    Title = "IELTS Computer-Delivered CDI Master Strategy 8.0+",
                    ShortDescription = "Chiến lược làm chủ 100% giao diện thi trên máy tính, quản lý thời gian và thao tác tối ưu cùng Giám đốc học thuật.",
                    Description = "Khóa học độc quyền từ Hội đồng học thuật LUMORA hướng dẫn kỹ năng highlight, note, split-screen và phản xạ gõ phím nhanh chuẩn format kỳ thi IELTS trên máy tính chính thức.",
                    Thumbnail = "https://images.unsplash.com/photo-1516321318423-f06f85e504b3?auto=format&fit=crop&w=800&q=80",
                    Level = "All Levels",
                    Category = "IELTS",
                    Price = 1950000,
                    OriginalPrice = 2900000,
                    Duration = "8 tuần",
                    TeacherId = t7?.Id,
                    IsPublished = true,
                    IsFeatured = true,
                    CreatedAt = DateTime.Now
                };
                context.Courses.Add(course8);
                await context.SaveChangesAsync();
            }

            // Sync teachers to appropriate courses
            var courseIPA = await context.Courses.FirstOrDefaultAsync(c => c.Title.Contains("Luyện Phát Âm Chuẩn IPA"));
            if (courseIPA != null && t4 != null && courseIPA.TeacherId != t4.Id)
            {
                courseIPA.TeacherId = t4.Id;
            }

            var courseGrammar = await context.Courses.FirstOrDefaultAsync(c => c.Title.Contains("Ngữ Pháp Chuyên Sâu"));
            if (courseGrammar != null && t5 != null && courseGrammar.TeacherId != t5.Id)
            {
                courseGrammar.TeacherId = t5.Id;
            }

            var courseReading = await context.Courses.FirstOrDefaultAsync(c => c.Title.Contains("Chiến Lược Đột Phá Reading"));
            if (courseReading != null && t6 != null && courseReading.TeacherId != t6.Id)
            {
                courseReading.TeacherId = t6.Id;
            }
            await context.SaveChangesAsync();

            // Sync Live Classes for all teachers
            var classDate = DateTime.Today;
            var additionalLiveClasses = new List<LiveClass>
            {
                new LiveClass
                {
                    ClassName = "Masterclass: Tư Duy Phản Biện & Luận Điểm Học Thuật Band 8.5+ (Writing Task 2)",
                    CourseId = (await context.Courses.FirstOrDefaultAsync(c => c.Title.Contains("Writing Task 2")))?.CourseId,
                    TeacherId = t3?.Id ?? "",
                    ScheduledDate = classDate.AddDays(3),
                    StartTime = new TimeSpan(19, 0, 0),
                    EndTime = new TimeSpan(20, 30, 0),
                    LiveRoomUrl = "https://meet.lumora.edu.vn/room-david-masterclass",
                    Capacity = 35,
                    Description = "Hội thảo trực tuyến chuyên sâu cùng Dr. David Miller: Khai thác phương pháp Brainstorming logic, triển khai ý mạch lạc và liên kết câu không bị lỗi tư duy tiếng Việt.",
                    Status = ClassStatus.Upcoming,
                    IsPublic = false
                },
                new LiveClass
                {
                    ClassName = "Lớp Thực Hành: Phát Âm Chuyên Sâu, Nối Âm & Ngữ Điệu Tự Nhiên Cho Speaking 8.0",
                    CourseId = courseIPA?.CourseId,
                    TeacherId = t4?.Id ?? "",
                    ScheduledDate = classDate.AddDays(5),
                    StartTime = new TimeSpan(19, 30, 0),
                    EndTime = new TimeSpan(21, 0, 0),
                    LiveRoomUrl = "https://meet.lumora.edu.vn/room-emma-speaking",
                    Capacity = 25,
                    Description = "Thực hành trực tiếp 1-1 và theo nhóm cùng Ms. Emma Richardson. Rèn luyện trọng âm từ, ngắt câu theo cụm nghĩa (chunking) và nhịp điệu tự nhiên của người bản ngữ.",
                    Status = ClassStatus.Upcoming,
                    IsPublic = false
                },
                new LiveClass
                {
                    ClassName = "Webinar Miễn Phí: Chiến Lược Giải Đề Reading Passage 3 'Bất Bại' Không Cần Dịch Hết",
                    CourseId = null,
                    TeacherId = t6?.Id ?? "",
                    ScheduledDate = classDate.AddDays(6),
                    StartTime = new TimeSpan(20, 0, 0),
                    EndTime = new TimeSpan(21, 30, 0),
                    LiveRoomUrl = "https://meet.lumora.edu.vn/room-olivia-reading-webinar",
                    Capacity = 250,
                    Description = "Buổi chia sẻ cộng đồng miễn phí từ Ms. Olivia Taylor: Cách định vị từ khóa paraphrased, vượt qua bẫy Yes/No/Not Given và quản lý thời gian 20 phút cho Passage khó nhất.",
                    Status = ClassStatus.Upcoming,
                    IsPublic = true
                },
                new LiveClass
                {
                    ClassName = "Workshop Tinh Gọn: Nâng Cấp Vốn Từ Vựng C1-C2 Cho Bài Viết Học Thuật Task 1 & 2",
                    CourseId = courseGrammar?.CourseId,
                    TeacherId = t5?.Id ?? "",
                    ScheduledDate = classDate.AddDays(7),
                    StartTime = new TimeSpan(18, 30, 0),
                    EndTime = new TimeSpan(20, 0, 0),
                    LiveRoomUrl = "https://meet.lumora.edu.vn/room-daniel-writing-vocab",
                    Capacity = 30,
                    Description = "Phân tích và sửa bài chi tiết cùng Mr. Daniel Evans: Thay thế các từ vựng thông dụng bằng cụm Collocations học thuật sắc sảo, tối ưu tiêu chí Lexical Resource.",
                    Status = ClassStatus.Upcoming,
                    IsPublic = false
                },
                new LiveClass
                {
                    ClassName = "Live Simulation: Mô Phỏng Thi Thật Speaking Part 2 & 3 Trực Tiếp Cùng Ban Học Thuật",
                    CourseId = null,
                    TeacherId = t7?.Id ?? "",
                    ScheduledDate = classDate.AddDays(8),
                    StartTime = new TimeSpan(19, 30, 0),
                    EndTime = new TimeSpan(21, 0, 0),
                    LiveRoomUrl = "https://meet.lumora.edu.vn/room-robert-simulation",
                    Capacity = 150,
                    Description = "Phòng thi thử trực tuyến tương tác cao: Giám đốc học thuật Mr. Robert Clark trực tiếp phỏng vấn mẫu thí sinh ngẫu nhiên, nhận xét điểm mạnh yếu và hướng dẫn chấm điểm theo Band Descriptors.",
                    Status = ClassStatus.Upcoming,
                    IsPublic = true
                }
            };

            foreach (var live in additionalLiveClasses)
            {
                if (!await context.LiveClasses.AnyAsync(lc => lc.ClassName == live.ClassName))
                {
                    context.LiveClasses.Add(live);
                }
            }
            await context.SaveChangesAsync();
        }
    }
}

