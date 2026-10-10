using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using LTW.Models;

namespace LTW.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Course> Courses { get; set; } = default!;
        public DbSet<Chapter> Chapters { get; set; } = default!;
        public DbSet<Lesson> Lessons { get; set; } = default!;
        public DbSet<LessonProgress> LessonProgresses { get; set; } = default!;
        public DbSet<Exercise> Exercises { get; set; } = default!;
        public DbSet<ExerciseQuestion> ExerciseQuestions { get; set; } = default!;
        public DbSet<ExerciseSubmission> ExerciseSubmissions { get; set; } = default!;
        public DbSet<LiveClass> LiveClasses { get; set; } = default!;
        public DbSet<LiveClassStudent> LiveClassStudents { get; set; } = default!;
        public DbSet<Enrollment> Enrollments { get; set; } = default!;
        public DbSet<Test> Tests { get; set; } = default!;
        public DbSet<TestQuestion> TestQuestions { get; set; } = default!;
        public DbSet<TestAttempt> TestAttempts { get; set; } = default!;
        public DbSet<IeltsTip> IeltsTips { get; set; } = default!;
        public DbSet<Notification> Notifications { get; set; } = default!;
        public DbSet<OtpRecord> OtpRecords { get; set; } = default!;

        // Legacy tables support if needed
        public DbSet<UserAccount> UserAccounts { get; set; } = default!;
        public DbSet<ClassRoom> ClassRooms { get; set; } = default!;
        public DbSet<Assignment> Assignments { get; set; } = default!;
        public DbSet<Question> Questions { get; set; } = default!;
        public DbSet<AnswerOption> AnswerOptions { get; set; } = default!;
        public DbSet<Submission> Submissions { get; set; } = default!;
        public DbSet<SubmissionAnswer> SubmissionAnswers { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure DeleteBehavior to Restrict for SQL Server multiple cascade paths
            builder.Entity<LessonProgress>()
                .HasOne(lp => lp.Student)
                .WithMany()
                .HasForeignKey(lp => lp.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ExerciseSubmission>()
                .HasOne(es => es.Student)
                .WithMany()
                .HasForeignKey(es => es.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Enrollment>()
                .HasOne(e => e.Student)
                .WithMany()
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Enrollment>()
                .HasOne(e => e.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<LiveClassStudent>()
                .HasOne(lcs => lcs.Student)
                .WithMany()
                .HasForeignKey(lcs => lcs.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<TestAttempt>()
                .HasOne(ta => ta.Student)
                .WithMany()
                .HasForeignKey(ta => ta.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<LiveClass>()
                .HasOne(lc => lc.Teacher)
                .WithMany()
                .HasForeignKey(lc => lc.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<LiveClass>()
                .HasOne(lc => lc.Course)
                .WithMany(c => c.LiveClasses)
                .HasForeignKey(lc => lc.CourseId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Course>()
                .HasOne(c => c.Teacher)
                .WithMany()
                .HasForeignKey(c => c.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<SubmissionAnswer>()
                .HasOne(sa => sa.Submission)
                .WithMany(s => s.Answers)
                .HasForeignKey(sa => sa.SubmissionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SubmissionAnswer>()
                .HasOne(sa => sa.Question)
                .WithMany()
                .HasForeignKey(sa => sa.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Submission>()
                .HasOne(s => s.Student)
                .WithMany()
                .HasForeignKey(s => s.StudentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
