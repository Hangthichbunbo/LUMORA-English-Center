# LUMORA English Center

LUMORA English Center là đồ án website hỗ trợ giới thiệu trung tâm tiếng Anh, tra cứu khóa học, học trực tuyến và quản lý hoạt động giảng dạy. Hệ thống phục vụ khách truy cập, học viên (Student), giảng viên (Teacher) và quản trị viên (Admin).

Dự án sử dụng **ASP.NET Core MVC trên .NET 10**, giao diện Razor, **Entity Framework Core với SQL Server** và **ASP.NET Core Identity** để quản lý tài khoản, xác thực bằng cookie và phân quyền.

## Chức năng

### Khách truy cập

- Xem trang chủ, lộ trình học, nội dung giới thiệu và câu hỏi thường gặp.
- Tìm kiếm khóa học theo từ khóa, lọc danh mục và xem chi tiết chương trình, học phí, giáo viên, bài học và lịch live.
- Xem danh sách, hồ sơ giảng viên và các bài viết mẹo học IELTS.
- Truy cập bài thi mẫu và bài kiểm tra xếp lớp.
- Đăng ký tài khoản, đăng nhập và sử dụng các biểu mẫu tư vấn/giữ chỗ ở mức triển khai hiện tại.

### Học viên — Student

- Xem Dashboard, các khóa đã ghi danh và lịch học.
- Ghi danh qua quy trình thanh toán mô phỏng.
- Truy cập bài học, tài liệu qua URL, theo dõi và đánh dấu hoàn thành bài học.
- Làm bài tập, nộp hoặc làm lại bài, xem điểm tự động và nhận xét của giảng viên.
- Tham gia lớp trực tuyến theo điều kiện truy cập.
- Cập nhật hồ sơ, đổi mật khẩu, đọc và đánh dấu thông báo.

### Giảng viên — Teacher

- Xem Dashboard, lớp được phân công và lịch dạy.
- Quản lý bài học trong khóa được phụ trách: tạo, chỉnh sửa, chuyển chương và xóa.
- Tạo nhanh chương, đổi tên/thứ tự chương và xóa chương trống.
- Nhập nội dung, từ vựng, ngữ pháp và URL video, audio, tài liệu cho bài học.
- Tạo, sửa, xóa bài tập và câu hỏi trắc nghiệm hoặc Writing.
- Xem học viên đã ghi danh, bài nộp và thống kê kết quả.
- Chấm điểm, ghi nhận xét và tạo thông báo kết quả cho học viên.
- Quản lý hồ sơ cá nhân và thông báo.

### Quản trị viên — Admin

- Xem Dashboard và danh sách tài khoản; tạo tài khoản giảng viên, thay đổi trạng thái hoạt động và xóa tài khoản.
- Tạo, sửa, xuất bản hoặc xóa khóa học và phân công giảng viên qua thông tin khóa/lớp.
- Quản lý lớp trực tuyến, lịch học, trạng thái lớp và thành viên lớp.
- Quản lý bài viết mẹo học; xem danh sách bài thi.
- Xem và quản lý nội dung bài học, bài tập với quyền Admin.
- Xem thông báo và gửi thông báo trong hệ thống theo nhóm vai trò.

## Mức độ triển khai

Đây là sản phẩm phục vụ học tập và minh họa nghiệp vụ. Những giới hạn dưới đây cần được phân biệt với các chức năng đã có xử lý dữ liệu:

| Thành phần | Hiện trạng trong source |
|---|---|
| Trang chủ | Nội dung quảng bá, khóa học và đội ngũ giảng viên được tạo sẵn trong `LandingPageViewModel`; trang Courses và hồ sơ giảng viên đích đọc database. |
| Tìm kiếm khóa học | Có tìm kiếm và lọc danh mục; backend nhận thêm `level` nhưng trang danh sách chưa có bộ chọn trình độ. Sắp xếp cố định, chưa phân trang. |
| Tư vấn | Có kiểm tra dữ liệu và phản hồi từ controller; chưa lưu yêu cầu hoặc gửi email. |
| Giữ chỗ Live Lesson tại Home | JavaScript hiển thị xác nhận trên trình duyệt; chưa lưu đăng ký. |
| Thanh toán | `ProcessPayment` tự ghi enrollment có trạng thái `Completed`; chưa tích hợp xác nhận từ cổng thanh toán. |
| Phòng học trực tuyến | Có kiểm quyền vào lớp và dữ liệu thành viên; mic, camera, chat, giơ tay và chia sẻ màn hình đang mô phỏng giao diện. |
| Thi thử/xếp lớp | Có lưu kết quả; Writing được ước lượng theo độ dài văn bản, Speaking dùng điểm mô phỏng. |
| Tài liệu bài học | Lưu URL video/audio/tài liệu; chưa có quy trình tải tệp lên trong form bài học. |
| Email và Google | Nhắc email/OTP hiện có phần mô phỏng. Google OAuth chỉ được đăng ký khi có cấu hình; đăng nhập nhanh tài khoản demo là luồng riêng. |
| Phân quyền và thống kê | Cần hoàn thiện kiểm quyền sở hữu ở thao tác lưu điểm, kiểm chủ ghi danh ở trang xác nhận, xác minh OTP tại bước reset mật khẩu và phạm vi tính điểm theo khóa. |

Các nhận định trên dựa trên source, không thay thế kết quả kiểm thử tích hợp hoặc xác nhận hệ thống đã sẵn sàng triển khai thực tế.

## Công nghệ và tổ chức mã nguồn

| Thành phần | Công nghệ |
|---|---|
| Backend | C#, ASP.NET Core MVC, .NET 10 (`net10.0`) |
| Giao diện | Razor Views (`.cshtml`), HTML, CSS, JavaScript, Bootstrap, Bootstrap Icons, jQuery |
| Truy cập dữ liệu | Entity Framework Core, SQL Server provider |
| Tài khoản | ASP.NET Core Identity, role, cookie; Google OAuth tùy cấu hình |
| Lưu trữ phiên | Session với distributed memory cache |
| Gói Microsoft chính | Phiên bản `10.0.12` theo `LTW.csproj` |

Controller trực tiếp sử dụng `ApplicationDbContext` và các dịch vụ Identity qua dependency injection. Các luồng hiện tại chưa tách thành tầng Service/Repository nghiệp vụ riêng. `Areas/Identity` chứa các Razor Pages tài khoản bên cạnh giao diện Account MVC.

```text
LUMORA-English-Center/
├── README.md
├── LTW/
│   ├── LTW.csproj                 # Project web, target net10.0
│   ├── LTW.slnx                  # Solution
│   ├── Program.cs                # DI, SQL Server, Identity, middleware, route và seed
│   ├── Controllers/              # Home, Course, Teacher, Student, Admin, Account...
│   ├── Data/
│   │   ├── ApplicationDbContext.cs
│   │   ├── ApplicationUser.cs
│   │   ├── DbInitializer.cs       # Khởi tạo database và dữ liệu mẫu
│   │   └── Migrations/
│   ├── Models/                   # Entity, ViewModel, DTO và enum
│   ├── Views/                    # Razor Views và layout/partial dùng chung
│   ├── Areas/Identity/            # Razor Pages Identity
│   ├── wwwroot/                  # CSS, JavaScript, thư viện giao diện
│   ├── Properties/launchSettings.json
│   ├── appsettings.json
│   └── appsettings.Development.json
└── docs/
    ├── feature-analysis/         # Phân tích Home, Courses, Teacher và hướng dẫn bảo vệ
    └── system-analysis/          # Tài liệu phân tích toàn hệ thống
```

## Chạy trên máy phát triển

### 1. Chuẩn bị

- .NET SDK 10 tương thích với `net10.0`.
- SQL Server hoặc SQL Server Express/LocalDB trên Windows; tài khoản kết nối có quyền tạo database nếu sử dụng khởi tạo tự động.
- Git nếu tải dự án bằng lệnh clone.
- Có thể dùng Visual Studio hỗ trợ .NET 10 và workload **ASP.NET and web development**, hoặc dùng terminal.

### 2. Tải mã nguồn

```powershell
git clone https://github.com/Hangthichbunbo/LUMORA-English-Center.git
cd LUMORA-English-Center
```

Các lệnh tiếp theo chạy tại thư mục gốc repository, nơi chứa README này.

### 3. Cấu hình SQL Server

Ứng dụng đọc khóa `ConnectionStrings:DefaultConnection`. Thiết lập giá trị phù hợp với máy bằng biến môi trường trong terminal PowerShell hiện tại. Ví dụ cho Windows LocalDB:

```powershell
$env:ConnectionStrings__DefaultConnection = 'Server=(localdb)\MSSQLLocalDB;Database=LumoraEnglishCenterDev;Trusted_Connection=True;TrustServerCertificate=True;'
```

Đây là ví dụ cấu hình cho database phát triển riêng, không phải thông tin kết nối của hệ thống đang vận hành. Nếu dùng SQL Server instance khác, thay tên server/database và cơ chế xác thực tương ứng. Biến môi trường ghi đè giá trị cùng khóa trong `appsettings.json`.

**Khi khởi động**, `Program.cs` gọi `DbInitializer.InitializeAsync`. Initializer dùng `EnsureCreatedAsync` và tạo/bổ sung role, tài khoản, khóa học cùng dữ liệu minh họa theo điều kiện trong code. Hãy trỏ đến database dành riêng cho phát triển. Đây không phải cơ chế tự động nâng cấp schema của database đã tồn tại; không chạy `dotnet ef database update` như một bước mặc định của hướng dẫn này.

Tài khoản mẫu được khai báo trong `LTW/Data/DbInitializer.cs`. README không công bố mật khẩu; kiểm tra mã khởi tạo trong môi trường phát triển để xác định tài khoản được tạo. Luồng đăng ký công khai tạo Student; tài khoản Teacher được seed hoặc tạo bởi Admin.

### 4. Khôi phục gói và build

```powershell
dotnet restore .\LTW\LTW.csproj
dotnet build .\LTW\LTW.csproj --no-restore
```

### 5. Chạy ứng dụng

Nếu máy chưa tin cậy chứng chỉ HTTPS phát triển, chạy một lần và làm theo yêu cầu của hệ điều hành:

```powershell
dotnet dev-certs https --trust
```

Chạy với profile HTTPS đã khai báo:

```powershell
dotnet run --project .\LTW\LTW.csproj --launch-profile https
```

Theo `LTW/Properties/launchSettings.json`, profile `https` lắng nghe tại:

- HTTPS: [https://localhost:7025](https://localhost:7025)
- HTTP: [http://localhost:5223](http://localhost:5223)

Ứng dụng bật chuyển hướng HTTPS. Nếu đổi cổng hoặc profile, sử dụng địa chỉ thực tế được in trong terminal. Dừng bằng `Ctrl+C`.

Với Visual Studio, mở `LTW/LTW.slnx` hoặc `LTW/LTW.csproj`, chọn profile `https` rồi chạy bằng F5/Ctrl+F5.

### Google OAuth — tùy chọn

Cấu hình hai khóa `Authentication:Google:ClientId` và `Authentication:Google:ClientSecret` bằng biến môi trường hoặc User Secrets cho project. Callback path trong source là `/signin-google`; redirect URI phải khớp host/cổng đang chạy, ví dụ `https://localhost:7025/signin-google` khi dùng profile trên. Không cần cấu hình Google để sử dụng đăng nhập bằng tài khoản/mật khẩu.

## Các đường dẫn chính

Route MVC mặc định: `{controller=Home}/{action=Index}/{id?}`.

| Đường dẫn | Nội dung |
|---|---|
| `/` | Trang chủ |
| `/Course` | Danh sách khóa học |
| `/Course/Detail/{id}` | Chi tiết khóa học |
| `/Teacher` | Danh sách giảng viên công khai |
| `/Teacher/Detail/{id}` | Hồ sơ giảng viên công khai |
| `/Test/ExampleTest` | Bài thi mẫu |
| `/Test/PlacementTest` | Kiểm tra xếp lớp |
| `/Tip` | Bài viết mẹo học |
| `/Class` | Danh sách lớp trực tuyến |
| `/Account/Login` | Đăng nhập |
| `/Account/Profile` | Hồ sơ tài khoản hiện tại |
| `/Student/Dashboard` | Dashboard học tập, yêu cầu đăng nhập |
| `/Teacher/Dashboard` | Portal Teacher/Admin |
| `/Admin/Dashboard` | Portal Admin |
| `/Notification` | Thông báo của tài khoản hiện tại |

Tên controller không tự giới hạn vai trò: ví dụ StudentController sử dụng `[Authorize]` mà không khóa riêng role Student; các action còn có quy tắc truy cập riêng. Portal Teacher và Admin có kiểm role tương ứng.

## Tài liệu phân tích

- [Home](docs/feature-analysis/01-home-analysis.md)
- [Courses](docs/feature-analysis/02-courses-analysis.md)
- [Chức năng Teacher](docs/feature-analysis/03-teacher-features.md)
- [Xác thực và phân quyền](docs/feature-analysis/04-teacher-authentication-authorization.md)
- [Triển khai kỹ thuật và bản đồ route](docs/feature-analysis/05-technical-implementation.md)
- [Bảng tổng hợp chức năng](docs/feature-analysis/06-feature-summary.md)
- [Hướng dẫn thuyết trình](docs/feature-analysis/07-presentation-guide.md)
- [Phân tích toàn hệ thống](docs/system-analysis/README.md)

## Hướng phát triển

- Hoàn thiện kiểm quyền tài nguyên, quy trình khôi phục mật khẩu và kiểm tra dữ liệu phía máy chủ.
- Lưu và quản lý yêu cầu tư vấn, đăng ký buổi học; tích hợp dịch vụ gửi email.
- Kết nối cổng thanh toán và xác minh giao dịch trước khi cấp quyền học.
- Triển khai tương tác phòng học theo thời gian thực và quản lý tệp tài liệu.
- Chuẩn hóa thang điểm, thống kê theo khóa/lớp và bảo toàn lịch sử câu hỏi, bài nộp.
- Bổ sung phân trang, tối ưu truy vấn và kiểm thử các luồng quan trọng.
