using System;
using System.Collections.Generic;
using LTW.Data;

namespace LTW.Models
{
    /// <summary>
    /// ViewModel chi tiết lớp học dành cho Ban Quản Trị (Admin):
    /// Cung cấp dữ liệu danh sách học viên, giảng viên, biểu đồ phân tích và tỉ lệ chuyên cần
    /// </summary>
    public class AdminClassDetailsViewModel
    {
        public LiveClass LiveClass { get; set; } = null!;
        public ApplicationUser? PrimaryTeacher { get; set; }

        // Danh sách giảng viên (Primary + Giảng viên dự thính / Trợ giảng)
        public List<ClassTeacherInfo> Teachers { get; set; } = new();

        // Danh sách học viên thực tế đã vào lớp / điểm danh
        public List<ClassStudentInfo> AttendingStudents { get; set; } = new();

        // Danh sách tất cả học viên đã mua khóa học liên kết (nếu lớp thuộc Course)
        public List<CourseEnrolledStudentInfo> EnrolledStudents { get; set; } = new();

        // Danh sách cán bộ Quản trị viên tham gia giám sát
        public List<ClassAdminInfo> AdminParticipants { get; set; } = new();

        // Thống kê số liệu KPI
        public int TotalParticipants => AttendingStudents.Count + Teachers.Count + AdminParticipants.Count;
        public int Capacity => LiveClass?.Capacity ?? 0;
        public double OccupancyRate => Capacity > 0 ? Math.Round((double)AttendingStudents.Count / Capacity * 100, 1) : 0;
        public int EnrolledCourseCount => EnrolledStudents.Count;
        public double CourseAttendanceRate => EnrolledCourseCount > 0 
            ? Math.Round((double)AttendingStudents.Count(s => EnrolledStudents.Any(e => e.StudentId == s.StudentId)) / EnrolledCourseCount * 100, 1)
            : 0;

        // Dữ liệu hỗ trợ biểu đồ Chart.js
        public int RemainingSeats => Math.Max(0, Capacity - AttendingStudents.Count);
        public int StudentCount => AttendingStudents.Count;
        public int TeacherCount => Teachers.Count;
        public int AdminCount => AdminParticipants.Count;

        // Biểu đồ phân bổ thời gian điểm danh so với giờ học
        public int EarlyAttendanceCount { get; set; }
        public int OnTimeAttendanceCount { get; set; }
        public int LateAttendanceCount { get; set; }

        // Biểu đồ học viên khóa học: Đã vào vs Chưa vào
        public int EnrolledAndAttendedCount => EnrolledStudents.Count(e => e.HasAttendedClass);
        public int EnrolledAndAbsentCount => EnrolledStudents.Count(e => !e.HasAttendedClass);
    }

    public class ClassTeacherInfo
    {
        public string TeacherId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? AvatarUrl { get; set; }
        public bool IsPrimary { get; set; }
        public DateTime? JoinedAt { get; set; }
        public string RoleBadge { get; set; } = "Giảng viên chính";
    }

    public class ClassStudentInfo
    {
        public int AttendanceId { get; set; }
        public string StudentId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? AvatarUrl { get; set; }
        public DateTime RegisteredAt { get; set; }
        public bool IsEnrolledInCourse { get; set; }
        public string StatusLabel { get; set; } = "Đã điểm danh";
    }

    public class CourseEnrolledStudentInfo
    {
        public string StudentId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? AvatarUrl { get; set; }
        public DateTime EnrolledAt { get; set; }
        public bool HasAttendedClass { get; set; }
    }

    public class ClassAdminInfo
    {
        public string AdminId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public DateTime JoinedAt { get; set; }
    }
}

