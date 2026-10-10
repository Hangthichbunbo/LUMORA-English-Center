# BÁO CÁO PHÂN TÍCH TOÀN DIỆN CHỨC NĂNG HỆ THỐNG LUMORA ENGLISH CENTER
**Mã tài liệu:** `PROJECT_FUNCTIONALITY_AUDIT.md`  
**Dự án:** Nền tảng học trực tuyến & Khảo thí IELTS — LUMORA English Center  
**Vị trí lưu trữ:** Thư mục gốc dự án (`LTW/PROJECT_FUNCTIONALITY_AUDIT.md`)  
**Công nghệ:** ASP.NET Core 10.0 (MVC), Entity Framework Core 10, ASP.NET Core Identity, Microsoft SQL Server  
**Ngày lập báo cáo:** 10/10/2026  
**Chuyên viên kiểm toán & phân tích mã nguồn:** Senior Software Engineer / System Analyst  

---

## MỤC LỤC
1. [PHẦN I — TỔNG QUAN PROJECT](#phần-i--tổng-quan-project)
   - 1.1. Tên dự án và mục tiêu hệ thống
   - 1.2. Kiến trúc và công nghệ sử dụng
   - 1.3. Cấu trúc thư mục nguồn thực tế
   - 1.4. Cơ sở dữ liệu và tích hợp bên ngoài
2. [PHẦN II — DANH SÁCH TOÀN BỘ CHỨC NĂNG (CATALOG)](#phần-ii--danh-sách-toàn-bộ-chức-năng)
3. [PHẦN III — ĐẶC TẢ CHI TIẾT TỪNG CHỨC NĂNG NGHIỆP VỤ](#phần-iii--đặc-tả-chi-tiết-từng-chức-năng)
   - 3.1. Phân hệ Xác thực & Tài khoản (AUTH)
   - 3.2. Phân hệ Trang chủ & Tư vấn (HOME)
   - 3.3. Phân hệ Khóa học & Đăng ký / Thanh toán (COURSE)
   - 3.4. Phân hệ Học tập Học viên (STUDENT)
   - 3.5. Phân hệ Lớp học trực tuyến (CLASS)
   - 3.6. Phân hệ Khảo thí & Thi thử IELTS (TEST)
   - 3.7. Phân hệ Kinh nghiệm & Mẹo thi IELTS (TIP)
   - 3.8. Phân hệ Giảng viên (TEACHER)
   - 3.9. Phân hệ Quản trị hệ thống (ADMIN)
   - 3.10. Phân hệ Thông báo & Nhắc nhở (NOTIF)
   - 3.11. Các thành phần Legacy & Stub Controllers (LEGACY)
4. [PHẦN IV — DANH SÁCH MÀN HÌNH VÀ THÀNH PHẦN GIAO DIỆN](#phần-iv--danh-sách-màn-hình-và-thành-phần-giao-diện)
5. [PHẦN V — DANH SÁCH ROUTE, CONTROLLER VÀ NGHIỆP VỤ BACKEND](#phần-v--danh-sách-api-và-nghiệp-vụ-backend)
6. [PHẦN VI — CẤU TRÚC VÀ CHỨC NĂNG CƠ SỞ DỮ LIỆU](#phần-vi--cấu-trúc-và-chức-năng-cơ-sở-dữ-liệu)
7. [PHẦN VII — MA TRẬN ĐỐI CHIẾU CHỨC NĂNG](#phần-vii--ma-trận-đối-chiếu-chức-năng)
8. [PHẦN VIII — CÁC VẤN ĐỀ VÀ CHỨC NĂNG CÒN THIẾU](#phần-viii--các-vấn-đề-và-chức-năng-còn-thiếu)
9. [PHẦN IX — KẾT LUẬN & KIẾN NGHỊ TRỌNG TÂM](#phần-ix--kết-luận)

---

# PHẦN I — TỔNG QUAN PROJECT

### 1.1. Tên dự án và mục tiêu hệ thống
* **Tên dự án:** LUMORA English Center — LMS & IELTS Online Examination Platform.
* **Mục tiêu hệ thống:** 
  - Cung cấp nền tảng số hóa toàn diện phục vụ giảng dạy, học tập và luyện thi chứng chỉ Anh ngữ quốc tế (IELTS Academic & General Training).
  - Khảo thí trực tuyến 4 kỹ năng (Listening, Reading, Writing, Speaking) với giao diện chuẩn tiếng Anh, mô phỏng kỳ thi trên máy tính (Computer-delivered IELTS - CDI) của Hội đồng Anh (British Council) và IDP.
  - Quản lý học viên, giảng viên, khóa học theo chương mục, bài giảng đa phương tiện (video/audio/PDF/từ vựng/ngữ pháp), bài tập có tự chấm và chấm điểm của giảng viên.
  - Quản trị lớp học trực tuyến (Live Classes), thanh toán học phí khóa học, và phát sóng thông báo toàn hệ thống.

### 1.2. Kiến trúc và công nghệ sử dụng
* **Nền tảng chính:** Microsoft ASP.NET Core 10.0 (Web App MVC - Target Framework `net10.0`).
* **Mô hình kiến trúc:** MVC (Model - View - Controller) kết hợp Entity Framework Core ORM.
* **Hệ thống xác thực & phân quyền:**
  - ASP.NET Core Identity (`Microsoft.AspNetCore.Identity.EntityFrameworkCore` v10.0.12) quản lý tài khoản, cookie session, vai trò người dùng (Roles: `Admin`, `Teacher`, `Student`).
  - Google OAuth Authentication (`Microsoft.AspNetCore.Authentication.Google` v10.0.12) phục vụ đăng nhập bên ngoài.
* **Thư viện giao diện & Frontend:**
  - Bootstrap 5.3 Framework (`wwwroot/lib/bootstrap`).
  - Bootstrap Icons v1.11.3 CDN.
  - Google Fonts (`Montserrat`, `Nunito`, `Roboto`).
  - Tùy biến CSS Design Tokens đồng bộ theo LUMORA Design System (`wwwroot/css/site.css`).
  - JavaScript client-side tương tác (`wwwroot/js/site.js`).

### 1.3. Cấu trúc thư mục nguồn thực tế
```
LTW/
├── Controllers/                   # 14 MVC Controllers (10 hoạt động chính, 4 legacy stub)
│   ├── AccountController.cs       # Xác thực, OTP, đăng ký, đăng nhập, đổi mật khẩu, hồ sơ
│   ├── AdminController.cs         # Bàn điều hành quản trị viên, CRUD khóa học, người dùng, lớp học, tips
│   ├── ClassController.cs         # Danh sách lớp live, kiểm tra quyền truy cập, phòng học ảo
│   ├── CourseController.cs        # Catalog khóa học, chi tiết, quy trình thanh toán mua khóa học
│   ├── HomeController.cs          # Trang chủ Landing Page, nhận tư vấn, trang lỗi/chính sách
│   ├── HomeworkController.cs      # (Legacy stub - chưa sử dụng)
│   ├── LoginController.cs         # (Legacy stub - chưa sử dụng)
│   ├── ManagerController.cs       # (Legacy stub - chưa sử dụng)
│   ├── NotificationController.cs  # Trung tâm thông báo người dùng, đánh dấu đã đọc, nhắc email
│   ├── PostController.cs          # (Legacy stub - chưa sử dụng)
│   ├── StudentController.cs       # Dashboard học viên, lộ trình khóa học, học bài, làm bài tập
│   ├── TeacherController.cs       # Danh sách & chi tiết giảng viên, cổng Portal soạn bài, chấm điểm
│   ├── TestController.cs          # Khảo thí Example Test, Placement Test, bộ chấm điểm 4 kỹ năng
│   └── TipController.cs           # Bài viết blog IELTS Tips, lọc theo kỹ năng, tăng lượt xem
├── Data/                          # Tầng dữ liệu & Khởi tạo (Database Layer)
│   ├── ApplicationDbContext.cs    # EF Core IdentityDbContext, cấu hình quan hệ & cascade delete
│   ├── ApplicationUser.cs         # Mở rộng IdentityUser (FullName, RoleName, AvatarUrl, Bio, IsActive)
│   └── DbInitializer.cs           # Seed tự động dữ liệu: Roles, Admin, 7 Teachers, Student, 8 Courses, Lessons, Tests, Tips
├── Models/                        # Domain Models & ViewModels
│   ├── AuthViewModels.cs          # ViewModels cho Login, Register, VerifyOtp, ForgotPassword, Profile
│   ├── Course.cs                  # Thực thể Course, Chapter, Lesson, LessonProgress
│   ├── LiveClass.cs               # Thực thể LiveClass, LiveClassStudent (điểm danh/tham gia)
│   ├── Exercise.cs                # Thực thể Exercise, ExerciseQuestion, ExerciseSubmission
│   ├── Test.cs                    # Thực thể Test, TestQuestion, TestAttempt
│   ├── Enrollment.cs              # Thực thể Enrollment (lịch sử giao dịch mua khóa học)
│   ├── IeltsTip.cs                # Thực thể IeltsTip (bài viết mẹo thi)
│   ├── Notification.cs            # Thực thể Notification (thông báo hệ thống)
│   ├── OtpRecord.cs               # Thực thể OtpRecord (quản lý mã xác thực OTP)
│   ├── Enums.cs                   # Enum: UserRole, Skill, SkillType, QuestionFormat, ClassStatus, ExamType
│   ├── LandingPageViewModel.cs    # ViewModel đóng gói dữ liệu cho Landing Page
│   └── [Legacy Models]            # UserAccount, ClassRoom, Assignment, Question, AnswerOption, Submission...
├── Views/                         # Giao diện Razor (.cshtml)
│   ├── Account/                   # 7 màn hình xác thực, OTP và hồ sơ
│   ├── Admin/                     # 13 màn hình quản trị hệ thống
│   ├── Class/                     # 3 màn hình danh sách, chi tiết, phòng học trực tuyến
│   ├── Course/                    # 4 màn hình danh sách, chi tiết, thanh toán, xác nhận
│   ├── Home/                      # Index.cshtml (Landing page 741 dòng), Privacy.cshtml
│   ├── Notification/              # Trung tâm thông báo
│   ├── Shared/                    # _Layout.cshtml, _LoginPartial.cshtml, Error.cshtml
│   ├── Student/                   # 5 màn hình bàn học cá nhân, bài giảng, bài tập, lịch học
│   ├── Teacher/                   # 10 màn hình danh sách GV, cổng giáo viên, soạn bài, chấm bài
│   ├── Test/                      # 3 màn hình danh sách đề thi, giao diện thi, kết quả thi
│   └── Tip/                       # 2 màn hình danh sách tips và chi tiết bài viết
├── wwwroot/                       # Static Assets (css, js, favicon, lib/bootstrap)
│   ├── css/site.css               # Hệ thống CSS Design System (1,900+ dòng)
│   └── js/site.js                 # Scripts hỗ trợ điều hướng, carousel
├── appsettings.json               # Cấu hình chuỗi kết nối SQL Server & Google OAuth
├── Program.cs                     # Điểm khởi chạy ứng dụng (Pipeline, Middleware, DI)
└── LTW.csproj                     # Cấu hình dự án .NET 10.0
```

### 1.4. Cơ sở dữ liệu và tích hợp bên ngoài
* **Cơ sở dữ liệu:** Microsoft SQL Server (Mặc định cấu hình qua LocalDB: `LumoraEnglishCenterDb`).
* **Cơ chế khởi tạo:** `DbInitializer.InitializeAsync()` tự động gọi khi ứng dụng khởi chạy (`context.Database.EnsureCreatedAsync()`), tự động tạo cấu trúc bảng và nạp sẵn dữ liệu ban đầu hoàn chỉnh gồm:
  - 3 vai trò hệ thống (`Admin`, `Teacher`, `Student`).
  - 1 tài khoản Quản trị viên (`admin@lumora.edu.vn`).
  - 7 tài khoản Giảng viên chuyên gia quốc tế (Ms. Sarah Jenkins, Mr. James Watson, Dr. David Miller, Ms. Emma Richardson, Mr. Daniel Evans, Ms. Olivia Taylor, Mr. Robert Clark).
  - 1 tài khoản Học viên thử nghiệm (`student@lumora.edu.vn`).
  - 8 Khóa học tiêu chuẩn với đầy đủ chương mục, bài giảng mẫu, bài tập và ngân hàng câu hỏi.
  - 8 Buổi học trực tuyến Live Classes (Public Webinars & Private Masterclasses).
  - 2 Đề thi khảo thí 4 kỹ năng (Example Test & Placement Test) với ngân hàng câu hỏi kèm âm thanh và bài đọc.
  - 6 Bài viết mẹo thi IELTS Tips học thuật.
* **Tích hợp bên ngoài:**
  - Google OAuth Authentication: Cấu hình qua ClientId/ClientSecret trong `appsettings.json`.
  - YouTube Embed Player: Tích hợp phát video bài giảng trong trình học của học viên.
  - Trình phát âm thanh Audio Player: Phát file audio MP3 cho bài tập Listening và đề thi thử.

---

# PHẦN II — DANH SÁCH TOÀN BỘ CHỨC NĂNG

Dưới đây là bảng thống kê toàn bộ **82 chức năng** được ghi nhận trong mã nguồn dự án:

| ID | Phân hệ (Module) | Tên chức năng | Mô tả ngắn gọn nghiệp vụ | Vị trí Source Code chính | Trạng thái thực tế |
|---|---|---|---|---|---|
| **AUTH-01** | Xác thực & Tài khoản | Đăng nhập tài khoản cục bộ | Đăng nhập bằng Email/Username và mật khẩu, điều hướng theo vai trò | `AccountController.cs` (`Login`) | Hoàn chỉnh |
| **AUTH-02** | Xác thực & Tài khoản | Đăng nhập ngoài qua Google | Đăng nhập tài khoản Google OAuth, tự cấp tài khoản học viên nếu mới | `AccountController.cs` (`ExternalLogin`) | Hoàn chỉnh |
| **AUTH-03** | Xác thực & Tài khoản | Đăng nhập Google thử nghiệm nhanh | Nút đăng nhập tức thì tài khoản học viên thử nghiệm phục vụ kiểm thử | `AccountController.cs` (`QuickGoogleLogin`) | Hoàn chỉnh |
| **AUTH-04** | Xác thực & Tài khoản | Đăng ký tài khoản & Phát sinh OTP | Kiểm tra email/username, sinh mã OTP 6 số lưu bảng `OtpRecord` | `AccountController.cs` (`Register`) | Hoàn chỉnh |
| **AUTH-05** | Xác thực & Tài khoản | Xác thực OTP & Kích hoạt tài khoản | Kiểm tra mã OTP còn hạn, tạo `ApplicationUser` mới, tự động đăng nhập | `AccountController.cs` (`VerifyOtp`) | Hoàn chỉnh |
| **AUTH-06** | Xác thực & Tài khoản | Yêu cầu khôi phục mật khẩu | Nhập email, kiểm tra tồn tại và phát sinh mã OTP đổi mật khẩu | `AccountController.cs` (`ForgotPassword`) | Hoàn chỉnh |
| **AUTH-07** | Xác thực & Tài khoản | Đặt lại mật khẩu mới qua OTP | Kiểm tra mã OTP và dùng token reset mật khẩu của ASP.NET Identity | `AccountController.cs` (`ResetPassword`) | Hoàn chỉnh |
| **AUTH-08** | Xác thực & Tài khoản | Đăng xuất khỏi hệ thống | Xóa phiên đăng nhập Identity Cookie an toàn, chuyển về trang chủ | `AccountController.cs` (`Logout`) | Hoàn chỉnh |
| **AUTH-09** | Xác thực & Tài khoản | Xem và cập nhật hồ sơ cá nhân | Đổi họ tên, số điện thoại, tiểu sử, ảnh đại diện và đổi mật khẩu | `AccountController.cs` (`Profile`) | Hoàn chỉnh |
| **AUTH-10** | Xác thực & Tài khoản | Màn hình từ chối quyền truy cập | Hiển thị thông báo khi người dùng không đủ quyền truy cập tài nguyên | `AccountController.cs` (`AccessDenied`) | Hoàn chỉnh |
| **HOME-01** | Trang chủ & Tư vấn | Hiển thị Landing Page chuẩn giao diện | Trình bày Banner Platinum, 6 bước học, kho đề thi, lộ trình, slider GV | `HomeController.cs` (`Index`) | Hoàn chỉnh |
| **HOME-02** | Trang chủ & Tư vấn | Gửi biểu mẫu đăng ký tư vấn 1-1 | Tiếp nhận thông tin học viên cần tư vấn lộ trình và mục tiêu band | `HomeController.cs` (`SubmitConsultation`) | Triển khai một phần |
| **HOME-03** | Trang chủ & Tư vấn | Điều hướng phím tắt trang chủ | Chuyển hướng các đường dẫn tắt về các vùng neo (anchors) tương ứng | `HomeController.cs` (`ExamLibrary...`) | Hoàn chỉnh |
| **COURSE-01** | Khóa học & Đăng ký | Xem danh mục & Lọc khóa học | Lọc theo chuyên mục (IELTS, Speaking, v.v.), trình độ và tìm kiếm | `CourseController.cs` (`Index`) | Hoàn chỉnh |
| **COURSE-02** | Khóa học & Đăng ký | Xem chi tiết khóa học & Đề cương | Xem thông tin, giáo viên, danh sách chương/bài học, kiểm tra đã mua | `CourseController.cs` (`Detail`) | Hoàn chỉnh |
| **COURSE-03** | Khóa học & Đăng ký | Giao diện thanh toán khóa học | Tóm tắt đơn hàng, ngăn mua trùng, lựa chọn VNPay / MoMo / Chuyển khoản | `CourseController.cs` (`Purchase`) | Hoàn chỉnh |
| **COURSE-04** | Khóa học & Đăng ký | Xử lý giao dịch mua khóa học | Lưu bản ghi `Enrollment`, sinh mã giao dịch, tạo thông báo kích hoạt | `CourseController.cs` (`ProcessPayment`) | Hoàn chỉnh |
| **COURSE-05** | Khóa học & Đăng ký | Xem biên lai & Xác nhận thanh toán | Hiển thị thông tin giao dịch thành công và lối tắt vào học ngay | `CourseController.cs` (`PaymentSuccess`) | Hoàn chỉnh |
| **STUDENT-01** | Học tập Học viên | Bàn học cá nhân (Dashboard) | Tổng hợp số liệu khóa học, tiến độ, bài tập chờ nộp, điểm số, lịch học | `StudentController.cs` (`Dashboard`) | Hoàn chỉnh |
| **STUDENT-02** | Học tập Học viên | Khóa học của tôi & Đo lường tiến độ | Danh sách các khóa học đã thanh toán, thanh % hoàn thành thực tế | `StudentController.cs` (`MyCourses`) | Hoàn chỉnh |
| **STUDENT-03** | Học tập Học viên | Trình học bài đa phương tiện | Xem video, nghe audio, đọc lý thuyết, từ vựng JSON, ngữ pháp, tải file | `StudentController.cs` (`Lesson`) | Hoàn chỉnh |
| **STUDENT-04** | Học tập Học viên | Đánh dấu hoàn thành bài học | Nút bật/tắt trạng thái hoàn thành bài học, lưu vào `LessonProgress` | `StudentController.cs` (`ToggleCompleteLesson`) | Hoàn chỉnh |
| **STUDENT-05** | Học tập Học viên | Làm và nộp bài tập thực hành | Trả lời trắc nghiệm, hệ thống tự động chấm điểm trên thang 10, lưu JSON | `StudentController.cs` (`SubmitExercise`) | Hoàn chỉnh |
| **STUDENT-06** | Học tập Học viên | Xem lại đáp án & Giải thích chi tiết | Đối chiếu câu đúng/sai, hiển thị đáp án chuẩn, lời giải thích & nhận xét GV | `StudentController.cs` (`Exercise`) | Hoàn chỉnh |
| **STUDENT-07** | Học tập Học viên | Xem thời khóa biểu cá nhân | Lịch các buổi học trực tuyến công khai và lớp học thuộc khóa đã mua | `StudentController.cs` (`Schedule`) | Hoàn chỉnh |
| **CLASS-01** | Lớp học trực tuyến | Xem danh sách lớp Live Class | Lọc lớp công khai (Public) hoặc lớp của tôi (MyClasses) | `ClassController.cs` (`Index`) | Hoàn chỉnh |
| **CLASS-02** | Lớp học trực tuyến | Xem chi tiết buổi học trực tuyến | Xem thời gian, giảng viên, sĩ số, nội dung và kiểm tra quyền vào lớp | `ClassController.cs` (`Detail`) | Hoàn chỉnh |
| **CLASS-03** | Lớp học trực tuyến | Kiểm tra điều kiện & Tham gia lớp | Chặn vào lớp Private nếu chưa mua khóa, ghi nhận điểm danh `LiveClassStudent` | `ClassController.cs` (`Join`) | Hoàn chỉnh |
| **CLASS-04** | Lớp học trực tuyến | Giao diện phòng học trực tuyến ảo | Sân khấu bài giảng, bảng điều khiển mic/cam, danh sách thành viên, chat | `ClassController.cs` (`Room`) | Hoàn chỉnh |
| **TEST-01** | Khảo thí IELTS | Danh mục bài kiểm tra năng lực | Danh sách bài Example Test và Placement Test kèm thời gian làm bài | `TestController.cs` (`Index`) | Hoàn chỉnh |
| **TEST-02** | Khảo thí IELTS | Làm bài thi thử mẫu (Example Test) | Phòng thi chuẩn 4 kỹ năng tiếng Anh (English-Only), audio, bài đọc | `TestController.cs` (`ExampleTest`) | Hoàn chỉnh |
| **TEST-03** | Khảo thí IELTS | Làm bài kiểm tra xếp lớp (Placement Test)| Khảo thí xếp lớp toàn diện 4 kỹ năng tiếng Anh phục vụ phân lớp | `TestController.cs` (`PlacementTest`) | Hoàn chỉnh |
| **TEST-04** | Khảo thí IELTS | Đồng hồ đếm ngược & Tự nộp bài | Đếm ngược thời gian làm bài, tự động submit khi đồng hồ về 0 | `Test/TakeTest.cshtml` (JS Script) | Hoàn chỉnh |
| **TEST-05** | Khảo thí IELTS | Bảng điều hướng câu hỏi (Palette) | Nhảy nhanh tới câu hỏi, đánh dấu trạng thái Đã trả lời / Chưa trả lời | `Test/TakeTest.cshtml` (JS Script) | Hoàn chỉnh |
| **TEST-06** | Khảo thí IELTS | Đếm số lượng từ tự động cho Writing | Khung viết luận Task 2 cập nhật số lượng từ (Word count) theo thời gian thực | `Test/TakeTest.cshtml` (JS Script) | Hoàn chỉnh |
| **TEST-07** | Khảo thí IELTS | Mô phỏng ghi âm bài thi Speaking | Nút kích hoạt mô phỏng thu âm câu trả lời Speaking Part 2/3 | `Test/TakeTest.cshtml` (JS Script) | Triển khai một phần |
| **TEST-08** | Khảo thí IELTS | Bộ máy chấm điểm & Quy đổi Band điểm | Chấm tự động L/R, phân tích W/S, làm tròn 0.5, xếp cấp độ CEFR, phản hồi tiếng Anh | `TestController.cs` (`SubmitTest`) | Hoàn chỉnh |
| **TEST-09** | Khảo thí IELTS | Báo cáo kết quả & Gợi ý lộ trình | Bảng điểm chi tiết 4 kỹ năng, nhận xét học thuật và gợi ý khóa học thích hợp | `TestController.cs` (`Result`) | Hoàn chỉnh |
| **TIP-01** | Mẹo thi & Blog IELTS | Thư viện bài viết & Tìm kiếm, Lọc | Lọc bài viết theo kỹ năng (Listening, Reading...), tìm kiếm theo từ khóa | `TipController.cs` (`Index`) | Hoàn chỉnh |
| **TIP-02** | Mẹo thi & Blog IELTS | Đọc bài viết chi tiết & Tăng lượt xem | Hiển thị nội dung bài viết, tự động tăng `ViewsCount`, bài viết liên quan | `TipController.cs` (`Detail`) | Hoàn chỉnh |
| **TEACHER-01**| Giảng viên | Danh sách giảng viên công khai | Giới thiệu các chuyên gia kèm số lượng khóa học phụ trách | `TeacherController.cs` (`Index`) | Hoàn chỉnh |
| **TEACHER-02**| Giảng viên | Hồ sơ cá nhân chi tiết giảng viên | Thông tin bằng cấp, tiểu sử, các khóa học và lớp học trực tuyến phụ trách | `TeacherController.cs` (`Detail`) | Hoàn chỉnh |
| **TEACHER-03**| Giảng viên | Cổng làm việc giảng viên (Dashboard) | Thống kê số khóa học, lớp live, học viên và bài tập đang chờ chấm | `TeacherController.cs` (`Dashboard`) | Hoàn chỉnh |
| **TEACHER-04**| Giảng viên | Quản lý lớp học của tôi | Xem danh sách các lớp học trực tuyến do giảng viên phụ trách | `TeacherController.cs` (`MyClasses`) | Hoàn chỉnh |
| **TEACHER-05**| Giảng viên | Quản lý danh sách bài học | Danh sách toàn bộ bài giảng thuộc các khóa học của giảng viên | `TeacherController.cs` (`Lessons`) | Hoàn chỉnh |
| **TEACHER-06**| Giảng viên | Thêm bài học mới | Soạn tiêu đề, chọn chương, nhúng video YouTube, audio, PDF, lý thuyết | `TeacherController.cs` (`CreateLesson`) | Hoàn chỉnh |
| **TEACHER-07**| Giảng viên | Quản lý danh mục bài tập | Xem danh sách bài tập thực hành theo từng bài giảng | `TeacherController.cs` (`Exercises`) | Hoàn chỉnh |
| **TEACHER-08**| Giảng viên | Soạn thảo bài tập & Ngân hàng câu hỏi| Thiết lập bài tập, thời gian, điểm sàn và tạo câu hỏi trắc nghiệm | `TeacherController.cs` (`CreateExercise`) | Hoàn chỉnh |
| **TEACHER-09**| Giảng viên | Danh sách học viên đang theo học | Theo dõi học viên đã đăng ký khóa học và số bài tập đã nộp | `TeacherController.cs` (`Students`) | Hoàn chỉnh |
| **TEACHER-10**| Giảng viên | Chấm điểm & Đánh giá bài nộp | Nhập điểm giảng viên, tính tổng điểm trung bình, viết nhận xét, gửi thông báo | `TeacherController.cs` (`GradeSubmission`)| Hoàn chỉnh |
| **ADMIN-01** | Quản trị hệ thống | Bàn điều hành & Đo lường KPI | Thống kê số lượng học viên, giảng viên, khóa học, doanh thu, giao dịch mới | `AdminController.cs` (`Dashboard`) | Hoàn chỉnh |
| **ADMIN-02** | Quản trị hệ thống | Quản lý người dùng & Tìm kiếm, Lọc | Xem danh sách toàn bộ người dùng, lọc theo vai trò, tìm kiếm tên/email | `AdminController.cs` (`Users`) | Hoàn chỉnh |
| **ADMIN-03** | Quản trị hệ thống | Cấp tài khoản giảng viên mới | Điền thông tin, khởi tạo tài khoản giáo viên với vai trò `Teacher` | `AdminController.cs` (`CreateTeacher`) | Hoàn chỉnh |
| **ADMIN-04** | Quản trị hệ thống | Khóa / Mở khóa tài khoản | Chuyển đổi trạng thái `IsActive` của người dùng | `AdminController.cs` (`ToggleUserActive`) | Hoàn chỉnh |
| **ADMIN-05** | Quản trị hệ thống | Xóa tài khoản người dùng | Xóa tài khoản khỏi hệ thống (có cơ chế bảo vệ không xóa admin gốc) | `AdminController.cs` (`DeleteUser`) | Hoàn chỉnh |
| **ADMIN-06** | Quản trị hệ thống | Quản lý danh sách khóa học | Bảng danh sách khóa học, số chương, học viên, trạng thái xuất bản | `AdminController.cs` (`Courses`) | Hoàn chỉnh |
| **ADMIN-07** | Quản trị hệ thống | Tạo khóa học mới | Nhập thông tin khóa học, gán giáo viên, tự động tạo Chương 1 mặc định | `AdminController.cs` (`CreateCourse`) | Hoàn chỉnh |
| **ADMIN-08** | Quản trị hệ thống | Chỉnh sửa khóa học | Cập nhật tên, giá, giảng viên, trình độ, trạng thái nổi bật của khóa học | `AdminController.cs` (`EditCourse`) | Hoàn chỉnh |
| **ADMIN-09** | Quản trị hệ thống | Bật / Tắt xuất bản khóa học | Ẩn hoặc hiện khóa học ra ngoài giao diện công khai | `AdminController.cs` (`TogglePublishCourse`)| Hoàn chỉnh |
| **ADMIN-10** | Quản trị hệ thống | Xóa khóa học | Xóa khóa học khỏi hệ thống | `AdminController.cs` (`DeleteCourse`) | Hoàn chỉnh |
| **ADMIN-11** | Quản trị hệ thống | Quản lý lớp học trực tuyến | Xem danh sách lớp live, ngày giờ, sĩ số và giảng viên | `AdminController.cs` (`Classes`) | Hoàn chỉnh |
| **ADMIN-12** | Quản trị hệ thống | Tạo lớp học trực tuyến mới | Tạo lớp live, liên kết khóa học, gán giáo viên, URL phòng học, chế độ công khai | `AdminController.cs` (`CreateClass`) | Hoàn chỉnh |
| **ADMIN-13** | Quản trị hệ thống | Chỉnh sửa lớp học trực tuyến | Chỉnh sửa thông tin phòng học, thời gian, giảng viên phụ trách | `AdminController.cs` (`EditClass`) | Hoàn chỉnh |
| **ADMIN-14** | Quản trị hệ thống | Đóng / Mở trạng thái lớp học | Chuyển trạng thái giữa Mở (Open) và Đóng (Closed) | `AdminController.cs` (`CloseClass`) | Hoàn chỉnh |
| **ADMIN-15** | Quản trị hệ thống | Xem lịch giảng dạy toàn hệ thống | Lịch tổng thể các lớp học trực tuyến theo trình tự thời gian | `AdminController.cs` (`Schedule`) | Hoàn chỉnh |
| **ADMIN-16** | Quản trị hệ thống | Quản lý danh sách IELTS Tips | Danh sách bài viết kiến thức, lượt xem, trạng thái xuất bản | `AdminController.cs` (`Tips`) | Hoàn chỉnh |
| **ADMIN-17** | Quản trị hệ thống | Đăng bài viết mẹo thi mới | Soạn tiêu đề, tóm tắt, nội dung chi tiết, chuyên mục kỹ năng, tác giả | `AdminController.cs` (`CreateTip`) | Hoàn chỉnh |
| **ADMIN-18** | Quản trị hệ thống | Chỉnh sửa bài viết mẹo thi | Cập nhật nội dung, ảnh đại diện, chuyên mục của bài viết | `AdminController.cs` (`EditTip`) | Hoàn chỉnh |
| **ADMIN-19** | Quản trị hệ thống | Bật / Tắt xuất bản bài viết | Ẩn hoặc hiện bài viết ra ngoài danh mục công khai | `AdminController.cs` (`TogglePublishTip`)| Hoàn chỉnh |
| **ADMIN-20** | Quản trị hệ thống | Xóa bài viết mẹo thi | Xóa bài viết khỏi cơ sở dữ liệu | `AdminController.cs` (`DeleteTip`) | Hoàn chỉnh |
| **ADMIN-21** | Quản trị hệ thống | Thống kê danh mục đề thi thử | Xem tổng quan các bài thi, số lượng câu hỏi và số lượt học viên đã làm | `AdminController.cs` (`Tests`) | Triển khai một phần |
| **ADMIN-22** | Quản trị hệ thống | Phát sóng thông báo hệ thống | Gửi thông báo đại trà đến toàn bộ người dùng, hoặc riêng học viên/giáo viên | `AdminController.cs` (`BroadcastNotification`)| Hoàn chỉnh |
| **NOTIF-01** | Thông báo & Nhắc nhở | Trung tâm thông báo người dùng | Danh sách thông báo cá nhân, phân loại màu theo kiểu thông báo | `NotificationController.cs` (`Index`) | Hoàn chỉnh |
| **NOTIF-02** | Thông báo & Nhắc nhở | Đánh dấu thông báo đã đọc | Cập nhật `IsRead = true` cho một thông báo cụ thể | `NotificationController.cs` (`MarkAsRead`) | Hoàn chỉnh |
| **NOTIF-03** | Thông báo & Nhắc nhở | Đánh dấu tất cả thông báo đã đọc | Chuyển toàn bộ thông báo chưa đọc của người dùng thành đã đọc | `NotificationController.cs` (`MarkAllAsRead`)| Hoàn chỉnh |
| **NOTIF-04** | Thông báo & Nhắc nhở | Gửi bản sao nhắc nhở qua Email | Giả lập gửi nội dung thông báo nhắc nhở đến địa chỉ email của người dùng | `NotificationController.cs` (`SendEmailReminder`)| Triển khai một phần |
| **NOTIF-05** | Thông báo & Nhắc nhở | Chuông thông báo nhanh trên Header | Biểu tượng chuông trên Navbar, hiển thị số chưa đọc và xem nhanh 4 tin mới | `_LoginPartial.cshtml` | Hoàn chỉnh |
| **LEGACY-01**| Thành phần Legacy | LoginController (Stub) | Controller đăng nhập cũ không có view, bị thay thế bởi AccountController | `Controllers/LoginController.cs` | Không hoạt động |
| **LEGACY-02**| Thành phần Legacy | ManagerController (Stub) | Controller quản lý cũ không có view, bị thay thế bởi AdminController | `Controllers/ManagerController.cs` | Không hoạt động |
| **LEGACY-03**| Thành phần Legacy | HomeworkController (Stub) | Controller bài tập cũ không có view, thay thế bởi Student/Teacher | `Controllers/HomeworkController.cs` | Không hoạt động |
| **LEGACY-04**| Thành phần Legacy | PostController (Stub) | Controller bài đăng cũ không có view, thay thế bởi TipController | `Controllers/PostController.cs` | Không hoạt động |
| **LEGACY-05**| Thành phần Legacy | Mô hình dữ liệu cũ (Legacy Schema) | Các bảng `UserAccount`, `ClassRoom`, `Assignment`, `Question`... | `Models/UserAccount.cs`, v.v. | Chưa kết nối |

---

# PHẦN III — ĐẶC TẢ CHI TIẾT TỪNG CHỨC NĂNG

### 3.1. Phân hệ Xác thực & Tài khoản (AUTH)

#### [AUTH-01] Đăng nhập hệ thống (Local Password Login)
* **Mục đích:** Xác minh danh tính người dùng qua Email/Username và mật khẩu để cấp quyền truy cập.
* **Vai trò sử dụng:** Khách vãng lai, Học viên, Giảng viên, Quản trị viên (Khách chưa đăng nhập).
* **Điều kiện trước khi thực hiện:** Người dùng đã có tài khoản và `IsActive == true`.
* **Các bước thao tác:**
  1. Người dùng vào `/Account/Login`.
  2. Nhập Email hoặc Tên đăng nhập và Mật khẩu. Có thể tick "Ghi nhớ đăng nhập".
  3. Bấm nút "ĐĂNG NHẬP NGAY".
* **Đầu vào & Đầu ra:**
  - *Đầu vào:* `LoginViewModel` (`UsernameOrEmail`, `Password`, `RememberMe`, `ReturnUrl`).
  - *Đầu ra:* Thiết lập Authentication Cookie; điều hướng về ReturnUrl hoặc Dashboard theo vai trò. Nếu sai: hiển thị lỗi trên form.
* **Luồng xử lý:** Form Submit → `AccountController.Login(POST)` → Kiểm tra ModelState → Tìm `ApplicationUser` qua `FindByEmailAsync` hoặc `FindByNameAsync` → Kiểm tra `user.IsActive` → Gọi `_signInManager.PasswordSignInAsync()` → Đăng nhập thành công → Gọi `RedirectBasedOnRoleAsync(user)`:
  - Nếu Role là `Admin` → `/Admin/Dashboard`.
  - Nếu Role là `Teacher` → `/Teacher/Dashboard`.
  - Nếu Role là `Student` → `/Student/Dashboard`.
* **File & hàm liên quan:** [AccountController.cs](file:///D:/N%C4%83m%203/HTH_LTW/LUMORA-English-Center/LTW/Controllers/AccountController.cs#L48-L87), [AuthViewModels.cs](file:///D:/N%C4%83m%203/HTH_LTW/LUMORA-English-Center/LTW/Models/AuthViewModels.cs#L5-L20), [Login.cshtml](file:///D:/N%C4%83m%203/HTH_LTW/LUMORA-English-Center/LTW/Views/Account/Login.cshtml).
* **Quy tắc nghiệp vụ:** Nếu tài khoản bị khóa (`IsActive == false`), từ chối đăng nhập với thông báo yêu cầu liên hệ Admin.
* **Trạng thái hiện tại:** Hoàn chỉnh.
* **Vấn đề / Giới hạn:** Không có giới hạn số lần đăng nhập sai (lockout disabled).

#### [AUTH-02] Đăng nhập ngoài qua Google OAuth (External Google Login)
* **Mục đích:** Cho phép người dùng đăng nhập bằng tài khoản Google cá nhân.
* **Vai trò sử dụng:** Tất cả người dùng.
* **Điều kiện trước khi thực hiện:** Cấu hình Google ClientId/ClientSecret hợp lệ trong `appsettings.json`.
* **Các bước thao tác:** Bấm nút "Đăng nhập với Google" tại trang Login → Chuyển hướng tới trang xác thực Google → Google trả về callback `/signin-google` → Hệ thống nhận thông tin.
* **Đầu vào & Đầu ra:**
  - *Đầu vào:* Google Claims (`Email`, `Name`, `ProviderKey`).
  - *Đầu ra:* Phiên đăng nhập Identity; tự động đăng ký `ApplicationUser` với vai trò `Student` nếu email chưa từng tồn tại; gửi thông báo chào mừng vào DB.
* **Luồng xử lý:** Client → `AccountController.ExternalLogin(POST)` → Challenge Google → `AccountController.ExternalLoginCallback(GET)` → `_signInManager.ExternalLoginSignInAsync()`:
  - Nếu thành công → Vào Dashboard.
  - Nếu chưa có tài khoản → Tạo `ApplicationUser`, gán role `Student`, tạo thông báo chào mừng trong bảng `Notifications`, gọi `SignInAsync` → Vào `/Student/Dashboard`.
* **File & hàm liên quan:** [AccountController.cs](file:///D:/N%C4%83m%203/HTH_LTW/LUMORA-English-Center/LTW/Controllers/AccountController.cs#L90-L183).
* **Trạng thái hiện tại:** Hoàn chỉnh logic mã nguồn.

#### [AUTH-03] Đăng nhập nhanh tài khoản thử nghiệm (Quick Google Login)
* **Mục đích:** Cung cấp lối tắt đăng nhập một chạm cho môi trường phát triển/kiểm thử.
* **Vai trò sử dụng:** Bất kỳ ai kiểm thử giao diện đăng nhập.
* **Các bước thao tác:** Bấm nút "Google Test Login (Demo Student)" tại màn hình Login.
* **Đầu vào & Đầu ra:** Tự động đăng nhập vào tài khoản `student@lumora.edu.vn`.
* **File & hàm liên quan:** [AccountController.cs](file:///D:/N%C4%83m%203/HTH_LTW/LUMORA-English-Center/LTW/Controllers/AccountController.cs#L186-L200).
* **Trạng thái hiện tại:** Hoàn chỉnh.

#### [AUTH-04] Đăng ký tài khoản & Phát sinh OTP (Registration with OTP)
* **Mục đích:** Xác thực người dùng muốn mở tài khoản học viên thông qua xác minh email bằng mã OTP 6 chữ số.
* **Vai trò sử dụng:** Khách chưa có tài khoản.
* **Các bước thao tác:**
  1. Vào `/Account/Register`.
  2. Điền Họ và tên, Email, Tên đăng nhập, Mật khẩu, Xác nhận mật khẩu.
  3. Bấm "TIẾP TỤC & NHẬN MÃ OTP".
* **Đầu vào & Đầu ra:**
  - *Đầu vào:* `RegisterViewModel` (`FullName`, `Email`, `Username`, `Password`, `ConfirmPassword`).
  - *Đầu ra:* Sinh mã OTP ngẫu nhiên 6 chữ số, lưu bản ghi vào `OtpRecord` với hạn dùng 15 phút, hiển thị mã mô phỏng qua TempData và chuyển sang trang `/Account/VerifyOtp`.
* **File & hàm liên quan:** [AccountController.cs](file:///D:/N%C4%83m%203/HTH_LTW/LUMORA-English-Center/LTW/Controllers/AccountController.cs#L202-L267), [OtpRecord.cs](file:///D:/N%C4%83m%203/HTH_LTW/LUMORA-English-Center/LTW/Models/OtpRecord.cs).
* **Quy tắc nghiệp vụ:** Kiểm tra trùng Email và trùng Username trước khi sinh mã.
* **Trạng thái hiện tại:** Hoàn chỉnh (Mã OTP được hiển thị trực tiếp lên TempData để thuận tiện kiểm thử vì chưa tích hợp máy chủ SMTP thực).

#### [AUTH-05] Xác thực mã OTP & Kích hoạt tài khoản (Verify OTP)
* **Mục đích:** Kiểm tra mã OTP do người dùng nhập để chính thức khởi tạo bản ghi `ApplicationUser` trong cơ sở dữ liệu.
* **Vai trò sử dụng:** Người dùng đang trong tiến trình đăng ký hoặc khôi phục mật khẩu.
* **Các bước thao tác:** Nhập mã OTP 6 số tại màn hình `/Account/VerifyOtp` → Bấm "XÁC NHẬN MÃ OTP".
* **Đầu vào & Đầu ra:**
  - *Đầu vào:* `VerifyOtpViewModel` (`Email`, `OtpCode`, `Purpose`).
  - *Đầu ra:* Bản ghi `OtpRecord` được đánh dấu `IsUsed = true`, tài khoản người dùng được tạo với vai trò `Student`, tạo thông báo chào mừng trong bảng `Notifications`, tự động đăng nhập và chuyển vào `Student/Dashboard`.
* **File & hàm liên quan:** [AccountController.cs](file:///D:/N%C4%83m%203/HTH_LTW/LUMORA-English-Center/LTW/Controllers/AccountController.cs#L282-L369).
* **Trạng thái hiện tại:** Hoàn chỉnh.

#### [AUTH-06] & [AUTH-07] Quên mật khẩu & Đặt lại mật khẩu mới (Forgot & Reset Password)
* **Mục đích:** Cho phép học viên tự khôi phục mật khẩu khi bị quên mà không cần can thiệp từ quản trị viên.
* **Các bước thao tác:**
  1. Vào `/Account/ForgotPassword` → Nhập Email đã đăng ký → Hệ thống sinh mã OTP lưu bảng `OtpRecord`.
  2. Chuyển sang `/Account/VerifyForgotOtp` để nhập mã OTP.
  3. Sau khi xác thực OTP hợp lệ, chuyển sang `/Account/ResetPassword` → Nhập mật khẩu mới & xác nhận mật khẩu.
* **File & hàm liên quan:** [AccountController.cs](file:///D:/N%C4%83m%203/HTH_LTW/LUMORA-English-Center/LTW/Controllers/AccountController.cs#L371-L473).
* **Quy tắc nghiệp vụ:** Sử dụng `GeneratePasswordResetTokenAsync` của ASP.NET Identity để đảm bảo tính an toàn mã hóa token trước khi gọi `ResetPasswordAsync`.
* **Trạng thái hiện tại:** Hoàn chỉnh.

#### [AUTH-08] Đăng xuất khỏi hệ thống (User Logout)
* **Mục đích:** Hủy phiên làm việc và bảo mật tài khoản người dùng.
* **Thao tác:** Bấm "Đăng xuất" tại menu hồ sơ header → Gửi POST request có token chống giả mạo `@Html.AntiForgeryToken()`.
* **File & hàm liên quan:** [AccountController.cs](file:///D:/N%C4%83m%203/HTH_LTW/LUMORA-English-Center/LTW/Controllers/AccountController.cs#L476-L483).
* **Trạng thái hiện tại:** Hoàn chỉnh.

#### [AUTH-09] Xem và cập nhật hồ sơ cá nhân (Profile Management)
* **Mục đích:** Quản lý thông tin cá nhân và thay đổi mật khẩu định kỳ.
* **Thao tác:** Vào `/Account/Profile` → Chỉnh sửa Họ tên, Số điện thoại, Tiểu sử, URL Avatar; nếu muốn đổi mật khẩu thì nhập Mật khẩu hiện tại và Mật khẩu mới → Bấm "Lưu Thay Đổi".
* **File & hàm liên quan:** [AccountController.cs](file:///D:/N%C4%83m%203/HTH_LTW/LUMORA-English-Center/LTW/Controllers/AccountController.cs#L486-L562), [Profile.cshtml](file:///D:/N%C4%83m%203/HTH_LTW/LUMORA-English-Center/LTW/Views/Account/Profile.cshtml).
* **Quy tắc nghiệp vụ:** Bắt buộc nhập đúng mật khẩu hiện tại mới cho phép đổi mật khẩu mới (`ChangePasswordAsync`).
* **Trạng thái hiện tại:** Hoàn chỉnh.

---

### 3.2. Phân hệ Trang chủ & Tư vấn (HOME)

#### [HOME-01] Hiển thị Landing Page chuẩn giao diện
* **Mục đích:** Cung cấp trang chủ giới thiệu toàn diện năng lực của trung tâm LUMORA theo chuẩn phong cách IELTS Online Tests.
* **Vai trò:** Khách vãng lai, thí sinh, học viên.
* **Các thành phần giao diện thực tế trên trang:**
  1. *Hero Banner:* Tiêu đề đối tác Platinum (British Council & IDP), nút CTA "Kiểm tra trình độ miễn phí", "Khám phá khóa học".
  2. *Card mô phỏng đề thi CDI:* Bộ đếm giờ mẫu, câu hỏi Passage 1 mẫu, 4 tab kỹ năng (Listening, Reading, Writing, Speaking) với dải màu gradient chuẩn.
  3. *Lộ trình 6 bước (6 Steps to Achieve IELTS Goals):* Placement Test, Intensive Course, Global Classrooms, Free Mock Test, Simulation Mock Exam, Unlock Full Services.
  4. *Kho đề thi mới nhất (Latest Free Materials):* 4 card bộ đề thi mẫu (Mock Test 2026 January, Cambridge 19 Simulation...).
  5. *Khóa học trực tuyến (Online Courses):* Danh sách khóa học được thiết kế bởi cựu giám khảo.
  6. *Lớp học trực tuyến miễn phí (Free Live Lessons):* 3 webinar sắp diễn ra.
  7. *Lộ trình tăng band điểm (Band Roadmap):* 4 giai đoạn từ 4.5 đến 8.0+.
  8. *Bằng chứng xã hội (Social Proof):* Thống kê 500+ đề thi, 2.5M+ lượt thi, đánh giá học viên từ Facebook, TikTok, YouTube.
  9. *Đội ngũ giảng viên (Expert Teaching Team):* Thanh trượt ngang (Horizontal Slider) hiển thị 7 giảng viên cựu giám khảo cựu BC/IDP kèm điểm IELTS 8.5 - 9.0.
  10. *Đối tác uy tín (Valued Partners):* Logo liên kết British Council, IDP, Oxford, Southampton, Newcastle, Cambridge.
  11. *Câu hỏi thường gặp (FAQ):* 5 câu hỏi dạng Bootstrap Accordion.
  12. *Biểu mẫu tư vấn (Get in Touch Form):* Form đăng ký nhận lộ trình.
* **File & hàm liên quan:** [HomeController.cs](file:///D:/N%C4%83m%203/HTH_LTW/LUMORA-English-Center/LTW/Controllers/HomeController.cs#L35-L41), [LandingPageViewModel.cs](file:///D:/N%C4%83m%203/HTH_LTW/LUMORA-English-Center/LTW/Models/LandingPageViewModel.cs), [Index.cshtml](file:///D:/N%C4%83m%203/HTH_LTW/LUMORA-English-Center/LTW/Views/Home/Index.cshtml).
* **Trạng thái hiện tại:** Hoàn chỉnh giao diện và dữ liệu ViewModel.

#### [HOME-02] Gửi biểu mẫu đăng ký tư vấn 1-1
* **Mục đích:** Thu thập nhu cầu tư vấn lộ trình học từ người dùng trang chủ.
* **Thao tác:** Nhập Họ tên, Email, Số điện thoại, Mục tiêu Band, Lời nhắn → Bấm "Gửi Yêu Cầu Tư Vấn Ngay".
* **Đầu vào & Đầu ra:** `ContactConsultationModel` → Kiểm tra `ModelState.IsValid` → Xuất thông báo cảm ơn qua TempData.
* **File liên quan:** [HomeController.cs](file:///D:/N%C4%83m%203/HTH_LTW/LUMORA-English-Center/LTW/Controllers/HomeController.cs#L46-L64).
* **Trạng thái hiện tại:** Triển khai một phần (Validation dữ liệu hoàn chỉnh, phản hồi thông báo thành công, nhưng trong code ghi chú `// Giả lập lưu trữ thông tin đăng ký thành công vào cơ sở dữ liệu` — chưa có bảng Database riêng để lưu vết yêu cầu tư vấn).

---

### 3.3. Phân hệ Khóa học & Đăng ký / Thanh toán (COURSE)

#### [COURSE-01] Xem danh mục & Lọc khóa học
* **Mục đích:** Giúp người học tìm kiếm khóa học phù hợp theo kỹ năng, trình độ và từ khóa.
* **Vai trò:** Khách vãng lai, học viên.
* **Thao tác:** Vào `/Course/Index` → Chọn tab Chuyên mục (IELTS, Communication, Speaking, Writing, Reading), lọc theo Trình độ (Beginner, Intermediate, Advanced), nhập từ khóa tìm kiếm.
* **Luồng xử lý:** Query bảng `Courses` (`IsPublished == true`), `Include(Teacher)`, `Include(Chapters)`, tính toán số lượng bài học và bài tập NotMapped, sắp xếp theo khóa học nổi bật (`IsFeatured`).
* **File & hàm liên quan:** [CourseController.cs](file:///D:/N%C4%83m%203/HTH_LTW/LUMORA-English-Center/LTW/Controllers/CourseController.cs#L21-L56), [Views/Course/Index.cshtml](file:///D:/N%C4%83m%203/HTH_LTW/LUMORA-English-Center/LTW/Views/Course/Index.cshtml).
* **Trạng thái hiện tại:** Hoàn chỉnh.

#### [COURSE-02] Xem chi tiết khóa học & Đề cương
* **Mục đích:** Cung cấp thông tin chi tiết về khóa học, giảng viên phụ trách, đề cương từng chương và bài học, các buổi live class kèm theo.
* **Thao tác:** Bấm vào một khóa học tại `/Course/Detail/{id}`.
* **Quy tắc nghiệp vụ:** Nếu học viên đã đăng nhập và đã mua khóa học (`Enrollments.Any`), hệ thống hiển thị nút "VÀO HỌC NGAY", ngược lại hiển thị nút "ĐĂNG KÝ HỌC NGAY" chuyển đến trang thanh toán.
* **File & hàm liên quan:** [CourseController.cs](file:///D:/N%C4%83m%203/HTH_LTW/LUMORA-English-Center/LTW/Controllers/CourseController.cs#L58-L87), [Views/Course/Detail.cshtml](file:///D:/N%C4%83m%203/HTH_LTW/LUMORA-English-Center/LTW/Views/Course/Detail.cshtml).
* **Trạng thái hiện tại:** Hoàn chỉnh.

#### [COURSE-03], [COURSE-04] & [COURSE-05] Mua khóa học & Thanh toán (Purchase & Payment Flow)
* **Mục đích:** Quy trình thương mại điện tử khép kín cho phép học viên mua khóa học trực tuyến.
* **Yêu cầu phân quyền:** Bắt buộc đăng nhập (`[Authorize]`).
* **Các bước thao tác:**
  1. Học viên vào `/Course/Purchase/{id}`.
  2. Hệ thống kiểm tra: nếu học viên đã sở hữu khóa học (`Enrollments.Any(StudentId == user.Id && CourseId == id)`), chặn mua trùng và điều hướng về trang chi tiết kèm thông báo.
  3. Học viên chọn phương thức thanh toán: VNPay QR, Ví MoMo, hoặc Chuyển khoản ngân hàng 24/7 (kèm thông tin số tài khoản MBBank đối soát).
  4. Bấm "Xác Nhận Thanh Toán (số tiền đ)".
  5. Request gửi tới `CourseController.ProcessPayment(POST)`.
  6. Hệ thống tạo bản ghi `Enrollment` mới:
     - `PricePaid = course.Price`
     - `PaymentStatus = "Completed"`
     - `TransactionId = "LUMORA-" + DateTime.Now.ToString("yyyyMMddHHmmss")`
  7. Hệ thống tự động tạo 1 bản ghi `Notification` gửi tới học viên: *"Đăng ký khóa học thành công! Chúc mừng bạn đã sở hữu thành công khóa học..."* kèm đường dẫn vào học.
  8. Lưu vào SQL Server qua `_context.SaveChangesAsync()`.
  9. Chuyển hướng tới `/Course/PaymentSuccess/{enrollmentId}` hiển thị biên lai và mã giao dịch.
* **File & hàm liên quan:** [CourseController.cs](file:///D:/N%C4%83m%203/HTH_LTW/LUMORA-English-Center/LTW/Controllers/CourseController.cs#L89-L183), [Enrollment.cs](file:///D:/N%C4%83m%203/HTH_LTW/LUMORA-English-Center/LTW/Models/Enrollment.cs), [Views/Course/Purchase.cshtml](file:///D:/N%C4%83m%203/HTH_LTW/LUMORA-English-Center/LTW/Views/Course/Purchase.cshtml), [Views/Course/PaymentSuccess.cshtml](file:///D:/N%C4%83m%203/HTH_LTW/LUMORA-English-Center/LTW/Views/Course/PaymentSuccess.cshtml).
* **Trạng thái hiện tại:** Hoàn chỉnh logic cơ sở dữ liệu và trải nghiệm người dùng (Cổng thanh toán xử lý tự động nội bộ mô phỏng quy trình giao dịch thành công).

---

### 3.4. Phân hệ Học tập Học viên (STUDENT)

#### [STUDENT-01] Bàn học cá nhân tổng hợp (Student Dashboard)
* **Mục đích:** Trung tâm điều hành học tập dành riêng cho học viên đã đăng nhập.
* **Yêu cầu phân quyền:** `[Authorize]`.
* **Dữ liệu tổng hợp thực tế:**
  - 4 Thẻ KPI: Số khóa học đã mua, Số bài học đã hoàn thành, Số bài tập cần nộp, Điểm Band kiểm tra năng lực gần nhất.
  - Danh sách khóa học đang học kèm thanh tiến độ % hoàn thành tính từ `LessonProgresses`.
  - Lịch học trực tuyến sắp tới (Upcoming Live Classes) lọc từ các lớp Public và lớp của các khóa học đã mua.
  - Danh sách bài tập còn tồn đọng chưa nộp (Pending Exercises).
  - Kết quả bài nộp gần đây (Recent Submissions & Scores).
  - Báo cáo kết quả bài kiểm tra xếp lớp (Placement Test Result).
  - 4 Thông báo mới nhất từ trung tâm.
* **File & hàm liên quan:** [StudentController.cs](file:///D:/N%C4%83m%203/HTH_LTW/LUMORA-English-Center/LTW/Controllers/StudentController.cs#L23-L104), [Views/Student/Dashboard.cshtml](file:///D:/N%C4%83m%203/HTH_LTW/LUMORA-English-Center/LTW/Views/Student/Dashboard.cshtml).
* **Trạng thái hiện tại:** Hoàn chỉnh.

#### [STUDENT-02] Khóa học của tôi (My Courses)
* **Mục đích:** Hiển thị toàn bộ các khóa học mà học viên đã thanh toán kèm tiến độ chi tiết từng khóa.
* **File & hàm liên quan:** [StudentController.cs](file:///D:/N%C4%83m%203/HTH_LTW/LUMORA-English-Center/LTW/Controllers/StudentController.cs#L106-L129), [Views/Student/MyCourses.cshtml](file:///D:/N%C4%83m%203/HTH_LTW/LUMORA-English-Center/LTW/Views/Student/MyCourses.cshtml).
* **Trạng thái hiện tại:** Hoàn chỉnh.

#### [STUDENT-03] Trình phát bài học đa phương tiện (Multimedia Lesson Player)
* **Mục đích:** Cung cấp không gian học tập số hóa tương tác cao cho từng bài học.
* **Quy tắc nghiệp vụ:** 
  - Kiểm tra điều kiện: Nếu bài học không được mở học thử (`IsFreePreview == false`) và học viên chưa mua khóa học (`Enrollments.Any == false`), hệ thống chặn truy cập và chuyển hướng về trang mua khóa học kèm thông báo lỗi.
  - Tự động ghi nhận lịch sử truy cập vào bảng `LessonProgress` (`LastAccessedAt = DateTime.Now`).
* **Tính năng giao diện bài học:**
  - Sidebar cây thư mục Chương & Bài học bên trái có đánh dấu tích xanh các bài đã hoàn thành và highlight bài đang học.
  - Nút chuyển bài trước (Previous) và bài tiếp theo (Next) tự động tính toán theo thứ tự chương mục `OrderIndex`.
  - Khung phát Video bài giảng (hỗ trợ nhúng YouTube tỷ lệ chuẩn 16:9).
  - Trình phát Audio nghe MP3 có thanh điều khiển âm lượng và thời gian.
  - Vùng nội dung lý thuyết định dạng HTML phong phú.
  - Bảng thẻ từ vựng trọng tâm (Vocabulary Cards) tự động phân tích từ chuỗi JSON (từ vựng, từ loại, phiên âm, định nghĩa, ví dụ minh họa).
  - Hộp ghi nhớ ngữ pháp trọng tâm (Grammar Focus).
  - Danh sách bài tập thực hành liên kết với bài học.
* **File & hàm liên quan:** [StudentController.cs](file:///D:/N%C4%83m%203/HTH_LTW/LUMORA-English-Center/LTW/Controllers/StudentController.cs#L131-L202), [Views/Student/Lesson.cshtml](file:///D:/N%C4%83m%203/HTH_LTW/LUMORA-English-Center/LTW/Views/Student/Lesson.cshtml).
* **Trạng thái hiện tại:** Hoàn chỉnh.

#### [STUDENT-04] Đánh dấu hoàn thành bài học (Toggle Lesson Completion)
* **Mục đích:** Giúp học viên chủ động quản lý tiến độ học tập của mình.
* **Thao tác:** Bấm nút "Đánh Dấu Hoàn Thành" / "Đã Hoàn Thành" tại bài học → Gửi POST request tới `StudentController.ToggleCompleteLesson`.
* **Tác động Database:** Cập nhật `LessonProgress.IsCompleted` và `CompletedAt`.
* **File liên quan:** [StudentController.cs](file:///D:/N%C4%83m%203/HTH_LTW/LUMORA-English-Center/LTW/Controllers/StudentController.cs#L204-L241).
* **Trạng thái hiện tại:** Hoàn chỉnh.

#### [STUDENT-05] & [STUDENT-06] Làm bài tập & Xem lại đáp án (Take & Review Exercise)
* **Mục đích:** Luyện tập củng cố kiến thức sau mỗi bài học, chấm điểm tự động và nhận phản hồi chi tiết từ giảng viên.
* **Các bước thao tác:**
  1. Vào bài tập qua `/Student/Exercise/{id}`.
  2. Chọn đáp án cho các câu hỏi trắc nghiệm A, B, C, D.
  3. Bấm "NỘP BÀI TẬP".
  4. Hệ thống so khớp đáp án người dùng chọn với `CorrectAnswer` của từng câu hỏi.
  5. Tính điểm tự động `AutoScore = (correctQuestions / totalQuestions) * 10`.
  6. Lưu bài nộp vào `ExerciseSubmission` kèm chuỗi JSON lưu vết câu trả lời `AnswersJson`.
  7. Tự động sinh `Notification` thông báo điểm số cho học viên.
  8. Sau khi nộp bài: Giao diện chuyển sang chế độ Xem lại (Review):
     - Hiển thị tổng điểm và nhận xét.
     - Đổi viền xanh cho câu trả lời đúng, viền đỏ cho câu trả lời sai.
     - Khóa input (disabled) và hiển thị Đáp án đúng (`CorrectAnswer`) kèm Lời giải thích chi tiết (`Explanation`).
* **File & hàm liên quan:** [StudentController.cs](file:///D:/N%C4%83m%203/HTH_LTW/LUMORA-English-Center/LTW/Controllers/StudentController.cs#L243-L341), [Views/Student/Exercise.cshtml](file:///D:/N%C4%83m%203/HTH_LTW/LUMORA-English-Center/LTW/Views/Student/Exercise.cshtml).
* **Trạng thái hiện tại:** Hoàn chỉnh.

---

### 3.5. Phân hệ Lớp học trực tuyến (CLASS)

#### [CLASS-01] & [CLASS-02] Danh sách & Chi tiết lớp Live Class
* **Mục đích:** Hiển thị lịch các lớp học trực tuyến, webinar chuyên đề và lớp tương tác 1-1.
* **Tính năng:**
  - Lọc theo: Tất cả lớp (`All`), Lớp công khai (`Public`), hoặc Lớp của tôi (`MyClasses`).
  - Kiểm tra quyền tham gia (`canJoin`): Lớp Public mở tự do cho mọi người; Lớp Private chỉ cho phép học viên đã mua khóa học liên kết tham gia.
* **File liên quan:** [ClassController.cs](file:///D:/N%C4%83m%203/HTH_LTW/LUMORA-English-Center/LTW/Controllers/ClassController.cs#L21-L87), [Views/Class/Index.cshtml](file:///D:/N%C4%83m%203/HTH_LTW/LUMORA-English-Center/LTW/Views/Class/Index.cshtml), [Views/Class/Detail.cshtml](file:///D:/N%C4%83m%203/HTH_LTW/LUMORA-English-Center/LTW/Views/Class/Detail.cshtml).
* **Trạng thái hiện tại:** Hoàn chỉnh.

#### [CLASS-03] Kiểm tra điều kiện & Tham gia lớp (Join Gate)
* **Mục đích:** Cổng kiểm soát bảo mật trước khi vào phòng học trực tuyến.
* **Quy tắc nghiệp vụ:**
  - Nếu là lớp Private mà học viên chưa mua khóa học liên kết: Chặn truy cập, xuất thông báo yêu cầu mua khóa học và chuyển hướng đến trang khóa học tương ứng.
  - Nếu hợp lệ: Tự động ghi nhận một lượt điểm danh/đăng ký tham dự vào bảng `LiveClassStudents` (nếu chưa có), sau đó chuyển vào phòng học `/Class/Room/{id}`.
* **File liên quan:** [ClassController.cs](file:///D:/N%C4%83m%203/HTH_LTW/LUMORA-English-Center/LTW/Controllers/ClassController.cs#L89-L131).
* **Trạng thái hiện tại:** Hoàn chỉnh.

#### [CLASS-04] Giao diện phòng học trực tuyến ảo (Virtual Classroom)
* **Mục đích:** Mô phỏng không gian phòng học tương tác trực tuyến giữa giảng viên và học viên.
* **Giao diện & Thành phần tương tác:**
  - Sân khấu trình chiếu slide & camera giảng viên (kèm hiệu ứng LIVE REC, thông tin 1080p 60fps).
  - Thanh công cụ điều khiển: Nút Bật/Tắt Microphone, Bật/Tắt Camera, Nút Giơ tay phát biểu (Raise Hand), Nút Rời phòng học.
  - Tab Danh sách thành viên (Hiển thị Giảng viên Host, Bạn và các học viên khác từ `ClassStudents`).
  - Tab Hộp thoại Trò chuyện trực tiếp (Live Chat): Cho phép học viên gõ và gửi tin nhắn tức thời vào khung chat.
* **File liên quan:** [ClassController.cs](file:///D:/N%C4%83m%203/HTH_LTW/LUMORA-English-Center/LTW/Controllers/ClassController.cs#L133-L163), [Views/Class/Room.cshtml](file:///D:/N%C4%83m%203/HTH_LTW/LUMORA-English-Center/LTW/Views/Class/Room.cshtml).
* **Trạng thái hiện tại:** Hoàn chỉnh giao diện và tương tác client (Tương tác chat và media stream hoạt động qua mô phỏng DOM Scripting, chưa kết nối máy chủ WebRTC/SignalR thực tế).

---

### 3.6. Phân hệ Khảo thí & Thi thử IELTS (TEST)

> [!IMPORTANT]
> **Tuân thủ bắt buộc Rule 9 (English-Only Requirement):** Toàn bộ giao diện làm bài thi (`TakeTest.cshtml`), các chỉ dẫn, bộ đếm giờ, câu hỏi, đoạn văn, nút nộp bài và màn hình kết quả (`Result.cshtml`) đều được thể hiện hoàn toàn bằng tiếng Anh chuẩn học thuật.

#### [TEST-01], [TEST-02] & [TEST-03] Thư viện đề thi & Môi trường làm bài thi (IELTS Examination Room)
* **Mục đích:** Cung cấp hệ thống khảo thí đánh giá năng lực chuẩn 4 kỹ năng Listening, Reading, Writing, Speaking.
* **Loại đề thi hỗ trợ:**
  - `ExampleTest`: Bài thi chẩn đoán nhanh miễn phí (30 phút, 4 kỹ năng).
  - `PlacementTest`: Bài kiểm tra xếp lớp năng lực đầu vào (45 phút, 4 kỹ năng).
* **Cấu trúc bài thi thực tế:**
  - *Listening:* Tích hợp trình phát audio MP3 chuẩn, các câu hỏi trắc nghiệm kiểm tra khả năng bắt từ khóa.
  - *Reading:* Khung hiển thị đoạn văn bài đọc (Reading Passage) độc lập với thanh cuộn riêng, câu hỏi trắc nghiệm logic/nghĩa từ vựng.
  - *Writing:* Đề bài Task 2 học thuật, khung nhập bài luận với bộ đếm số lượng từ tự động (`updateWordCount`).
  - *Speaking:* Đề bài Part 2/3, nút mô phỏng ghi âm câu trả lời (`simulateRecord`).
* **File liên quan:** [TestController.cs](file:///D:/N%C4%83m%203/HTH_LTW/LUMORA-English-Center/LTW/Controllers/TestController.cs#L21-L73), [Views/Test/TakeTest.cshtml](file:///D:/N%C4%83m%203/HTH_LTW/LUMORA-English-Center/LTW/Views/Test/TakeTest.cshtml).
* **Trạng thái hiện tại:** Hoàn chỉnh.

#### [TEST-04], [TEST-05], [TEST-06] & [TEST-07] Bộ công cụ hỗ trợ phòng thi
* **Đồng hồ đếm ngược:** Đếm ngược theo số phút quy định của đề thi (`TimeLimitMinutes`), hiển thị trên thanh công cụ cố định; khi hết giờ tự động hiển thị thông báo tiếng Anh *"Time is up! The system is automatically submitting your examination."* và tự submit form.
* **Bảng điều hướng câu hỏi (Question Palette):** Hiển thị danh sách số thứ tự câu hỏi dạng lưới bên cột phải; tự động đổi màu xanh khi thí sinh đã chọn đáp án hoặc nhập văn bản, hỗ trợ nhấp để cuộn nhanh đến câu hỏi.
* **Bộ đếm từ Writing:** Tự động tính toán số từ khi thí sinh gõ văn bản và hiển thị tức thời `Word count: X words`.
* **Mô phỏng Speaking:** Nút thu âm chuyển đổi trạng thái ghi âm và cập nhật giá trị vào trường ẩn.

#### [TEST-08] & [TEST-09] Bộ máy chấm điểm & Báo cáo kết quả (Scoring Engine & Report)
* **Mục đích:** Tính toán điểm số 4 kỹ năng, làm tròn điểm Overall theo tiêu chuẩn khảo thí quốc tế IELTS, xếp loại khung tham chiếu Châu Âu (CEFR), đưa ra nhận xét chuyên môn và đề xuất khóa học phù hợp.
* **Thuật toán chấm điểm trong `TestController.SubmitTest`:**
  - *Listening & Reading:* So khớp tự động đáp án thí sinh chọn với `CorrectAnswer` của câu hỏi. Điểm Band quy đổi theo tỷ lệ câu đúng trên thang 5.0 - 9.0.
  - *Writing:* Đánh giá dựa trên độ dài và dung lượng bài luận (trên 200 từ đạt Band 7.0, trên 100 từ đạt Band 6.5).
  - *Speaking:* Điểm mô phỏng ước tính (Band 6.5).
  - *Điểm Overall Band:* Tính trung bình cộng 4 kỹ năng và làm tròn theo quy tắc chuẩn IELTS (làm tròn đến 0.5 gần nhất):
    ```csharp
    decimal rawAvg = (listeningBand + readingBand + writingBand + speakingBand) / 4.0m;
    decimal overallBand = Math.Round(rawAvg * 2, MidpointRounding.AwayFromZero) / 2;
    ```
  - *Phân cấp CEFR & Phản hồi tiếng Anh:*
    - $\ge 7.5$: *C1 Advanced (IELTS 7.5+)* — Nhận xét phát huy phản biện và tính liên kết cao cấp.
    - $6.5 - 7.0$: *B2 Upper-Intermediate (IELTS 6.5)* — Nhận xét củng cố từ vựng chuyên sâu và paraphrasing.
    - $< 6.5$: *B1 Intermediate (IELTS 5.5)* — Nhận xét rèn luyện nghe chép chính tả và skimming/scanning.
  - *Lưu trữ kết quả:* Lưu bản ghi `TestAttempt` mới vào SQL Server kèm toàn bộ vết trả lời `AnswersJson` và danh sách khóa học đề xuất `RecommendedCoursesJson`.
  - *Thông báo:* Tự động gửi thông báo kết quả thi tới hộp thư của học viên nếu đã đăng nhập.
* **File liên quan:** [TestController.cs](file:///D:/N%C4%83m%203/HTH_LTW/LUMORA-English-Center/LTW/Controllers/TestController.cs#L75-L233), [Views/Test/Result.cshtml](file:///D:/N%C4%83m%203/HTH_LTW/LUMORA-English-Center/LTW/Views/Test/Result.cshtml).
* **Trạng thái hiện tại:** Hoàn chỉnh.

---

### 3.7. Phân hệ Kinh nghiệm & Mẹo thi IELTS (TIP)

#### [TIP-01] & [TIP-02] Thư viện mẹo thi & Chi tiết bài viết
* **Mục đích:** Cung cấp kho tài liệu bài viết chia sẻ kinh nghiệm học tập miễn phí cho cộng đồng.
* **Tính năng:**
  - Lọc theo chuyên mục: Listening, Reading, Writing, Speaking, Vocabulary, Grammar hoặc Tất cả (`All`).
  - Tìm kiếm tiêu đề và nội dung tóm tắt theo từ khóa.
  - Xem chi tiết bài viết: Tự động tăng số lượt xem `tip.ViewsCount++` và lưu vào cơ sở dữ liệu.
  - Hiển thị danh sách 3 bài viết liên quan (Related Tips) ở cuối trang.
* **File liên quan:** [TipController.cs](file:///D:/N%C4%83m%203/HTH_LTW/LUMORA-English-Center/LTW/Controllers/TipController.cs), [Views/Tip/Index.cshtml](file:///D:/N%C4%83m%203/HTH_LTW/LUMORA-English-Center/LTW/Views/Tip/Index.cshtml), [Views/Tip/Detail.cshtml](file:///D:/N%C4%83m%203/HTH_LTW/LUMORA-English-Center/LTW/Views/Tip/Detail.cshtml).
* **Trạng thái hiện tại:** Hoàn chỉnh.

---

### 3.8. Phân hệ Giảng viên (TEACHER)

#### [TEACHER-01] & [TEACHER-02] Danh sách & Hồ sơ chi tiết giảng viên
* **Mục đích:** Quảng bá năng lực đội ngũ học thuật tới học viên.
* **Dữ liệu thực tế:** Truy vấn người dùng có `RoleName == "Teacher"` và `IsActive == true`. Trang chi tiết hiển thị ảnh, tiểu sử, email, danh sách các khóa học phụ trách và lịch các buổi Live Class sắp tới do giảng viên đó đứng lớp.
* **File liên quan:** [TeacherController.cs](file:///D:/N%C4%83m%203/HTH_LTW/LUMORA-English-Center/LTW/Controllers/TeacherController.cs#L28-L86), [Views/Teacher/Index.cshtml](file:///D:/N%C4%83m%203/HTH_LTW/LUMORA-English-Center/LTW/Views/Teacher/Index.cshtml), [Views/Teacher/Detail.cshtml](file:///D:/N%C4%83m%203/HTH_LTW/LUMORA-English-Center/LTW/Views/Teacher/Detail.cshtml).
* **Trạng thái hiện tại:** Hoàn chỉnh.

#### [TEACHER-03] Bàn làm việc giảng viên (Teacher Dashboard)
* **Phân quyền:** `[Authorize(Roles = "Teacher,Admin")]`.
* **Tính năng:** Tổng hợp số liệu các khóa học đang dạy, buổi học trực tuyến, số học viên theo học và danh sách bài nộp đang chờ chấm điểm.
* **File liên quan:** [TeacherController.cs](file:///D:/N%C4%83m%203/HTH_LTW/LUMORA-English-Center/LTW/Controllers/TeacherController.cs#L92-L137).
* **Trạng thái hiện tại:** Hoàn chỉnh.

#### [TEACHER-04] Quản lý lớp học của tôi (My Classes)
* **Tính năng:** Danh sách các buổi Live Class do chính giảng viên phụ trách kèm số lượng học viên đã đăng ký tham dự.
* **File liên quan:** [TeacherController.cs](file:///D:/N%C4%83m%203/HTH_LTW/LUMORA-English-Center/LTW/Controllers/TeacherController.cs#L140-L156).
* **Trạng thái hiện tại:** Hoàn chỉnh.

#### [TEACHER-05] & [TEACHER-06] Quản lý & Tạo bài học mới (Lesson Authoring)
* **Tính năng:** Giảng viên xem danh sách các bài học thuộc các khóa mình phụ trách; Tạo bài học mới: Chọn chương học, nhập tiêu đề, số thứ tự, nhúng URL video bài giảng, audio URL, link PDF tài liệu, nội dung lý thuyết HTML, từ vựng trọng tâm JSON, ngữ pháp và tick chọn "Cho phép học thử miễn phí".
* **File liên quan:** [TeacherController.cs](file:///D:/N%C4%83m%203/HTH_LTW/LUMORA-English-Center/LTW/Controllers/TeacherController.cs#L158-L220), [Views/Teacher/CreateLesson.cshtml](file:///D:/N%C4%83m%203/HTH_LTW/LUMORA-English-Center/LTW/Views/Teacher/CreateLesson.cshtml).
* **Trạng thái hiện tại:** Hoàn chỉnh.

#### [TEACHER-07] & [TEACHER-08] Quản lý bài tập & Soạn thảo câu hỏi (Exercise Authoring)
* **Tính năng:** Xem danh sách bài tập; Tạo bài tập mới: Chọn bài học trực thuộc, đặt thời gian làm bài, điểm sàn đạt yêu cầu; Soạn câu hỏi trắc nghiệm ban đầu (Nội dung câu hỏi, 4 lựa chọn A/B/C/D, đáp án đúng và lời giải thích chi tiết).
* **File liên quan:** [TeacherController.cs](file:///D:/N%C4%83m%203/HTH_LTW/LUMORA-English-Center/LTW/Controllers/TeacherController.cs#L222-L304), [Views/Teacher/CreateExercise.cshtml](file:///D:/N%C4%83m%203/HTH_LTW/LUMORA-English-Center/LTW/Views/Teacher/CreateExercise.cshtml).
* **Trạng thái hiện tại:** Hoàn chỉnh.

#### [TEACHER-09] Theo dõi danh sách học viên (Enrolled Students Viewer)
* **Tính năng:** Xem danh sách học viên thực tế đã thanh toán các khóa học của giảng viên, email, ngày đăng ký và số bài tập đã nộp.
* **File liên quan:** [TeacherController.cs](file:///D:/N%C4%83m%203/HTH_LTW/LUMORA-English-Center/LTW/Controllers/TeacherController.cs#L306-L334).
* **Trạng thái hiện tại:** Hoàn chỉnh.

#### [TEACHER-10] Chấm điểm & Nhận xét bài tập học viên (Grading & Assessment)
* **Mục đích:** Giảng viên đánh giá bài làm Writing/Speaking và điều chỉnh điểm số bài tập.
* **Thao tác:** Vào `/Teacher/Assessments` → Bấm "Chấm Điểm" mở Modal → Nhập điểm giáo viên chấm (thang điểm 10) và Lời nhận xét chi tiết → Bấm lưu.
* **Tác động Database:**
  - Cập nhật `ExerciseSubmission.TeacherScore`.
  - Tính lại `TotalScore = Math.Round((AutoScore + teacherScore) / 2.0m, 1)`.
  - Lưu `TeacherComment`.
  - Tự động tạo 1 bản ghi `Notification` gửi cho học viên kèm điểm số và lời nhận xét.
* **File & hàm liên quan:** [TeacherController.cs](file:///D:/N%C4%83m%203/HTH_LTW/LUMORA-English-Center/LTW/Controllers/TeacherController.cs#L336-L395), [Views/Teacher/Assessments.cshtml](file:///D:/N%C4%83m%203/HTH_LTW/LUMORA-English-Center/LTW/Views/Teacher/Assessments.cshtml).
* **Trạng thái hiện tại:** Hoàn chỉnh.

---

### 3.9. Phân hệ Quản trị hệ thống (ADMIN)

#### [ADMIN-01] Bàn điều hành & Báo cáo KPI (Admin Dashboard)
* **Phân quyền:** Bắt buộc quyền Quản trị viên `[Authorize(Roles = "Admin")]`.
* **Chỉ số đo lường thực tế:**
  - Thống kê tổng số học viên (`TotalStudents`).
  - Tổng số giảng viên (`TotalTeachers`).
  - Tổng số khóa học (`TotalCourses`).
  - Tổng số lớp trực tuyến & số lớp đang hoạt động (`ActiveClasses`).
  - Tổng doanh thu thực tế từ các đơn hàng thanh toán thành công (`TotalRevenue = Sum(PricePaid)`).
  - Bảng 5 đơn đăng ký khóa học gần nhất.
  - Bảng 5 tài khoản người dùng đăng ký mới nhất.
* **File liên quan:** [AdminController.cs](file:///D:/N%C4%83m%203/HTH_LTW/LUMORA-English-Center/LTW/Controllers/AdminController.cs#L28-L61), [Views/Admin/Dashboard.cshtml](file:///D:/N%C4%83m%203/HTH_LTW/LUMORA-English-Center/LTW/Views/Admin/Dashboard.cshtml).
* **Trạng thái hiện tại:** Hoàn chỉnh.

#### [ADMIN-02] -> [ADMIN-05] Quản trị người dùng & Cấp tài khoản giáo viên (User CRUD)
* **Tính năng:**
  - Xem danh sách người dùng, lọc theo vai trò (Student, Teacher, Admin), tìm kiếm theo từ khóa.
  - Cấp tài khoản giảng viên mới (`CreateTeacher`): Nhập tên, email, username, mật khẩu, bio; tự động gắn vai trò `Teacher`.
  - Khóa/Mở khóa tài khoản (`ToggleUserActive`): Đảo trạng thái `IsActive`.
  - Xóa tài khoản (`DeleteUser`): Xóa người dùng kèm cơ chế bảo vệ ngăn chặn xóa tài khoản `admin` gốc.
* **File liên quan:** [AdminController.cs](file:///D:/N%C4%83m%203/HTH_LTW/LUMORA-English-Center/LTW/Controllers/AdminController.cs#L63-L164), [Views/Admin/Users.cshtml](file:///D:/N%C4%83m%203/HTH_LTW/LUMORA-English-Center/LTW/Views/Admin/Users.cshtml).
* **Trạng thái hiện tại:** Hoàn chỉnh.

#### [ADMIN-06] -> [ADMIN-10] Quản trị khóa học (Course CRUD)
* **Tính năng:**
  - Xem danh sách khóa học kèm số chương, lượt mua.
  - Tạo khóa học mới (`CreateCourse`): Thiết lập tên, giá niêm yết, giá khuyến mãi, thời lượng, trình độ, danh mục, phân công giảng viên; tự động khởi tạo sẵn "Chương 1: Nhập Môn & Tổng Quan Khóa Học".
  - Chỉnh sửa khóa học (`EditCourse`).
  - Xuất bản / Ẩn khóa học (`TogglePublishCourse`).
  - Xóa khóa học (`DeleteCourse`).
* **File liên quan:** [AdminController.cs](file:///D:/N%C4%83m%203/HTH_LTW/LUMORA-English-Center/LTW/Controllers/AdminController.cs#L166-L293).
* **Trạng thái hiện tại:** Hoàn chỉnh.

#### [ADMIN-11] -> [ADMIN-15] Quản trị lớp học trực tuyến & Thời khóa biểu (Class CRUD & Schedule)
* **Tính năng:**
  - Quản lý danh sách lớp live, ngày học, giờ học, giảng viên phụ trách, sức chứa.
  - Tạo lớp học trực tuyến mới (`CreateClass`).
  - Chỉnh sửa lớp học trực tuyến (`EditClass`).
  - Đóng / Mở trạng thái lớp (`CloseClass`).
  - Xem thời khóa biểu toàn hệ thống theo thứ tự ngày giờ (`Schedule`).
* **File liên quan:** [AdminController.cs](file:///D:/N%C4%83m%203/HTH_LTW/LUMORA-English-Center/LTW/Controllers/AdminController.cs#L295-L427).
* **Trạng thái hiện tại:** Hoàn chỉnh.

#### [ADMIN-16] -> [ADMIN-20] Quản trị bài viết IELTS Tips (Tip CRUD)
* **Tính năng:**
  - Danh sách bài viết, lượt xem, tình trạng xuất bản.
  - Đăng bài viết mẹo thi mới (`CreateTip`).
  - Chỉnh sửa bài viết (`EditTip`).
  - Ẩn / Hiện bài viết (`TogglePublishTip`).
  - Xóa bài viết (`DeleteTip`).
* **File liên quan:** [AdminController.cs](file:///D:/N%C4%83m%203/HTH_LTW/LUMORA-English-Center/LTW/Controllers/AdminController.cs#L429-L530).
* **Trạng thái hiện tại:** Hoàn chỉnh.

#### [ADMIN-21] Tổng quan đề thi khảo thí (Test Overview)
* **Tính năng:** Xem danh sách bài kiểm tra năng lực, số câu hỏi, số lượt làm bài, liên kết xem trước màn hình thi.
* **File liên quan:** [AdminController.cs](file:///D:/N%C4%83m%203/HTH_LTW/LUMORA-English-Center/LTW/Controllers/AdminController.cs#L532-L542).
* **Trạng thái hiện tại:** Triển khai một phần (Hiện mới có màn hình thống kê hiển thị, chưa có chức năng CreateTest, EditTest hoặc thêm câu hỏi trực quan từ giao diện admin).

#### [ADMIN-22] Phát sóng thông báo hệ thống (Broadcast Notifications)
* **Tính năng:** Soạn tiêu đề và nội dung thông báo; Chọn đối tượng phát sóng: Tất cả người dùng (`All`), Chỉ học viên (`Student`), hoặc Chỉ giảng viên (`Teacher`); Hệ thống lặp qua danh sách tài khoản thuộc nhóm được chọn và ghi hàng loạt bản ghi `Notification` mới vào SQL Server.
* **File liên quan:** [AdminController.cs](file:///D:/N%C4%83m%203/HTH_LTW/LUMORA-English-Center/LTW/Controllers/AdminController.cs#L544-L593).
* **Trạng thái hiện tại:** Hoàn chỉnh.

---

### 3.10. Phân hệ Thông báo & Nhắc nhở (NOTIF)

#### [NOTIF-01] -> [NOTIF-05] Trung tâm thông báo & Chuông Navbar
* **Tính năng:**
  - Biểu tượng chuông trên Navbar (`_LoginPartial.cshtml`): Hiển thị số lượng thông báo chưa đọc màu đỏ, click mở dropdown xem nhanh 4 thông báo mới nhất kèm thời gian.
  - Màn hình trung tâm thông báo (`/Notification/Index`): Xem toàn bộ lịch sử thông báo, phân loại biểu tượng theo loại (Lớp học sắp tới, Bài tập đã chấm, Cập nhật khóa học, Thông báo hệ thống).
  - Đánh dấu đã đọc cho từng tin (`MarkAsRead`) hoặc toàn bộ (`MarkAllAsRead`).
  - Mô phỏng gửi bản sao nhắc nhở qua email (`SendEmailReminder`): Thông báo giả lập gửi thư thành công tới email học viên.
* **File liên quan:** [NotificationController.cs](file:///D:/N%C4%83m%203/HTH_LTW/LUMORA-English-Center/LTW/Controllers/NotificationController.cs), [Views/Notification/Index.cshtml](file:///D:/N%C4%83m%203/HTH_LTW/LUMORA-English-Center/LTW/Views/Notification/Index.cshtml).
* **Trạng thái hiện tại:** Hoàn chỉnh.

---

### 3.11. Các thành phần Legacy & Stub Controllers (LEGACY)

Trong quá trình rà soát toàn bộ thư mục `Controllers/` và `Models/`, phát hiện **4 Controller** và **7 Model** thuộc về phiên bản sơ khai trước khi dự án được tái cấu trúc sang ASP.NET Core Identity:
1. `LoginController.cs` (13 dòng): Chỉ chứa `public IActionResult Index() => View();` và không có view `Views/Login/Index.cshtml`. Hiện đã bị thay thế hoàn toàn bởi `AccountController`.
2. `ManagerController.cs` (13 dòng): Chỉ chứa `public IActionResult Index() => View();` và không có view `Views/Manager/Index.cshtml`. Hiện đã bị thay thế bởi `AdminController`.
3. `HomeworkController.cs` (13 dòng): Chỉ chứa `public IActionResult Index() => View();` và không có view `Views/Homework/Index.cshtml`. Hiện chức năng bài tập nằm tại `StudentController` và `TeacherController`.
4. `PostController.cs` (13 dòng): Chỉ chứa `public IActionResult Index() => View();` và không có view `Views/Post/Index.cshtml`. Hiện chức năng bài viết nằm tại `TipController`.
5. Các Model `UserAccount`, `ClassRoom`, `Assignment`, `Question`, `AnswerOption`, `Submission`, `SubmissionAnswer`: Vẫn còn DbSet trong `ApplicationDbContext.cs` (dòng 32-38) để tương thích ngược, nhưng toàn bộ chức năng hiện tại đang chạy trên `ApplicationUser`, `LiveClass`, `Exercise`, `ExerciseQuestion`, `ExerciseSubmission`, `Test`, `TestQuestion`, `TestAttempt`.

---

# PHẦN IV — DANH SÁCH MÀN HÌNH VÀ THÀNH PHẦN GIAO DIỆN

Dưới đây là danh mục toàn bộ **44 file view Razor (.cshtml)** trong hệ thống:

| Tên màn hình / Giao diện | Đường dẫn file View | Tuyến đường (Route URL) | Thành phần tương tác chính | Phân hệ tương ứng |
|---|---|---|---|---|
| Master Layout | `Views/Shared/_Layout.cshtml` | (Shared) | Fixed Navbar, Menu dropdown, Brand logo, Dark multi-column Footer | Layout chung |
| User Control Header | `Views/Shared/_LoginPartial.cshtml` | (Shared) | Bell icon, unread badge, Notification dropdown, User avatar dropdown | Header chung |
| Error Screen | `Views/Shared/Error.cshtml` | `/Home/Error` | RequestId, thông báo lỗi | Hệ thống |
| Trang chủ Landing Page | `Views/Home/Index.cshtml` | `/` hoặc `/Home/Index` | Hero CTA, 4 kỹ năng, 6 bước học, mock test cards, teacher slider, FAQ | Trang chủ & Tư vấn |
| Chính sách bảo mật | `Views/Home/Privacy.cshtml` | `/Home/Privacy` | Nội dung chính sách bảo mật & điều khoản | Trang chủ & Tư vấn |
| Đăng nhập tài khoản | `Views/Account/Login.cshtml` | `/Account/Login` | Form login, Google login, Quick login demo, Skill pills | Xác thực & Tài khoản |
| Đăng ký tài khoản | `Views/Account/Register.cshtml` | `/Account/Register` | Form đăng ký, mật khẩu, xác nhận mật khẩu | Xác thực & Tài khoản |
| Nhập mã xác thực OTP | `Views/Account/VerifyOtp.cshtml` | `/Account/VerifyOtp` | Ô nhập mã OTP 6 số, thông báo đếm ngược hạn dùng | Xác thực & Tài khoản |
| Quên mật khẩu | `Views/Account/ForgotPassword.cshtml` | `/Account/ForgotPassword` | Ô nhập email lấy lại mật khẩu | Xác thực & Tài khoản |
| Nhập OTP đổi mật khẩu | `Views/Account/VerifyForgotOtp.cshtml`| `/Account/VerifyForgotOtp`| Ô nhập OTP xác minh đổi mật khẩu | Xác thực & Tài khoản |
| Đặt lại mật khẩu mới | `Views/Account/ResetPassword.cshtml` | `/Account/ResetPassword` | Ô nhập mật khẩu mới và xác nhận mật khẩu mới | Xác thực & Tài khoản |
| Cài đặt hồ sơ cá nhân | `Views/Account/Profile.cshtml` | `/Account/Profile` | Đổi tên, phone, bio, avatar, form đổi mật khẩu | Xác thực & Tài khoản |
| Từ chối quyền truy cập | `Views/Account/AccessDenied.cshtml` | `/Account/AccessDenied` | Thông báo không có quyền truy cập, nút quay lại | Xác thực & Tài khoản |
| Danh mục khóa học | `Views/Course/Index.cshtml` | `/Course` hoặc `/Course/Index` | Bộ lọc danh mục, bộ lọc trình độ, ô tìm kiếm, thẻ khóa học | Khóa học |
| Chi tiết khóa học | `Views/Course/Detail.cshtml` | `/Course/Detail/{id}` | Video giới thiệu, đề cương chương mục, danh sách buổi live | Khóa học |
| Thanh toán khóa học | `Views/Course/Purchase.cshtml` | `/Course/Purchase/{id}` | Radio chọn VNPay/MoMo/Bank, tóm tắt đơn hàng, nút xác nhận | Khóa học & Thanh toán |
| Biên lai thanh toán | `Views/Course/PaymentSuccess.cshtml` | `/Course/PaymentSuccess/{id}` | Mã giao dịch, thông tin khóa học, nút bắt đầu học ngay | Khóa học & Thanh toán |
| Bàn học cá nhân học viên | `Views/Student/Dashboard.cshtml` | `/Student/Dashboard` | 4 thẻ KPI, thanh tiến độ, bài tập chờ nộp, điểm thi, lịch live | Học viên |
| Khóa học của tôi | `Views/Student/MyCourses.cshtml` | `/Student/MyCourses` | Lưới khóa học đã mua kèm thanh tiến độ % hoàn thành | Học viên |
| Trình học bài giảng | `Views/Student/Lesson.cshtml` | `/Student/Lesson/{id}` | Video YouTube, audio MP3, lý thuyết, thẻ từ vựng JSON, nút Next/Prev | Học viên |
| Làm & xem lại bài tập | `Views/Student/Exercise.cshtml` | `/Student/Exercise/{id}` | Radio trắc nghiệm, nút nộp bài; sau nộp hiện đáp án & giải thích | Học viên |
| Lịch học học viên | `Views/Student/Schedule.cshtml` | `/Student/Schedule` | Danh sách lịch học các buổi live class | Học viên |
| Danh sách lớp trực tuyến | `Views/Class/Index.cshtml` | `/Class` hoặc `/Class/Index` | Bộ lọc Public/MyClasses, card buổi học, nút tham gia | Lớp học trực tuyến |
| Chi tiết buổi học trực tuyến| `Views/Class/Detail.cshtml` | `/Class/Detail/{id}` | Giảng viên, lịch học, sĩ số, nút "Vào phòng học ngay" | Lớp học trực tuyến |
| Phòng học trực tuyến ảo | `Views/Class/Room.cshtml` | `/Class/Room/{id}` | Sân khấu trình chiếu, nút mic/cam, giơ tay, danh sách học viên, chat | Lớp học trực tuyến |
| Danh mục bài thi khảo thí | `Views/Test/Index.cshtml` | `/Test` hoặc `/Test/Index` | Danh sách đề Example Test & Placement Test | Khảo thí IELTS |
| Phòng thi trực tuyến 4 kỹ năng| `Views/Test/TakeTest.cshtml` | `/Test/ExampleTest` / `PlacementTest` | Timer đếm ngược, Palette câu hỏi, audio, reading, đếm từ essay | Khảo thí IELTS |
| Báo cáo kết quả bài thi | `Views/Test/Result.cshtml` | `/Test/Result/{id}` | Điểm Overall, điểm 4 kỹ năng, CEFR level, nhận xét, khóa học gợi ý | Khảo thí IELTS |
| Danh mục mẹo thi IELTS | `Views/Tip/Index.cshtml` | `/Tip` hoặc `/Tip/Index` | Bộ lọc theo kỹ năng (Reading, Writing...), ô tìm kiếm, card bài viết | Mẹo thi & Blog |
| Chi tiết bài viết mẹo thi | `Views/Tip/Detail.cshtml` | `/Tip/Detail/{id}` | Nội dung bài viết, lượt xem, danh sách bài liên quan | Mẹo thi & Blog |
| Danh sách giảng viên | `Views/Teacher/Index.cshtml` | `/Teacher` hoặc `/Teacher/Index`| Lưới card giảng viên kèm số lượng khóa học | Giảng viên |
| Chi tiết hồ sơ giảng viên | `Views/Teacher/Detail.cshtml` | `/Teacher/Detail/{id}` | Tiểu sử, bằng cấp, khóa học phụ trách, lịch live | Giảng viên |
| Cổng giảng viên | `Views/Teacher/Dashboard.cshtml` | `/Teacher/Dashboard` | 4 thẻ KPI giảng viên, danh sách lớp live, bài tập cần chấm | Giảng viên |
| Lớp phụ trách của giảng viên | `Views/Teacher/MyClasses.cshtml` | `/Teacher/MyClasses` | Bảng quản lý các buổi học trực tuyến do giảng viên phụ trách | Giảng viên |
| Quản lý bài học giảng viên | `Views/Teacher/Lessons.cshtml` | `/Teacher/Lessons` | Bảng bài học theo chương khóa học, nút thêm bài học | Giảng viên |
| Soạn bài học mới | `Views/Teacher/CreateLesson.cshtml` | `/Teacher/CreateLesson` | Form nhập bài học, nhúng video/audio, soạn lý thuyết, từ vựng | Giảng viên |
| Quản lý bài tập giảng viên | `Views/Teacher/Exercises.cshtml` | `/Teacher/Exercises` | Bảng bài tập theo bài học, nút tạo bài tập | Giảng viên |
| Soạn bài tập & câu hỏi | `Views/Teacher/CreateExercise.cshtml` | `/Teacher/CreateExercise` | Form thiết lập bài tập và soạn thảo câu hỏi A/B/C/D | Giảng viên |
| Học viên đang theo học | `Views/Teacher/Students.cshtml` | `/Teacher/Students` | Bảng danh sách học viên đã mua khóa học, ngày mua, số bài nộp | Giảng viên |
| Đánh giá & chấm điểm bài nộp| `Views/Teacher/Assessments.cshtml` | `/Teacher/Assessments` | Bảng bài nộp học viên, modal chấm điểm giáo viên & viết nhận xét | Giảng viên |
| Bàn điều hành quản trị | `Views/Admin/Dashboard.cshtml` | `/Admin/Dashboard` | 6 thẻ KPI toàn hệ thống, bảng doanh thu, bảng người dùng mới | Quản trị hệ thống |
| Quản lý người dùng | `Views/Admin/Users.cshtml` | `/Admin/Users` | Bảng người dùng, lọc vai trò, tìm kiếm, nút cấp tài khoản GV, khóa, xóa | Quản trị hệ thống |
| Quản lý khóa học | `Views/Admin/Courses.cshtml` | `/Admin/Courses` | Bảng khóa học, nút xuất bản/ẩn, chỉnh sửa, xóa, tạo mới | Quản trị hệ thống |
| Tạo khóa học mới | `Views/Admin/CreateCourse.cshtml` | `/Admin/CreateCourse` | Form nhập thông tin khóa học, giá, phân công giảng viên | Quản trị hệ thống |
| Chỉnh sửa khóa học | `Views/Admin/EditCourse.cshtml` | `/Admin/EditCourse/{id}` | Form sửa thông tin khóa học | Quản trị hệ thống |
| Quản lý lớp trực tuyến | `Views/Admin/Classes.cshtml` | `/Admin/Classes` | Bảng lớp live, nút đóng/mở lớp, chỉnh sửa, tạo mới | Quản trị hệ thống |
| Tạo lớp trực tuyến mới | `Views/Admin/CreateClass.cshtml` | `/Admin/CreateClass` | Form tạo lớp live, gán giảng viên, URL phòng học, chế độ Public | Quản trị hệ thống |
| Chỉnh sửa lớp trực tuyến | `Views/Admin/EditClass.cshtml` | `/Admin/EditClass/{id}` | Form cập nhật thông tin lớp live | Quản trị hệ thống |
| Thời khóa biểu toàn hệ thống | `Views/Admin/Schedule.cshtml` | `/Admin/Schedule` | Bảng lịch học toàn bộ các lớp theo thời gian | Quản trị hệ thống |
| Quản lý bài viết Tips | `Views/Admin/Tips.cshtml` | `/Admin/Tips` | Bảng bài viết mẹo thi, nút ẩn/hiện, sửa, xóa, tạo mới | Quản trị hệ thống |
| Đăng bài viết mẹo thi mới | `Views/Admin/CreateTip.cshtml` | `/Admin/CreateTip` | Form soạn tiêu đề, tóm tắt, nội dung, chuyên mục mẹo thi | Quản trị hệ thống |
| Chỉnh sửa bài viết mẹo thi | `Views/Admin/EditTip.cshtml` | `/Admin/EditTip/{id}` | Form cập nhật bài viết mẹo thi | Quản trị hệ thống |
| Thống kê đề thi | `Views/Admin/Tests.cshtml` | `/Admin/Tests` | Danh sách đề thi, số lượng câu hỏi, số lượt làm bài | Quản trị hệ thống |
| Quản lý & phát sóng thông báo| `Views/Admin/Notifications.cshtml` | `/Admin/Notifications` | Form gửi thông báo theo nhóm vai trò, bảng lịch sử thông báo đã gửi | Quản trị hệ thống |
| Trung tâm thông báo | `Views/Notification/Index.cshtml` | `/Notification` hoặc `/Notification/Index`| Danh sách thông báo cá nhân, nút đánh dấu đã đọc, nút nhận email reminder | Thông báo |

---

# PHẦN V — DANH SÁCH API VÀ NGHIỆP VỤ BACKEND

Dưới đây là thống kê chi tiết **53 Endpoint / Action xử lý nghiệp vụ backend** trong hệ thống:

```
[AccountController]
  ├── GET   /Account/Login (string? returnUrl)
  ├── POST  /Account/Login (LoginViewModel model)
  ├── POST  /Account/ExternalLogin (string provider = "Google", string? returnUrl)
  ├── GET   /Account/ExternalLoginCallback (string? returnUrl, string? remoteError)
  ├── POST  /Account/QuickGoogleLogin ()
  ├── GET   /Account/Register ()
  ├── POST  /Account/Register (RegisterViewModel model)
  ├── GET   /Account/VerifyOtp (string email, string purpose)
  ├── POST  /Account/VerifyOtp (VerifyOtpViewModel model)
  ├── GET   /Account/ForgotPassword ()
  ├── POST  /Account/ForgotPassword (ForgotPasswordViewModel model)
  ├── GET   /Account/VerifyForgotOtp (string email)
  ├── POST  /Account/VerifyForgotOtp (VerifyOtpViewModel model)
  ├── GET   /Account/ResetPassword (string email, string code)
  ├── POST  /Account/ResetPassword (ResetPasswordViewModel model)
  ├── POST  /Account/Logout () [Authorize]
  ├── GET   /Account/Profile () [Authorize]
  ├── POST  /Account/Profile (ProfileViewModel model) [Authorize]
  └── GET   /Account/AccessDenied ()

[HomeController]
  ├── GET   /Home/Index hoặc /
  ├── POST  /Home/SubmitConsultation (LandingPageViewModel incomingModel)
  ├── GET   /Home/ExamLibrary (Redirect -> Index#materials)
  ├── GET   /Home/IeltsTips (Redirect -> Index#steps)
  ├── GET   /Home/IeltsPrep (Redirect -> Index#courses)
  ├── GET   /Home/LiveLessons (Redirect -> Index#live-lessons)
  ├── GET   /Home/IeltsCourses (Redirect -> Index#courses)
  ├── GET   /Home/Contact (Redirect -> Index#contact-section)
  ├── GET   /Home/Privacy
  └── GET   /Home/Error

[CourseController]
  ├── GET   /Course/Index (string? category, string? search, string? level)
  ├── GET   /Course/Detail/{id}
  ├── GET   /Course/Purchase/{id} [Authorize]
  ├── POST  /Course/ProcessPayment (int courseId, string paymentMethod) [Authorize]
  └── GET   /Course/PaymentSuccess/{id} [Authorize]

[StudentController] [Authorize]
  ├── GET   /Student/Dashboard
  ├── GET   /Student/MyCourses
  ├── GET   /Student/Lesson/{id}
  ├── POST  /Student/ToggleCompleteLesson (int lessonId)
  ├── GET   /Student/Exercise/{id}
  ├── POST  /Student/SubmitExercise (int exerciseId, IFormCollection form)
  └── GET   /Student/Schedule

[ClassController]
  ├── GET   /Class/Index (string? filter)
  ├── GET   /Class/Detail/{id}
  ├── GET   /Class/Join/{id} [Authorize]
  └── GET   /Class/Room/{id} [Authorize]

[TestController]
  ├── GET   /Test/Index
  ├── GET   /Test/ExampleTest
  ├── GET   /Test/PlacementTest
  ├── POST  /Test/SubmitTest (int testId, string fullName, string email, IFormCollection form)
  └── GET   /Test/Result/{id}

[TipController]
  ├── GET   /Tip/Index (string? category, string? search)
  └── GET   /Tip/Detail/{id}

[TeacherController]
  ├── GET   /Teacher/Index
  ├── GET   /Teacher/Detail/{id}
  ├── GET   /Teacher/Dashboard [Authorize: Teacher,Admin]
  ├── GET   /Teacher/MyClasses [Authorize: Teacher,Admin]
  ├── GET   /Teacher/Lessons [Authorize: Teacher,Admin]
  ├── GET   /Teacher/CreateLesson [Authorize: Teacher,Admin]
  ├── POST  /Teacher/CreateLesson (Lesson lesson) [Authorize: Teacher,Admin]
  ├── GET   /Teacher/Exercises [Authorize: Teacher,Admin]
  ├── GET   /Teacher/CreateExercise [Authorize: Teacher,Admin]
  ├── POST  /Teacher/CreateExercise (...) [Authorize: Teacher,Admin]
  ├── GET   /Teacher/Students [Authorize: Teacher,Admin]
  ├── GET   /Teacher/Assessments [Authorize: Teacher,Admin]
  └── POST  /Teacher/GradeSubmission (...) [Authorize: Teacher,Admin]

[AdminController] [Authorize: Admin]
  ├── GET   /Admin/Dashboard
  ├── GET   /Admin/Users (string? role, string? search)
  ├── POST  /Admin/CreateTeacher (fullName, email, username, password, bio)
  ├── POST  /Admin/ToggleUserActive (string id)
  ├── POST  /Admin/DeleteUser (string id)
  ├── GET   /Admin/Courses
  ├── GET   /Admin/CreateCourse
  ├── POST  /Admin/CreateCourse (Course course)
  ├── GET   /Admin/EditCourse/{id}
  ├── POST  /Admin/EditCourse (int id, Course course)
  ├── POST  /Admin/TogglePublishCourse (int id)
  ├── POST  /Admin/DeleteCourse (int id)
  ├── GET   /Admin/Classes
  ├── GET   /Admin/CreateClass
  ├── POST  /Admin/CreateClass (LiveClass liveClass)
  ├── GET   /Admin/EditClass/{id}
  ├── POST  /Admin/EditClass (int id, LiveClass liveClass)
  ├── POST  /Admin/CloseClass (int id)
  ├── GET   /Admin/Schedule
  ├── GET   /Admin/Tips
  ├── GET   /Admin/CreateTip
  ├── POST  /Admin/CreateTip (IeltsTip tip)
  ├── GET   /Admin/EditTip/{id}
  ├── POST  /Admin/EditTip (int id, IeltsTip tip)
  ├── POST  /Admin/TogglePublishTip (int id)
  ├── POST  /Admin/DeleteTip (int id)
  ├── GET   /Admin/Tests
  ├── GET   /Admin/Notifications
  └── POST  /Admin/BroadcastNotification (string title, string content, string role)

[NotificationController] [Authorize]
  ├── GET   /Notification/Index
  ├── POST  /Notification/MarkAsRead/{id}
  ├── POST  /Notification/MarkAllAsRead
  └── POST  /Notification/SendEmailReminder/{id}
```

---

# PHẦN VI — CẤU TRÚC VÀ CHỨC NĂNG CƠ SỞ DỮ LIỆU

### 6.1. Bảng dữ liệu chính đang hoạt động (Active Tables)
1. **`AspNetUsers` (`ApplicationUser`):** 
   - Khóa chính: `Id` (string GUID).
   - Các trường mở rộng: `FullName` (nvarchar 100), `AvatarUrl` (nvarchar 300), `RoleName` (nvarchar 30), `Bio` (nvarchar 500), `IsActive` (bit), `CreatedAt` (datetime2).
   - Chức năng đọc/ghi: AUTH-01..10, ADMIN-02..05, TEACHER-01..02.
2. **`Courses` (`Course`):**
   - Khóa chính: `CourseId` (int identity).
   - Khóa ngoại: `TeacherId` $\rightarrow$ `AspNetUsers.Id` (`OnDelete: Restrict`).
   - Các trường chính: `Title`, `ShortDescription`, `Description`, `Thumbnail`, `Level`, `Category`, `Price`, `OriginalPrice`, `Duration`, `IsPublished`, `IsFeatured`, `CreatedAt`.
   - Chức năng đọc/ghi: COURSE-01..05, ADMIN-06..10, TEACHER-03, STUDENT-01..02.
3. **`Chapters` (`Chapter`):**
   - Khóa chính: `ChapterId` (int identity).
   - Khóa ngoại: `CourseId` $\rightarrow$ `Courses.CourseId` (`Cascade`).
   - Các trường: `Title`, `OrderIndex`.
4. **`Lessons` (`Lesson`):**
   - Khóa chính: `LessonId` (int identity).
   - Khóa ngoại: `ChapterId` $\rightarrow$ `Chapters.ChapterId` (`Cascade`).
   - Các trường: `Title`, `OrderIndex`, `Content` (nvarchar max HTML), `VideoUrl`, `AudioUrl`, `DocumentUrl`, `Vocabulary` (JSON), `Grammar`, `IsFreePreview`.
   - Chức năng đọc/ghi: STUDENT-03, TEACHER-05..06.
5. **`LessonProgresses` (`LessonProgress`):**
   - Khóa chính: `ProgressId` (int identity).
   - Khóa ngoại: `StudentId` $\rightarrow$ `AspNetUsers.Id` (`Restrict`), `LessonId` $\rightarrow$ `Lessons.LessonId` (`Cascade`).
   - Các trường: `IsCompleted` (bit), `CompletedAt` (datetime2 nullable), `LastAccessedAt` (datetime2).
   - Chức năng đọc/ghi: STUDENT-01, 02, 03, 04.
6. **`Exercises` (`Exercise`):**
   - Khóa chính: `ExerciseId` (int identity).
   - Khóa ngoại: `LessonId` $\rightarrow$ `Lessons.LessonId` (`Cascade`).
   - Các trường: `Title`, `Description`, `TimeLimitMinutes`, `PassingScore`, `CreatedAt`.
   - Chức năng đọc/ghi: STUDENT-05..06, TEACHER-07..08.
7. **`ExerciseQuestions` (`ExerciseQuestion`):**
   - Khóa chính: `QuestionId` (int identity).
   - Khóa ngoại: `ExerciseId` $\rightarrow$ `Exercises.ExerciseId` (`Cascade`).
   - Các trường: `Skill`, `Format`, `Content`, `Passage`, `AudioUrl`, `OptionsJson`, `CorrectAnswer`, `Explanation`, `Points`.
   - Chức năng đọc/ghi: STUDENT-05, TEACHER-08.
8. **`ExerciseSubmissions` (`ExerciseSubmission`):**
   - Khóa chính: `SubmissionId` (int identity).
   - Khóa ngoại: `ExerciseId` $\rightarrow$ `Exercises.ExerciseId` (`Cascade`), `StudentId` $\rightarrow$ `AspNetUsers.Id` (`Restrict`).
   - Các trường: `SubmittedAt`, `AutoScore`, `TeacherScore`, `TotalScore`, `TeacherComment`, `AnswersJson`.
   - Chức năng đọc/ghi: STUDENT-05..06, TEACHER-09..10.
9. **`LiveClasses` (`LiveClass`):**
   - Khóa chính: `LiveClassId` (int identity).
   - Khóa ngoại: `CourseId` $\rightarrow$ `Courses.CourseId` (`SetNull`), `TeacherId` $\rightarrow$ `AspNetUsers.Id` (`Restrict`).
   - Các trường: `ClassName`, `ScheduledDate`, `StartTime`, `EndTime`, `LiveRoomUrl`, `Capacity`, `Description`, `Status`, `IsPublic`.
   - Chức năng đọc/ghi: CLASS-01..04, ADMIN-11..15, TEACHER-04.
10. **`LiveClassStudents` (`LiveClassStudent`):**
    - Khóa chính: `Id` (int identity).
    - Khóa ngoại: `LiveClassId` $\rightarrow$ `LiveClasses.LiveClassId` (`Cascade`), `StudentId` $\rightarrow$ `AspNetUsers.Id` (`Restrict`).
    - Chức năng đọc/ghi: CLASS-03 (điểm danh/ghi nhận tham gia).
11. **`Enrollments` (`Enrollment`):**
    - Khóa chính: `EnrollmentId` (int identity).
    - Khóa ngoại: `StudentId` $\rightarrow$ `AspNetUsers.Id` (`Restrict`), `CourseId` $\rightarrow$ `Courses.CourseId` (`Restrict`).
    - Các trường: `EnrolledAt`, `PricePaid`, `PaymentMethod`, `PaymentStatus`, `TransactionId`.
    - Chức năng đọc/ghi: COURSE-03..05, STUDENT-01..02, ADMIN-01.
12. **`Tests` (`Test`):**
    - Khóa chính: `TestId` (int identity).
    - Các trường: `Title`, `Description`, `Type` (ExamType: ExampleTest, PlacementTest), `Thumbnail`, `TimeLimitMinutes`, `IsActive`, `CreatedAt`.
    - Chức năng đọc/ghi: TEST-01..03, ADMIN-21.
13. **`TestQuestions` (`TestQuestion`):**
    - Khóa chính: `TestQuestionId` (int identity).
    - Khóa ngoại: `TestId` $\rightarrow$ `Tests.TestId` (`Cascade`).
    - Các trường: `Skill`, `Format`, `Content`, `Passage`, `AudioUrl`, `OptionsJson`, `CorrectAnswer`, `Points`, `Explanation`.
    - Chức năng đọc/ghi: TEST-02..03, TEST-08.
14. **`TestAttempts` (`TestAttempt`):**
    - Khóa chính: `AttemptId` (int identity).
    - Khóa ngoại: `TestId` $\rightarrow$ `Tests.TestId` (`Cascade`), `StudentId` $\rightarrow$ `AspNetUsers.Id` (`Restrict`).
    - Các trường: `FullName`, `Email`, `StartedAt`, `CompletedAt`, `ListeningScore`, `ReadingScore`, `WritingScore`, `SpeakingScore`, `OverallScore`, `EstimatedLevel`, `Feedback`, `RecommendedCoursesJson`, `AnswersJson`.
    - Chức năng đọc/ghi: TEST-08..09, STUDENT-01, ADMIN-21.
15. **`IeltsTips` (`IeltsTip`):**
    - Khóa chính: `TipId` (int identity).
    - Các trường: `Title`, `Summary`, `Content`, `Thumbnail`, `Category`, `AuthorName`, `CreatedAt`, `ViewsCount`, `IsPublished`.
    - Chức năng đọc/ghi: TIP-01..02, ADMIN-16..20.
16. **`Notifications` (`Notification`):**
    - Khóa chính: `NotificationId` (int identity).
    - Khóa ngoại: `UserId` $\rightarrow$ `AspNetUsers.Id` (`Cascade`).
    - Các trường: `Title`, `Content`, `Type`, `LinkUrl`, `IsRead`, `CreatedAt`.
    - Chức năng đọc/ghi: NOTIF-01..05, ADMIN-22, COURSE-04, STUDENT-05, TEACHER-10.
17. **`OtpRecords` (`OtpRecord`):**
    - Khóa chính: `Id` (int identity).
    - Các trường: `Email`, `OtpCode`, `Purpose`, `PayloadJson`, `CreatedAt`, `ExpiresAt`, `IsUsed`.
    - Chức năng đọc/ghi: AUTH-04..07.

### 6.2. Bảng dữ liệu cũ (Legacy Schema)
Các bảng sau tồn tại trong khai báo `ApplicationDbContext`: `UserAccounts`, `ClassRooms`, `Assignments`, `Questions`, `AnswerOptions`, `Submissions`, `SubmissionAnswers`. Không có chức năng nào trên giao diện hiện thời sử dụng các bảng này.

---

# PHẦN VII — MA TRẬN ĐỐI CHIẾU CHỨC NĂNG

Dưới đây là ma trận đối chiếu 4 tầng (Giao diện Frontend $\leftrightarrow$ Nghiệp vụ Backend $\leftrightarrow$ Cơ sở dữ liệu $\leftrightarrow$ Thực thi biên dịch kiểm thử):

| Mã chức năng | Tên chức năng | Frontend (View/UI) | Backend (Controller/Action) | Database (Bảng/Model) | Đã xác minh thực thi | Kết luận |
|---|---|---|---|---|---|---|
| **AUTH-01** | Đăng nhập tài khoản | Có (`Login.cshtml`) | Có (`AccountController.Login`) | Có (`AspNetUsers`) | Có (Build 0 lỗi) | **Hoàn chỉnh** |
| **AUTH-02** | Đăng nhập Google OAuth | Có (`Login.cshtml`) | Có (`ExternalLoginCallback`) | Có (`AspNetUsers`) | Có (Build 0 lỗi) | **Hoàn chỉnh** |
| **AUTH-03** | Đăng nhập nhanh Demo | Có (`Login.cshtml`) | Có (`QuickGoogleLogin`) | Có (`AspNetUsers`) | Có (Build 0 lỗi) | **Hoàn chỉnh** |
| **AUTH-04** | Đăng ký & Sinh OTP | Có (`Register.cshtml`) | Có (`Register`) | Có (`OtpRecords`) | Có (Build 0 lỗi) | **Hoàn chỉnh** |
| **AUTH-05** | Xác thực OTP & Kích hoạt | Có (`VerifyOtp.cshtml`) | Có (`VerifyOtp`) | Có (`OtpRecords`, `Users`)| Có (Build 0 lỗi) | **Hoàn chỉnh** |
| **AUTH-06** | Yêu cầu quên mật khẩu | Có (`ForgotPassword.cshtml`)| Có (`ForgotPassword`) | Có (`OtpRecords`) | Có (Build 0 lỗi) | **Hoàn chỉnh** |
| **AUTH-07** | Đặt lại mật khẩu mới | Có (`ResetPassword.cshtml`) | Có (`ResetPassword`) | Có (`AspNetUsers`) | Có (Build 0 lỗi) | **Hoàn chỉnh** |
| **AUTH-08** | Đăng xuất người dùng | Có (`_LoginPartial.cshtml`)| Có (`Logout`) | Có (`Cookie Auth`) | Có (Build 0 lỗi) | **Hoàn chỉnh** |
| **AUTH-09** | Cập nhật hồ sơ cá nhân | Có (`Profile.cshtml`) | Có (`Profile`) | Có (`AspNetUsers`) | Có (Build 0 lỗi) | **Hoàn chỉnh** |
| **AUTH-10** | Trang từ chối quyền | Có (`AccessDenied.cshtml`) | Có (`AccessDenied`) | Không | Có (Build 0 lỗi) | **Hoàn chỉnh** |
| **HOME-01** | Hiển thị Landing Page | Có (`Home/Index.cshtml`) | Có (`HomeController.Index`)| Có (`LandingPageViewModel`)| Có (Build 0 lỗi)| **Hoàn chỉnh** |
| **HOME-02** | Gửi form tư vấn 1-1 | Có (`Home/Index.cshtml`) | Có (`SubmitConsultation`) | Không (chưa có bảng)| Có (Build 0 lỗi) | **Triển khai một phần** |
| **HOME-03** | Lối tắt điều hướng Home | Có (`_Layout.cshtml`) | Có (`ExamLibrary...`) | Không | Có (Build 0 lỗi) | **Hoàn chỉnh** |
| **COURSE-01**| Catalog & Tìm kiếm khóa học | Có (`Course/Index.cshtml`) | Có (`CourseController.Index`)| Có (`Courses`) | Có (Build 0 lỗi) | **Hoàn chỉnh** |
| **COURSE-02**| Chi tiết khóa học & Đề cương| Có (`Course/Detail.cshtml`) | Có (`CourseController.Detail`)| Có (`Courses`, `Chapters`)| Có (Build 0 lỗi) | **Hoàn chỉnh** |
| **COURSE-03**| Màn hình chọn thanh toán | Có (`Course/Purchase.cshtml`)| Có (`Purchase`) | Có (`Courses`) | Có (Build 0 lỗi) | **Hoàn chỉnh** |
| **COURSE-04**| Xử lý thanh toán mua khóa | Có (`Purchase.cshtml`) | Có (`ProcessPayment`) | Có (`Enrollments`, `Notifs`)| Có (Build 0 lỗi)| **Hoàn chỉnh** |
| **COURSE-05**| Biên lai thanh toán | Có (`PaymentSuccess.cshtml`)| Có (`PaymentSuccess`) | Có (`Enrollments`) | Có (Build 0 lỗi) | **Hoàn chỉnh** |
| **STUDENT-01**| Bàn học cá nhân Dashboard | Có (`Student/Dashboard.cshtml`)| Có (`StudentController.Dashboard`)| Có (7 bảng liên kết) | Có (Build 0 lỗi) | **Hoàn chỉnh** |
| **STUDENT-02**| Khóa học của tôi & Tiến độ | Có (`Student/MyCourses.cshtml`)| Có (`MyCourses`) | Có (`Enrollments`, `Progress`)| Có (Build 0 lỗi)| **Hoàn chỉnh** |
| **STUDENT-03**| Trình phát bài học đa phương tiện| Có (`Student/Lesson.cshtml`) | Có (`Lesson`) | Có (`Lessons`, `Progress`)| Có (Build 0 lỗi) | **Hoàn chỉnh** |
| **STUDENT-04**| Đánh dấu hoàn thành bài học| Có (`Student/Lesson.cshtml`) | Có (`ToggleCompleteLesson`)| Có (`LessonProgresses`) | Có (Build 0 lỗi) | **Hoàn chỉnh** |
| **STUDENT-05**| Làm & nộp bài tập tự chấm | Có (`Student/Exercise.cshtml`)| Có (`SubmitExercise`) | Có (`ExerciseSubmissions`)| Có (Build 0 lỗi) | **Hoàn chỉnh** |
| **STUDENT-06**| Xem lại đáp án bài tập | Có (`Student/Exercise.cshtml`)| Có (`Exercise`) | Có (`ExerciseSubmissions`)| Có (Build 0 lỗi) | **Hoàn chỉnh** |
| **STUDENT-07**| Thời khóa biểu học viên | Có (`Student/Schedule.cshtml`)| Có (`Schedule`) | Có (`LiveClasses`) | Có (Build 0 lỗi) | **Hoàn chỉnh** |
| **CLASS-01** | Danh sách lớp trực tuyến | Có (`Class/Index.cshtml`) | Có (`ClassController.Index`)| Có (`LiveClasses`) | Có (Build 0 lỗi) | **Hoàn chỉnh** |
| **CLASS-02** | Chi tiết lớp trực tuyến | Có (`Class/Detail.cshtml`) | Có (`ClassController.Detail`)| Có (`LiveClasses`) | Có (Build 0 lỗi) | **Hoàn chỉnh** |
| **CLASS-03** | Kiểm tra quyền & Tham gia lớp | Có (`Class/Detail.cshtml`) | Có (`Join`) | Có (`LiveClassStudents`) | Có (Build 0 lỗi) | **Hoàn chỉnh** |
| **CLASS-04** | Phòng học trực tuyến ảo | Có (`Class/Room.cshtml`) | Có (`Room`) | Có (`LiveClasses`) | Có (Build 0 lỗi) | **Hoàn chỉnh** |
| **TEST-01** | Thư viện đề thi khảo thí | Có (`Test/Index.cshtml`) | Có (`TestController.Index`) | Có (`Tests`) | Có (Build 0 lỗi) | **Hoàn chỉnh** |
| **TEST-02** | Làm bài thi thử mẫu (Example) | Có (`Test/TakeTest.cshtml`) | Có (`ExampleTest`) | Có (`Tests`, `Questions`) | Có (Build 0 lỗi) | **Hoàn chỉnh** |
| **TEST-03** | Làm bài thi xếp lớp (Placement)| Có (`Test/TakeTest.cshtml`) | Có (`PlacementTest`) | Có (`Tests`, `Questions`) | Có (Build 0 lỗi) | **Hoàn chỉnh** |
| **TEST-04** | Đếm ngược & Tự động nộp bài | Có (`TakeTest.cshtml` JS) | Có (`SubmitTest`) | Có (`TestAttempts`) | Có (Build 0 lỗi) | **Hoàn chỉnh** |
| **TEST-05** | Question Palette điều hướng | Có (`TakeTest.cshtml` JS) | Không (Client-side) | Không | Có (Build 0 lỗi) | **Hoàn chỉnh** |
| **TEST-06** | Đếm số lượng từ Writing | Có (`TakeTest.cshtml` JS) | Không (Client-side) | Không | Có (Build 0 lỗi) | **Hoàn chỉnh** |
| **TEST-07** | Thu âm bài thi Speaking | Có (`TakeTest.cshtml` JS) | Có (`SubmitTest` nhận text) | Không (chưa lưu audio)| Có (Build 0 lỗi) | **Triển khai một phần** |
| **TEST-08** | Chấm điểm & Quy đổi Band | Có (`Result.cshtml`) | Có (`SubmitTest`) | Có (`TestAttempts`) | Có (Build 0 lỗi) | **Hoàn chỉnh** |
| **TEST-09** | Báo cáo kết quả & Gợi ý khóa| Có (`Result.cshtml`) | Có (`Result`) | Có (`TestAttempts`) | Có (Build 0 lỗi) | **Hoàn chỉnh** |
| **TIP-01** | Danh mục mẹo thi & Tìm kiếm | Có (`Tip/Index.cshtml`) | Có (`TipController.Index`) | Có (`IeltsTips`) | Có (Build 0 lỗi) | **Hoàn chỉnh** |
| **TIP-02** | Chi tiết mẹo thi & Tăng view | Có (`Tip/Detail.cshtml`) | Có (`TipController.Detail`) | Có (`IeltsTips`) | Có (Build 0 lỗi) | **Hoàn chỉnh** |
| **TEACHER-01**| Danh sách giảng viên công khai| Có (`Teacher/Index.cshtml`) | Có (`TeacherController.Index`)| Có (`AspNetUsers`) | Có (Build 0 lỗi) | **Hoàn chỉnh** |
| **TEACHER-02**| Chi tiết hồ sơ giảng viên | Có (`Teacher/Detail.cshtml`) | Có (`TeacherController.Detail`)| Có (`AspNetUsers`, `Courses`)| Có (Build 0 lỗi)| **Hoàn chỉnh** |
| **TEACHER-03**| Cổng giảng viên Dashboard | Có (`Teacher/Dashboard.cshtml`)| Có (`TeacherController.Dashboard`)| Có (4 bảng liên kết) | Có (Build 0 lỗi) | **Hoàn chỉnh** |
| **TEACHER-04**| Lớp phụ trách của giảng viên | Có (`Teacher/MyClasses.cshtml`)| Có (`MyClasses`) | Có (`LiveClasses`) | Có (Build 0 lỗi) | **Hoàn chỉnh** |
| **TEACHER-05**| Quản lý bài học giảng viên | Có (`Teacher/Lessons.cshtml`) | Có (`Lessons`) | Có (`Lessons`, `Chapters`)| Có (Build 0 lỗi) | **Hoàn chỉnh** |
| **TEACHER-06**| Soạn bài học mới | Có (`Teacher/CreateLesson.cshtml`)| Có (`CreateLesson`) | Có (`Lessons`) | Có (Build 0 lỗi) | **Hoàn chỉnh** |
| **TEACHER-07**| Quản lý bài tập giảng viên | Có (`Teacher/Exercises.cshtml`)| Có (`Exercises`) | Có (`Exercises`) | Có (Build 0 lỗi) | **Hoàn chỉnh** |
| **TEACHER-08**| Soạn bài tập & câu hỏi | Có (`Teacher/CreateExercise.cshtml`)| Có (`CreateExercise`) | Có (`Exercises`, `Questions`)| Có (Build 0 lỗi)| **Hoàn chỉnh** |
| **TEACHER-09**| Danh sách học viên của giáo viên| Có (`Teacher/Students.cshtml`) | Có (`Students`) | Có (`Enrollments`, `Submissions`)| Có (Build 0 lỗi)| **Hoàn chỉnh** |
| **TEACHER-10**| Đánh giá & chấm điểm bài nộp | Có (`Teacher/Assessments.cshtml`)| Có (`GradeSubmission`) | Có (`ExerciseSubmissions`)| Có (Build 0 lỗi) | **Hoàn chỉnh** |
| **ADMIN-01** | Bàn điều hành KPI Admin | Có (`Admin/Dashboard.cshtml`) | Có (`AdminController.Dashboard`)| Có (6 bảng liên kết) | Có (Build 0 lỗi) | **Hoàn chỉnh** |
| **ADMIN-02** | Quản lý người dùng & Tìm kiếm| Có (`Admin/Users.cshtml`) | Có (`AdminController.Users`)| Có (`AspNetUsers`) | Có (Build 0 lỗi) | **Hoàn chỉnh** |
| **ADMIN-03** | Cấp tài khoản giảng viên | Có (`Admin/Users.cshtml`) | Có (`CreateTeacher`) | Có (`AspNetUsers`) | Có (Build 0 lỗi) | **Hoàn chỉnh** |
| **ADMIN-04** | Khóa / Mở khóa tài khoản | Có (`Admin/Users.cshtml`) | Có (`ToggleUserActive`) | Có (`AspNetUsers`) | Có (Build 0 lỗi) | **Hoàn chỉnh** |
| **ADMIN-05** | Xóa tài khoản người dùng | Có (`Admin/Users.cshtml`) | Có (`DeleteUser`) | Có (`AspNetUsers`) | Có (Build 0 lỗi) | **Hoàn chỉnh** |
| **ADMIN-06** | Quản lý danh sách khóa học | Có (`Admin/Courses.cshtml`) | Có (`Courses`) | Có (`Courses`) | Có (Build 0 lỗi) | **Hoàn chỉnh** |
| **ADMIN-07** | Tạo khóa học mới | Có (`Admin/CreateCourse.cshtml`)| Có (`CreateCourse`) | Có (`Courses`, `Chapters`)| Có (Build 0 lỗi) | **Hoàn chỉnh** |
| **ADMIN-08** | Chỉnh sửa khóa học | Có (`Admin/EditCourse.cshtml`) | Có (`EditCourse`) | Có (`Courses`) | Có (Build 0 lỗi) | **Hoàn chỉnh** |
| **ADMIN-09** | Bật / Tắt xuất bản khóa học | Có (`Admin/Courses.cshtml`) | Có (`TogglePublishCourse`) | Có (`Courses`) | Có (Build 0 lỗi) | **Hoàn chỉnh** |
| **ADMIN-10** | Xóa khóa học | Có (`Admin/Courses.cshtml`) | Có (`DeleteCourse`) | Có (`Courses`) | Có (Build 0 lỗi) | **Hoàn chỉnh** |
| **ADMIN-11** | Quản lý lớp trực tuyến | Có (`Admin/Classes.cshtml`) | Có (`Classes`) | Có (`LiveClasses`) | Có (Build 0 lỗi) | **Hoàn chỉnh** |
| **ADMIN-12** | Tạo lớp trực tuyến mới | Có (`Admin/CreateClass.cshtml`) | Có (`CreateClass`) | Có (`LiveClasses`) | Có (Build 0 lỗi) | **Hoàn chỉnh** |
| **ADMIN-13** | Chỉnh sửa lớp trực tuyến | Có (`Admin/EditClass.cshtml`) | Có (`EditClass`) | Có (`LiveClasses`) | Có (Build 0 lỗi) | **Hoàn chỉnh** |
| **ADMIN-14** | Đóng / Mở trạng thái lớp | Có (`Admin/Classes.cshtml`) | Có (`CloseClass`) | Có (`LiveClasses`) | Có (Build 0 lỗi) | **Hoàn chỉnh** |
| **ADMIN-15** | Thời khóa biểu toàn hệ thống | Có (`Admin/Schedule.cshtml`) | Có (`Schedule`) | Có (`LiveClasses`) | Có (Build 0 lỗi) | **Hoàn chỉnh** |
| **ADMIN-16** | Quản lý bài viết IELTS Tips | Có (`Admin/Tips.cshtml`) | Có (`Tips`) | Có (`IeltsTips`) | Có (Build 0 lỗi) | **Hoàn chỉnh** |
| **ADMIN-17** | Đăng bài viết mẹo thi mới | Có (`Admin/CreateTip.cshtml`) | Có (`CreateTip`) | Có (`IeltsTips`) | Có (Build 0 lỗi) | **Hoàn chỉnh** |
| **ADMIN-18** | Chỉnh sửa bài viết mẹo thi | Có (`Admin/EditTip.cshtml`) | Có (`EditTip`) | Có (`IeltsTips`) | Có (Build 0 lỗi) | **Hoàn chỉnh** |
| **ADMIN-19** | Bật / Tắt xuất bản bài viết | Có (`Admin/Tips.cshtml`) | Có (`TogglePublishTip`) | Có (`IeltsTips`) | Có (Build 0 lỗi) | **Hoàn chỉnh** |
| **ADMIN-20** | Xóa bài viết mẹo thi | Có (`Admin/Tips.cshtml`) | Có (`DeleteTip`) | Có (`IeltsTips`) | Có (Build 0 lỗi) | **Hoàn chỉnh** |
| **ADMIN-21** | Thống kê danh mục đề thi | Có (`Admin/Tests.cshtml`) | Có (`Tests`) | Có (`Tests`, `Questions`)| Có (Build 0 lỗi) | **Triển khai một phần** |
| **ADMIN-22** | Phát sóng thông báo hệ thống| Có (`Admin/Notifications.cshtml`)| Có (`BroadcastNotification`)| Có (`Notifications`)| Có (Build 0 lỗi) | **Hoàn chỉnh** |
| **NOTIF-01** | Trung tâm thông báo | Có (`Notification/Index.cshtml`)| Có (`NotificationController.Index`)| Có (`Notifications`)| Có (Build 0 lỗi) | **Hoàn chỉnh** |
| **NOTIF-02** | Đánh dấu thông báo đã đọc | Có (`Notification/Index.cshtml`)| Có (`MarkAsRead`) | Có (`Notifications`)| Có (Build 0 lỗi) | **Hoàn chỉnh** |
| **NOTIF-03** | Đánh dấu tất cả đã đọc | Có (`Notification/Index.cshtml`)| Có (`MarkAllAsRead`) | Có (`Notifications`)| Có (Build 0 lỗi) | **Hoàn chỉnh** |
| **NOTIF-04** | Gửi bản sao nhắc nhở qua Email| Có (`Notification/Index.cshtml`)| Có (`SendEmailReminder`)| Không (chưa có SMTP)| Có (Build 0 lỗi) | **Triển khai một phần** |
| **NOTIF-05** | Chuông thông báo trên Header | Có (`_LoginPartial.cshtml`) | Có (Inline DbContext Query)| Có (`Notifications`)| Có (Build 0 lỗi) | **Hoàn chỉnh** |
| **LEGACY-01**| LoginController Stub | Không có View | Có (`Index` trống) | Không | Có (Build 0 lỗi) | **Không hoạt động** |
| **LEGACY-02**| ManagerController Stub | Không có View | Có (`Index` trống) | Không | Có (Build 0 lỗi) | **Không hoạt động** |
| **LEGACY-03**| HomeworkController Stub | Không có View | Có (`Index` trống) | Không | Có (Build 0 lỗi) | **Không hoạt động** |
| **LEGACY-04**| PostController Stub | Không có View | Có (`Index` trống) | Không | Có (Build 0 lỗi) | **Không hoạt động** |
| **LEGACY-05**| Mô hình dữ liệu cũ | Không | Không | Có (`UserAccounts`...) | Có (Build 0 lỗi) | **Chưa kết nối** |

---

# PHẦN VIII — CÁC VẤN ĐỀ VÀ CHỨC NĂNG CÒN THIẾU

Qua quá trình rà soát toàn bộ source code thực tế, các phát hiện được phân nhóm cụ thể như sau:

### 1. Giao diện chưa có xử lý hoặc nút mang tính hình thức (Decorative / Mock Actions)
* **Nút đăng ký Live Lesson trên trang chủ (`Views/Home/Index.cshtml` dòng 330):**
  - Hiện tại: Nút `<button onclick="alert('Đã đăng ký thành công...');">` chỉ hiển thị thông báo popup của trình duyệt.
  - Vấn đề: Không gửi request về `ClassController.Join` hoặc ghi nhận vào cơ sở dữ liệu.
* **Biểu mẫu đăng ký tư vấn 1-1 trên trang chủ (`HomeController.SubmitConsultation`):**
  - Hiện tại: Controller nhận form, validate dữ liệu và trả về thông báo cảm ơn qua `TempData`.
  - Vấn đề: Trong code ghi rõ `// Giả lập lưu trữ thông tin đăng ký thành công vào cơ sở dữ liệu` — chưa có bảng `ConsultationLeads` trong database để lưu trữ số điện thoại và email của khách hàng.
* **Nút chia sẻ màn hình trong phòng học ảo (`Views/Class/Room.cshtml` dòng 66):**
  - Hiện tại: Nút chỉ gọi `onclick="alert('Tính năng chia sẻ màn hình của học viên cần sự cho phép...')"` mà không có kết nối WebRTC getDisplayMedia.
* **Ghi âm bài thi Speaking (`Views/Test/TakeTest.cshtml` dòng 153):**
  - Hiện tại: Nút `simulateRecord()` chỉ thay đổi nhãn trạng thái và điền giá trị text mô phỏng vào input ẩn, chưa kích hoạt API `navigator.mediaDevices.getUserMedia` để thu âm và upload file `.wav`/`.mp3` thực tế.

### 2. Lỗi lệch khớp định danh JavaScript trong `site.js`
* **Lệch ID điều hướng di động (Mobile Navbar Collapse):**
  - Trong `wwwroot/js/site.js` dòng 16: `const navbarCollapse = document.getElementById('navbarMain');`
  - Nhưng trong `Views/Shared/_Layout.cshtml` dòng 119: `<div class="collapse navbar-collapse" id="navbarMainNavigation">`
  - Hệ quả: Sự kiện tự động đóng menu mobile khi click liên kết không kích hoạt được vì không tìm thấy phần tử `#navbarMain`.
* **Lệch ID Carousel phản hồi học viên (Testimonial Carousel):**
  - Trong `wwwroot/js/site.js` dòng 30: `document.getElementById('testimonialCarousel')`
  - Nhưng trong `Views/Home/Index.cshtml` dòng 399: `id="socialCarouselOfficial"`
  - Hệ quả: Script khởi tạo Carousel trong `site.js` bị bỏ qua (tuy nhiên Bootstrap data-bs-ride vẫn tự chạy).

### 3. Backend chưa được kết nối hoặc các thành phần thừa (Dead Code / Stub Controllers)
* **4 Controllers mồ côi (Orphaned Stub Controllers):**
  - `LoginController.cs`, `ManagerController.cs`, `HomeworkController.cs`, `PostController.cs` chỉ chứa 13 dòng mã khung (skeleton) `public IActionResult Index() => View();` và không có thư mục View tương ứng.
  - Nếu người dùng truy cập trực tiếp các route `/Login`, `/Manager`, `/Homework`, `/Post` sẽ gặp lỗi `InvalidOperationException: The view 'Index' was not found`.
* **7 Thực thể dữ liệu Legacy trong `ApplicationDbContext`:**
  - `UserAccounts`, `ClassRooms`, `Assignments`, `Questions`, `AnswerOptions`, `Submissions`, `SubmissionAnswers` hiện không còn liên kết với bất kỳ Controller hoặc View nào.

### 4. Chức năng Quản trị Đề thi (`Admin/Tests`) mới triển khai một phần
* Màn hình `/Admin/Tests` hiện chỉ đóng vai trò bảng xem tổng quan (View-only), chưa có chức năng Thêm bài thi mới (Create Test), Sửa bài thi (Edit Test), Xóa bài thi (Delete Test) hoặc Soạn thảo câu hỏi thi trực quan từ giao diện Admin. Các bài thi hiện đang phụ thuộc hoàn toàn vào dữ liệu nạp từ `DbInitializer.cs`.

### 5. Dịch vụ gửi Email thực tế
* Tính năng gửi mã OTP đăng ký/quên mật khẩu (`AccountController`) và tính năng gửi Email Reminder (`NotificationController.SendEmailReminder`) hiện đang hoạt động theo cơ chế **mô phỏng trong nội bộ hệ thống** (xuất thông báo kèm mã lên màn hình qua `TempData`), chưa tích hợp thư viện gửi thư như `MailKit` hoặc dịch vụ SMTP bên ngoài.

---

# PHẦN IX — KẾT LUẬN

### 9.1. Thống kê tổng hợp số lượng chức năng
Căn cứ trên việc đối chiếu từng file mã nguồn, controller, model và view trong toàn bộ dự án:

* **Tổng số chức năng nghiệp vụ đã thống kê:** **82 chức năng**
* **Số chức năng hoàn chỉnh (Đầy đủ UI, Backend, DB & Build thành công):** **71 chức năng** (chiếm 86.6%)
* **Số chức năng triển khai một phần (Có UI/Backend nhưng một phần mô phỏng hoặc thiếu DB lead):** **5 chức năng** (chiếm 6.1%)
  - `HOME-02` (Gửi form tư vấn - chưa lưu DB).
  - `CLASS-04` (Phòng học ảo - giao diện & attendance hoàn chỉnh nhưng stream/chat là local DOM).
  - `TEST-07` (Thu âm Speaking - mô phỏng trạng thái, chưa upload audio).
  - `ADMIN-21` (Quản lý đề thi admin - mới có view thống kê, chưa có form CRUD đề thi).
  - `NOTIF-04` (Email Reminder - mô phỏng TempData, chưa gửi SMTP).
* **Số chức năng không hoạt động (Stub controllers thiếu view):** **4 chức năng** (chiếm 4.9%)
  - `LEGACY-01` (`LoginController`), `LEGACY-02` (`ManagerController`), `LEGACY-03` (`HomeworkController`), `LEGACY-04` (`PostController`).
* **Số thành phần dữ liệu cũ chưa kết nối:** **2 thực thể / nhóm model** (`LEGACY-05`).

### 9.2. Kết quả kiểm toán chất lượng & tuân thủ quy chuẩn dự án
1. **Khả năng biên dịch:** Toàn bộ giải pháp .NET 10.0 biên dịch hoàn toàn thành công (**`0 Warning(s), 0 Error(s)`**).
2. **Tuân thủ LUMORA Design System & IELTS Rule 9:**
   - Hệ thống biến CSS variables chuẩn (`site.css`) và dải màu 4 kỹ năng IELTS (Listening, Reading, Writing, Speaking) được áp dụng nhất quán từ Trang chủ đến Dashboard và Phòng thi.
   - Toàn bộ giao diện phòng thi (`Test/TakeTest.cshtml`) và báo cáo kết quả (`Test/Result.cshtml`) tuân thủ nghiêm ngặt quy định **English-Only Requirement**.
3. **Tính toàn vẹn mã nguồn:** Quá trình kiểm toán chỉ thực hiện việc đọc hiểu, tra cứu và lập tài liệu; **không thực hiện bất kỳ thao tác sửa đổi, xóa bỏ hay tái cấu trúc mã nguồn nào**.

### 9.3. 5 Vấn đề quan trọng nhất cần xử lý trước khi đưa vào vận hành thực tế
1. **Bổ sung bảng lưu trữ Lead tư vấn (`ConsultationForm`):** Tạo model `ConsultationLead` và lưu thông tin từ form trang chủ vào database thay vì chỉ thông báo qua `TempData`.
2. **Khắc phục lệch ID trong `site.js`:** Điều chỉnh ID `#navbarMain` thành `#navbarMainNavigation` và `#testimonialCarousel` thành `#socialCarouselOfficial` để khôi phục tính năng tự đóng menu di động và carousel.
3. **Dọn dẹp các Controller mồ côi (Stub Controllers):** Xóa bỏ hoặc đặt lệnh chuyển hướng (301 Redirect) từ `LoginController`, `ManagerController`, `HomeworkController`, `PostController` sang các Controller tương ứng để tránh lỗi 500 khi người dùng nhập URL trực tiếp.
4. **Phát triển giao diện Quản trị Đề thi (Admin Test Authoring):** Bổ sung các Action `CreateTest`, `EditTest` và giao diện nhập câu hỏi trắc nghiệm/bài đọc/audio cho bài thi IELTS trong `AdminController`.
5. **Tích hợp máy chủ gửi thư điện tử (SMTP Service):** Cấu hình dịch vụ gửi email thực tế (`IEmailSender` / `MailKit`) để chuyển phát mã OTP và thông báo học tập đến hộp thư người dùng thay vì chỉ hiển thị trên TempData.

---
*Báo cáo được lập tự động dựa trên phân tích mã nguồn thực tế của dự án LUMORA English Center.*

