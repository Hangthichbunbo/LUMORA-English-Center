# LUMORA English Center

**LUMORA English Center** là dự án website hỗ trợ quản lý đào tạo tiếng Anh, cung cấp thông tin khóa học và hỗ trợ học viên học tập, luyện thi IELTS trực tuyến. Dự án hướng đến xây dựng một nền tảng học tập có giao diện hiện đại, dễ sử dụng và thuận tiện cho cả học viên lẫn người quản lý.

## 📌 1. Giới thiệu dự án

- **Tên dự án:** LUMORA English Center
- **Loại dự án:** Website quản lý trung tâm tiếng Anh và hỗ trợ học trực tuyến
- **Lĩnh vực:** Giáo dục, đào tạo tiếng Anh, luyện thi IELTS
- **Đối tượng sử dụng:** Học viên, giáo viên và quản trị viên
- **Mục tiêu:** Số hóa hoạt động giới thiệu khóa học, hỗ trợ đăng ký và tổ chức các hoạt động học tập trên nền tảng trực tuyến.

## ✨ 2. Các chức năng chính

### 2.1. Quản lý khóa học
- Hiển thị danh sách khóa học tiếng Anh.
- Cung cấp thông tin chi tiết về nội dung và lộ trình học.
- Hỗ trợ tìm kiếm và lựa chọn khóa học phù hợp.

### 2.2. Học tập và luyện thi IELTS
- Tổ chức nội dung luyện thi theo bốn kỹ năng: Listening, Reading, Writing và Speaking.
- Hỗ trợ làm bài tập và bài kiểm tra.
- Hướng đến việc lưu kết quả và theo dõi tiến độ học tập.

### 2.3. Quản lý giáo viên
- Hiển thị danh sách giáo viên.
- Cung cấp thông tin giới thiệu và hồ sơ giáo viên.
- Hỗ trợ học viên tìm hiểu đội ngũ giảng dạy.

### 2.4. Quản lý tài khoản
- Đăng ký và đăng nhập.
- Phân biệt quyền truy cập theo vai trò người dùng.
- Hỗ trợ truy cập các chức năng phù hợp với từng đối tượng.

### 2.5. Khu vực học viên
- Quản lý thông tin cá nhân.
- Theo dõi các khóa học đã đăng ký.
- Hỗ trợ truy cập nội dung học tập và kết quả học tập.

## 🛠️ 3. Công nghệ sử dụng

- **Ngôn ngữ lập trình:** C#
- **Nền tảng phát triển:** ASP.NET Core
- **Giao diện:** HTML, CSS, JavaScript
- **Môi trường phát triển:** Visual Studio
- **Quản lý mã nguồn:** Git và GitHub

Công nghệ cơ sở dữ liệu và các thư viện bổ sung sẽ được cập nhật theo cấu hình thực tế của dự án.

## ⚙️ 4. Cài đặt và chạy dự án

### Yêu cầu hệ thống

- Windows
- Visual Studio có cài đặt workload ASP.NET and web development
- .NET SDK tương thích với phiên bản được khai báo trong file dự án
- Git (nếu clone dự án từ GitHub)

### Bước 1: Clone repository

```bash
git clone https://github.com/Hangthichbunbo/LUMORA-English-Center.git
```

Di chuyển vào thư mục dự án:

```bash
cd LUMORA-English-Center
```

### Bước 2: Mở dự án

Mở file solution `.sln` bằng Visual Studio. Nếu repository không có file solution ở thư mục gốc, hãy mở file dự án `.csproj` tương ứng.

### Bước 3: Khôi phục các gói phụ thuộc

Mở Terminal tại thư mục chứa file dự án và chạy:

```bash
dotnet restore
```

### Bước 4: Biên dịch dự án

```bash
dotnet build
```

### Bước 5: Chạy website

Có thể nhấn **F5** hoặc **Ctrl + F5** trong Visual Studio.

Nếu sử dụng các profile được cấu hình trong `launchSettings.json`, website hiện có các địa chỉ phát triển:

- HTTP: `http://localhost:5223`
- HTTPS: `https://localhost:7025`

Địa chỉ thực tế có thể thay đổi theo cấu hình chạy của dự án.

## 📁 5. Cấu trúc dự án

Cấu trúc thư mục được tổ chức theo mã nguồn thực tế của repository. Với dự án ASP.NET Core, các thành phần thường gặp gồm:

```text
LUMORA-English-Center/
├── Controllers/       # Xử lý yêu cầu và điều hướng
├── Models/            # Các lớp dữ liệu
├── Views/             # Giao diện Razor (nếu sử dụng MVC)
├── wwwroot/           # CSS, JavaScript, hình ảnh và tài nguyên tĩnh
├── Properties/        # Cấu hình chạy ứng dụng
├── appsettings.json   # Cấu hình ứng dụng
├── Program.cs         # Điểm khởi chạy ứng dụng
└── README.md          # Tài liệu giới thiệu dự án
```

## 🔐 6. Lưu ý về cấu hình và bảo mật

- Không đưa mật khẩu, chuỗi kết nối cơ sở dữ liệu chứa thông tin nhạy cảm hoặc khóa API lên GitHub.
- Không commit các tệp cấu hình cá nhân và tệp chứa bí mật.
- Có thể sử dụng tệp `.env.example` hoặc cấu hình mẫu để hướng dẫn thiết lập môi trường.
- Kiểm tra các cấu hình cần thiết trước khi chạy dự án trên máy mới.

## 🚀 7. Định hướng phát triển

- Hoàn thiện các chức năng quản lý khóa học và đăng ký học.
- Phát triển hệ thống bài thi IELTS trực tuyến.
- Bổ sung chức năng lưu kết quả và theo dõi tiến độ học tập.
- Hoàn thiện phân quyền và quản lý tài khoản.
- Tối ưu giao diện trên máy tính, máy tính bảng và điện thoại.
- Nâng cao khả năng bảo mật và trải nghiệm người dùng.


**Dự án phục vụ mục đích học tập và phát triển phần mềm. Giấy phép sử dụng và phân phối sẽ được xác định theo quy định của chủ sở hữu repository.**

---

**LUMORA English Center** — Hỗ trợ học tập, kết nối tri thức và phát triển năng lực tiếng Anh.
