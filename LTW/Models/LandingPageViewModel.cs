// ============================================================================
// Main Author: hoangthuhang
// Project: LTW Project - IELTS Online Tests Landing Page System
// Architecture: ASP.NET Core MVC - Object-Oriented Programming (OOP)
// File: Models/LandingPageViewModel.cs
// Description: Lớp ViewModel trung tâm mô hình hóa toàn bộ cấu trúc dữ liệu của
//              Landing Page theo chuẩn giao diện ieltsonlinetests.com:
//              1. Platinum Partner Hero Banner (British Council & IDP)
//              2. 6 Steps to Achieve IELTS Goals
//              3. Latest Free IELTS Materials (Mock Tests 2025/2026)
//              4. Online IELTS Courses (Designed by Ex-Examiners with Roadmap)
//              5. Live Lessons with Registration
//              6. Social Proof & Testimonials (TikTok, Facebook, YouTube)
//              7. Expert Teaching Team (Band 8.5 - 9.0 Ex-Examiners)
//              8. Valued University Partners
//              9. Interactive FAQs (Accordion)
//              10. Contact Consultation Form Model
// ============================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LTW.Models
{
    /// <summary>
    /// ViewModel đóng gói dữ liệu hoàn chỉnh
    /// </summary>
    public class LandingPageViewModel
    {
        // 1. Banner / Hero Section Data
        public string PartnerHeadline { get; set; } = string.Empty;
        public string HeroTitle { get; set; } = string.Empty;
        public string HeroSubtitle { get; set; } = string.Empty;

        // 2. Section: 6 steps to Achieve your IELTS goals
        public List<StudyStepItem> Steps { get; set; } = new List<StudyStepItem>();

        // 3. Section: Latest Free IELTS Materials (Mock Tests)
        public List<IeltsMaterialItem> LatestMaterials { get; set; } = new List<IeltsMaterialItem>();

        // 4. Section: Online IELTS Course (Designed by Ex-Examiners)
        public List<IeltsCourseItem> Courses { get; set; } = new List<IeltsCourseItem>();
        public List<RoadmapMilestone> RoadmapMilestones { get; set; } = new List<RoadmapMilestone>();

        // 5. Section: Live Lessons (Lớp học trực tuyến miễn phí)
        public List<LiveLessonItem> LiveLessons { get; set; } = new List<LiveLessonItem>();

        // 6. Section: Testimonials & Social Proof (TikTok, Facebook, YouTube)
        public List<SocialTestimonialItem> SocialTestimonials { get; set; } = new List<SocialTestimonialItem>();

        // 7. Section: Expert Teaching Team
        public List<TeacherExpertItem> TeachingTeam { get; set; } = new List<TeacherExpertItem>();

        // 8. Section: Valued Partners (Các trường đại học & tổ chức uy tín)
        public List<PartnerItem> Partners { get; set; } = new List<PartnerItem>();

        // 9. Section: FAQ (Câu hỏi thường gặp)
        public List<FaqItem> Faqs { get; set; } = new List<FaqItem>();

        // 10. Section: Form liên hệ tư vấn
        public ContactConsultationModel ConsultationForm { get; set; } = new ContactConsultationModel();

        /// <summary>
        /// Constructor khởi tạo dữ liệu mẫu phong phú và đầy đủ 
        /// </summary>
        public LandingPageViewModel()
        {
            PartnerHeadline = "Proud to be PLATINUM PARTNER of the BRITISH COUNCIL and IDP for many years";
            HeroTitle = "Luyện Thi IELTS Online Chuẩn Quốc Tế Với Đề Thi Thật";
            HeroSubtitle = "Nền tảng kiểm tra và rèn luyện kỹ năng IELTS của LUMORA English Center với hơn 500+ đề thi mô phỏng bài thi trên máy tính (Computer-delivered), đáp án chuẩn xác và phân tích chuyên sâu.";

            InitSteps();
            InitLatestMaterials();
            InitCoursesAndRoadmap();
            InitLiveLessons();
            InitSocialTestimonials();
            InitTeachingTeam();
            InitPartners();
            InitFaqs();
        }

        private void InitSteps()
        {
            Steps = new List<StudyStepItem>
            {
                new StudyStepItem(
                    stepNumber: 1,
                    title: "Placement Test",
                    description: "Kiểm tra trình độ đầu vào nhanh chóng với bài test chuẩn hóa giúp xác định band điểm hiện tại của bạn.",
                    iconClass: "bi bi-clipboard-check-fill",
                    tag: "Bước 1"
                ),
                new StudyStepItem(
                    stepNumber: 2,
                    title: "Intensive Course",
                    description: "Khóa học tăng tốc được cá nhân hóa theo điểm yếu kỹ năng (Listening, Reading, Writing, Speaking).",
                    iconClass: "bi bi-lightning-charge-fill",
                    tag: "Bước 2"
                ),
                new StudyStepItem(
                    stepNumber: 3,
                    title: "Global Classrooms",
                    description: "Lớp học trực tuyến tương tác cao cùng giáo viên bản xứ và bạn bè quốc tế trên hơn 120 quốc gia.",
                    iconClass: "bi bi-globe-americas",
                    tag: "Bước 3"
                ),
                new StudyStepItem(
                    stepNumber: 4,
                    title: "Free IELTS Mock Test",
                    description: "Thư viện đề thi thử miễn phí cập nhật liên tục từ đề thi thật Cambridge và IDP/British Council.",
                    iconClass: "bi bi-journal-text",
                    tag: "Bước 4"
                ),
                new StudyStepItem(
                    stepNumber: 5,
                    title: "Simulation Mock Exam",
                    description: "Hệ thống giả lập thi thật trên máy tính (CDI), được giáo viên trung tâm chấm điểm Writing & Speaking chi tiết.",
                    iconClass: "bi bi-laptop-fill",
                    tag: "Bước 5"
                ),
                new StudyStepItem(
                    stepNumber: 6,
                    title: "Unlock Full Services",
                    description: "Mở khóa toàn bộ dịch vụ chấm bài chi tiết từ cựu giám khảo, phân tích số liệu và hỗ trợ du học.",
                    iconClass: "bi bi-key-fill",
                    tag: "Bước 6"
                )
            };
        }

        private void InitLatestMaterials()
        {
            LatestMaterials = new List<IeltsMaterialItem>
            {
                new IeltsMaterialItem(
                    title: "IELTS Mock Test 2026 January",
                    category: "Academic",
                    testCount: 4,
                    durationMinutes: 160,
                    viewsCount: 142500,
                    rating: 4.9,
                    badgeText: "HOT 2026",
                    releaseMonth: "January 2026"
                ),
                new IeltsMaterialItem(
                    title: "IELTS Mock Test 2025 December",
                    category: "Academic",
                    testCount: 4,
                    durationMinutes: 160,
                    viewsCount: 285400,
                    rating: 4.8,
                    badgeText: "POPULAR",
                    releaseMonth: "December 2025"
                ),
                new IeltsMaterialItem(
                    title: "IELTS General Training Mock 2026",
                    category: "General Training",
                    testCount: 4,
                    durationMinutes: 175,
                    viewsCount: 96800,
                    rating: 4.9,
                    badgeText: "NEW",
                    releaseMonth: "January 2026"
                ),
                new IeltsMaterialItem(
                    title: "Cambridge IELTS 19 Simulation Test",
                    category: "Academic",
                    testCount: 4,
                    durationMinutes: 160,
                    viewsCount: 312000,
                    rating: 5.0,
                    badgeText: "FEATURED",
                    releaseMonth: "Official Cambridge"
                )
            };
        }

        private void InitCoursesAndRoadmap()
        {
            Courses = new List<IeltsCourseItem>
            {
                new IeltsCourseItem(
                    title: "IELTS Masterclass Foundation to 6.5+",
                    instructor: "Designed by Ex-Examiner Michael Brown",
                    level: "Band 5.0 -> 6.5+",
                    lessonsCount: 48,
                    rating: 4.9,
                    originalPrice: "4.500.000đ",
                    salePrice: "2.890.000đ",
                    isBestSeller: true
                ),
                new IeltsCourseItem(
                    title: "IELTS Writing & Speaking Intensive 7.5+",
                    instructor: "Ex-British Council Senior Examiner",
                    level: "Band 6.5 -> 7.5+",
                    lessonsCount: 36,
                    rating: 5.0,
                    originalPrice: "5.200.000đ",
                    salePrice: "3.490.000đ",
                    isBestSeller: false
                ),
                new IeltsCourseItem(
                    title: "Complete Computer-Delivered IELTS Mastery",
                    instructor: "Master Trainer Sarah Jenkins (Band 9.0)",
                    level: "All Levels (Academic & GT)",
                    lessonsCount: 32,
                    rating: 4.8,
                    originalPrice: "3.800.000đ",
                    salePrice: "2.190.000đ",
                    isBestSeller: false
                )
            };

            RoadmapMilestones = new List<RoadmapMilestone>
            {
                new RoadmapMilestone("Giai đoạn 1", "Band 4.5 - 5.5", "Xây dựng nền tảng ngữ pháp, phát âm và từ vựng học thuật cơ bản"),
                new RoadmapMilestone("Giai đoạn 2", "Band 5.5 - 6.5", "Luyện phản xạ 4 kỹ năng, làm quen cấu trúc đề thi Academic/GT"),
                new RoadmapMilestone("Giai đoạn 3", "Band 6.5 - 7.5+", "Chiến thuật xử lý câu hỏi khó, sửa lỗi chi tiết cùng cựu giám khảo"),
                new RoadmapMilestone("Giai đoạn 4", "Band 8.0+", "Hoàn thiện phong cách ngôn ngữ tự nhiên, phản biện chuyên sâu")
            };
        }

        private void InitLiveLessons()
        {
            LiveLessons = new List<LiveLessonItem>
            {
                new LiveLessonItem(
                    topic: "Academic Writing Task 2: Advanced Cohesion & Coherence for Band 8.0",
                    speaker: "Dr. Ngo Dinh (Ex-Examiner, 15+ năm kinh nghiệm)",
                    dateTimeText: "Thứ Năm, 20:00 - 21:30 (GMT+7)",
                    skill: "Writing",
                    status: "Sắp diễn ra",
                    registeredCount: 840
                ),
                new LiveLessonItem(
                    topic: "Speaking Part 2 & 3: Master Idiomatic Expressions & Fluency",
                    speaker: "Ms. Hoang Hang (IELTS 9.0, CELTA Cambridge)",
                    dateTimeText: "Thứ Bảy, 15:00 - 16:30 (GMT+7)",
                    skill: "Speaking",
                    status: "Miễn phí",
                    registeredCount: 1250
                ),
                new LiveLessonItem(
                    topic: "Listening Section 4: Tackling Complex Academic Lectures & Notes",
                    speaker: "Mr. Thanh Nam (Head of Training, BC Partner)",
                    dateTimeText: "Chủ Nhật, 19:30 - 21:00 (GMT+7)",
                    skill: "Listening",
                    status: "Sắp mở",
                    registeredCount: 920
                )
            };
        }

        private void InitSocialTestimonials()
        {
            SocialTestimonials = new List<SocialTestimonialItem>
            {
                new SocialTestimonialItem(
                    studentName: "Hoàng Thu Hân",
                    bandAchieved: "Band 8.0 Overall",
                    platform: "Facebook",
                    platformIcon: "bi bi-facebook",
                    avatarUrl: "https://images.unsplash.com/photo-1534528741775-53994a69daeb?auto=format&fit=crop&w=150&q=80",
                    comment: "Review chân thực sau 3 tháng luyện đề trên IELTS Online Tests: Giao diện thi thử chuẩn 100% kỳ thi trên máy tính tại BC. Bài Speaking và Writing được thầy cô chấm chữa cực kỳ sát và chi tiết!",
                    likesCount: 1420
                ),
                new SocialTestimonialItem(
                    studentName: "Nguyễn Thành Lâm",
                    bandAchieved: "Band 7.5 Overall",
                    platform: "TikTok",
                    platformIcon: "bi bi-tiktok",
                    avatarUrl: "https://images.unsplash.com/photo-1539571696357-5a69c17a67c6?auto=format&fit=crop&w=150&q=80",
                    comment: "Cách phân tích lỗi sai trong phần Reading và giải thích đáp án chi tiết là cứu tinh của mình! Từ 5.5 nhảy vọt lên 7.5 chỉ sau 1 khóa học với Ex-Examiner.",
                    likesCount: 3890
                ),
                new SocialTestimonialItem(
                    studentName: "Ngô Hoàng Long",
                    bandAchieved: "Band 8.5 Listening & Reading",
                    platform: "YouTube",
                    platformIcon: "bi bi-youtube",
                    avatarUrl: "https://images.unsplash.com/photo-1517841905240-472988babdf9?auto=format&fit=crop&w=150&q=80",
                    comment: "Video livestream Live Lessons của thầy David rất chất lượng! Cảm ơn nền tảng đã cho học sinh Việt Nam cơ hội tiếp cận kho đề thi chuẩn quốc tế miễn phí.",
                    likesCount: 2150
                )
            };
        }

        private void InitTeachingTeam()
        {
            TeachingTeam = new List<TeacherExpertItem>
            {
                new TeacherExpertItem(
                    name: "Dr. Ngô Hoàng Đỉnh",
                    role: "Former IELTS Examiner (IDP & BC)",
                    score: "IELTS 9.0 Overall",
                    bio: "Hơn 18 năm kinh nghiệm khảo thí và huấn luyện hơn 25.000 học viên đạt band 7.0+ trên toàn cầu.",
                    avatarUrl: "https://images.unsplash.com/photo-1560250097-0b93528c311a?auto=format&fit=crop&w=250&q=80"
                ),
                new TeacherExpertItem(
                    name: "Ms. Hoàng Thu Hằng",
                    role: "Senior Academic Director",
                    score: "IELTS 9.0 Speaking & Writing",
                    bio: "Chuyên gia đào tạo sư phạm Cambridge, tác giả các bộ sách giải đề IELTS Actual Tests bán chạy nhất.",
                    avatarUrl: "https://images.unsplash.com/photo-1573496359142-b8d87734a5a2?auto=format&fit=crop&w=250&q=80"
                ),
                new TeacherExpertItem(
                    name: "Dr. Nguyễn Thành Nam",
                    role: "Head of English Assessment",
                    score: "IELTS 9.0 Overall",
                    bio: "Tiến sĩ Ngôn ngữ ứng dụng ĐH Southampton, cựu giám khảo chấm thi phần thi Nói và Viết tại London.",
                    avatarUrl: "https://images.unsplash.com/photo-1500648767791-00dcc994a43e?auto=format&fit=crop&w=250&q=80"
                ),
                new TeacherExpertItem(
                    name: "Ms. Lê Hoàng Yến",
                    role: "Lead Speaking Coach",
                    score: "IELTS 8.5 Overall (Speaking 9.0)",
                    bio: "Thạc sĩ TESOL ĐH Sydney, người truyền cảm hứng cho hàng ngàn học viên tự tin bắn tiếng Anh lưu loát.",
                    avatarUrl: "https://images.unsplash.com/photo-1580489944761-15a19d654956?auto=format&fit=crop&w=250&q=80"
                )
            };
        }

        private void InitPartners()
        {
            Partners = new List<PartnerItem>
            {
                new PartnerItem("British Council", "Official Platinum Partner", "bi bi-shield-check"),
                new PartnerItem("IDP Education", "Global Test Partner", "bi bi-award"),
                new PartnerItem("University of Oxford", "Academic Recognition", "bi bi-building"),
                new PartnerItem("University of Southampton", "Partner Institution", "bi bi-bank"),
                new PartnerItem("Newcastle University", "Direct Articulation", "bi bi-mortarboard-fill"),
                new PartnerItem("Cambridge University Press", "Resource Provider", "bi bi-book-half")
            };
        }

        private void InitFaqs()
        {
            Faqs = new List<FaqItem>
            {
                new FaqItem(
                    question: "Các bài thi thử trên LUMORA English Center có hoàn toàn miễn phí không?",
                    answer: "Chỉ miễn phí với các bài test LR. Với 100% kho đề thi thử IELTS Mock Tests (Academic và General Training) trên website đều được mở miễn phí cho mọi thí sinh ôn luyện không giới hạn số lần làm bài."
                ),
                new FaqItem(
                    question: "Đề thi thử tại đây sát với đề thi thật trên máy tính (Computer-delivered) như thế nào?",
                    answer: "Hệ thống mô phỏng chính xác giao diện, thanh công cụ highlight, bộ đếm giờ, dạng câu hỏi và âm thanh bài nghe chuẩn format kỳ thi IELTS chính thức của IDP và British Council."
                ),
                new FaqItem(
                    question: "Hoạt động chấm điểm Writing và Speaking hoạt động ra sao?",
                    answer: "Giám khảo, giảng viên hàng đầu có kinh nghiệm giảng dạy phong phú của trung tâm sẽ đánh giá chi tiết theo 4 tiêu chuẩn chấm điểm chính thức: Lexical Resource, Grammatical Range, Coherence & Fluency."
                ),
                new FaqItem(
                    question: "Làm thế nào để đăng ký tham gia các buổi Live Lessons miễn phí?",
                    answer: "Bạn chỉ cần chọn buổi học trong mục 'Live Lessons', bấm nút 'REGISTER' và nhận link lớp học trực tuyến qua email kèm tài liệu trước buổi học."
                ),
                new FaqItem(
                    question: "Tôi có được cấp chứng nhận sau khi hoàn thành khóa học không?",
                    answer: "Các học viên hoàn thành các khóa học IELTS Mastery sẽ được cấp chứng chỉ điện tử có mã xác thực quốc tế, công nhận quá trình rèn luyện năng lực."
                )
            };
        }
    }

    // ========================================================================
    // CÁC LỚP ĐỐI TƯỢNG HỖ TRỢ THEO NGUYÊN LÝ OOP 
    // ========================================================================

    public class StudyStepItem
    {
        public int StepNumber { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string IconClass { get; set; }
        public string Tag { get; set; }

        public StudyStepItem(int stepNumber, string title, string description, string iconClass, string tag)
        {
            StepNumber = stepNumber;
            Title = title;
            Description = description;
            IconClass = iconClass;
            Tag = tag;
        }
    }

    public class IeltsMaterialItem
    {
        public string Title { get; set; }
        public string Category { get; set; }
        public int TestCount { get; set; }
        public int DurationMinutes { get; set; }
        public int ViewsCount { get; set; }
        public double Rating { get; set; }
        public string BadgeText { get; set; }
        public string ReleaseMonth { get; set; }

        public IeltsMaterialItem(string title, string category, int testCount, int durationMinutes, int viewsCount, double rating, string badgeText, string releaseMonth)
        {
            Title = title;
            Category = category;
            TestCount = testCount;
            DurationMinutes = durationMinutes;
            ViewsCount = viewsCount;
            Rating = rating;
            BadgeText = badgeText;
            ReleaseMonth = releaseMonth;
        }
    }

    public class IeltsCourseItem
    {
        public string Title { get; set; }
        public string Instructor { get; set; }
        public string Level { get; set; }
        public int LessonsCount { get; set; }
        public double Rating { get; set; }
        public string OriginalPrice { get; set; }
        public string SalePrice { get; set; }
        public bool IsBestSeller { get; set; }

        public IeltsCourseItem(string title, string instructor, string level, int lessonsCount, double rating, string originalPrice, string salePrice, bool isBestSeller)
        {
            Title = title;
            Instructor = instructor;
            Level = level;
            LessonsCount = lessonsCount;
            Rating = rating;
            OriginalPrice = originalPrice;
            SalePrice = salePrice;
            IsBestSeller = isBestSeller;
        }
    }

    public class RoadmapMilestone
    {
        public string Stage { get; set; }
        public string TargetBand { get; set; }
        public string Description { get; set; }

        public RoadmapMilestone(string stage, string targetBand, string description)
        {
            Stage = stage;
            TargetBand = targetBand;
            Description = description;
        }
    }

    public class LiveLessonItem
    {
        public string Topic { get; set; }
        public string Speaker { get; set; }
        public string DateTimeText { get; set; }
        public string Skill { get; set; }
        public string Status { get; set; }
        public int RegisteredCount { get; set; }

        public LiveLessonItem(string topic, string speaker, string dateTimeText, string skill, string status, int registeredCount)
        {
            Topic = topic;
            Speaker = speaker;
            DateTimeText = dateTimeText;
            Skill = skill;
            Status = status;
            RegisteredCount = registeredCount;
        }
    }

    public class SocialTestimonialItem
    {
        public string StudentName { get; set; }
        public string BandAchieved { get; set; }
        public string Platform { get; set; }
        public string PlatformIcon { get; set; }
        public string AvatarUrl { get; set; }
        public string Comment { get; set; }
        public int LikesCount { get; set; }

        public SocialTestimonialItem(string studentName, string bandAchieved, string platform, string platformIcon, string avatarUrl, string comment, int likesCount)
        {
            StudentName = studentName;
            BandAchieved = bandAchieved;
            Platform = platform;
            PlatformIcon = platformIcon;
            AvatarUrl = avatarUrl;
            Comment = comment;
            LikesCount = likesCount;
        }
    }

    public class TeacherExpertItem
    {
        public string Name { get; set; }
        public string Role { get; set; }
        public string Score { get; set; }
        public string Bio { get; set; }
        public string AvatarUrl { get; set; }

        public TeacherExpertItem(string name, string role, string score, string bio, string avatarUrl)
        {
            Name = name;
            Role = role;
            Score = score;
            Bio = bio;
            AvatarUrl = avatarUrl;
        }
    }

    public class PartnerItem
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string IconClass { get; set; }

        public PartnerItem(string name, string description, string iconClass)
        {
            Name = name;
            Description = description;
            IconClass = iconClass;
        }
    }

    public class FaqItem
    {
        public string Question { get; set; }
        public string Answer { get; set; }

        public FaqItem(string question, string answer)
        {
            Question = question;
            Answer = answer;
        }
    }

    /// <summary>
    /// Model xác thực dữ liệu cho form liên hệ tư vấn (OOP Form Model)
    /// </summary>
    public class ContactConsultationModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ và tên của bạn")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ email")]
        [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại liên hệ")]
        [Phone(ErrorMessage = "Số điện thoại không đúng định dạng")]
        [Display(Name = "Số điện thoại")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Display(Name = "Mục tiêu band điểm")]
        public string TargetBand { get; set; } = "7.0+";

        [Display(Name = "Lời nhắn / Nhu cầu tư vấn")]
        public string Message { get; set; } = string.Empty;
    }
}
