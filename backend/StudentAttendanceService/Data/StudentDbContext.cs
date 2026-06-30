using Microsoft.EntityFrameworkCore;
using StudentAttendanceService.Entities;
using StudentAttendanceService.Enums;

namespace StudentAttendanceService.Data;

public sealed class StudentDbContext(DbContextOptions<StudentDbContext> options) : DbContext(options)
{
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<AttendanceSession> AttendanceSessions => Set<AttendanceSession>();
    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();
    public DbSet<StudentResult> StudentResults => Set<StudentResult>();
    public DbSet<CourseReview> CourseReviews => Set<CourseReview>();
    public DbSet<TeacherReview> TeacherReviews => Set<TeacherReview>();
    public DbSet<AiKnowledgeDocument> AiKnowledgeDocuments => Set<AiKnowledgeDocument>();
    public DbSet<AiKnowledgeChunk> AiKnowledgeChunks => Set<AiKnowledgeChunk>();
    public DbSet<AiEmailDraft> AiEmailDrafts => Set<AiEmailDraft>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Student>().HasIndex(x => x.StudentCode).IsUnique();
        modelBuilder.Entity<Student>().HasIndex(x => x.Email).IsUnique();
        modelBuilder.Entity<Enrollment>().HasIndex(x => new { x.StudentId, x.ClassId }).IsUnique();
        modelBuilder.Entity<AttendanceSession>().HasIndex(x => new { x.ClassId, x.AttendanceDate, x.SessionNumber }).IsUnique();
        modelBuilder.Entity<AttendanceRecord>().HasIndex(x => new { x.AttendanceSessionId, x.StudentId }).IsUnique();
        modelBuilder.Entity<Enrollment>().HasOne(x => x.Student).WithMany(x => x.Enrollments).HasForeignKey(x => x.StudentId);
        modelBuilder.Entity<AttendanceRecord>().HasOne(x => x.AttendanceSession).WithMany(x => x.Records).HasForeignKey(x => x.AttendanceSessionId);
        modelBuilder.Entity<StudentResult>().Property(x => x.MidtermScore).HasPrecision(5, 2);
        modelBuilder.Entity<StudentResult>().Property(x => x.FinalScore).HasPrecision(5, 2);
        modelBuilder.Entity<StudentResult>().Property(x => x.AverageScore).HasPrecision(5, 2);
        modelBuilder.Entity<StudentResult>().Property(x => x.AttendancePercent).HasPrecision(5, 2);
        modelBuilder.Entity<CourseReview>().HasIndex(x => new { x.StudentId, x.ClassId }).IsUnique();
        modelBuilder.Entity<CourseReview>().HasIndex(x => x.EnrollmentId).IsUnique();
        modelBuilder.Entity<CourseReview>().HasIndex(x => x.CourseId);
        modelBuilder.Entity<CourseReview>().Property(x => x.CourseRating).HasPrecision(3, 1);
        modelBuilder.Entity<CourseReview>().HasOne(x => x.Enrollment).WithMany().HasForeignKey(x => x.EnrollmentId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<TeacherReview>().HasIndex(x => x.TeacherId);
        modelBuilder.Entity<TeacherReview>().Property(x => x.Rating).HasPrecision(3, 1);
        modelBuilder.Entity<TeacherReview>().HasOne(x => x.CourseReview).WithMany(x => x.TeacherReviews).HasForeignKey(x => x.CourseReviewId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<AiKnowledgeDocument>().HasIndex(x => new { x.Scope, x.AudienceRole, x.OwnerReferenceId, x.ClassId });
        modelBuilder.Entity<AiKnowledgeChunk>().HasIndex(x => new { x.DocumentId, x.ChunkIndex }).IsUnique();
        modelBuilder.Entity<AiKnowledgeChunk>().HasOne(x => x.Document).WithMany(x => x.Chunks).HasForeignKey(x => x.DocumentId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<AiEmailDraft>().HasIndex(x => new { x.Status, x.CreatedAt });

        var now = new DateTime(2026, 1, 1, 8, 0, 0, DateTimeKind.Utc);
        var studentIds = Enumerable.Range(1, 20).Select(i => Guid.Parse($"aaaaaaaa-aaaa-aaaa-aaaa-{i:000000000000}")).ToArray();
        var classIds = Enumerable.Range(1, 6).Select(i => Guid.Parse($"33333333-3333-3333-3333-{i:000000000000}")).ToArray();
        var courseIds = Enumerable.Range(1, 8).Select(i => Guid.Parse($"22222222-2222-2222-2222-{i:000000000000}")).ToArray();
        var sessionIds = Enumerable.Range(1, 5).Select(i => Guid.Parse($"bbbbbbbb-bbbb-bbbb-bbbb-{i:000000000000}")).ToArray();

        modelBuilder.Entity<Student>().HasData(Enumerable.Range(0, 20).Select(i => new Student
        {
            Id = studentIds[i],
            StudentCode = $"STU{i + 1:000}",
            FullName = $"Student {i + 1:00}",
            Email = $"student{i + 1:00}@educenter.vn",
            Phone = $"091{i + 1:0000000}",
            DateOfBirth = new DateTime(2002 + i % 5, 1 + i % 12, 1 + i % 25),
            Gender = i % 2 == 0 ? Gender.Male : Gender.Female,
            Address = i % 2 == 0 ? "Ha Noi" : "Ho Chi Minh",
            Status = StudentStatus.Active,
            CreatedAt = now,
            UpdatedAt = now
        }));

        modelBuilder.Entity<Enrollment>().HasData(Enumerable.Range(0, 15).Select(i => new Enrollment
        {
            Id = Guid.Parse($"cccccccc-cccc-cccc-cccc-{i + 1:000000000000}"),
            StudentId = studentIds[i],
            StudentNameSnapshot = $"Student {i + 1:00}",
            CourseId = courseIds[i % 6],
            CourseNameSnapshot = $"Course {i % 6 + 1}",
            ClassId = classIds[i % 5],
            ClassNameSnapshot = $"Class {i % 5 + 1}",
            EnrolledAt = now.AddDays(i),
            Status = i % 3 == 0 ? EnrollmentStatus.Confirmed : EnrollmentStatus.Studying,
            Note = "Seed enrollment",
            CreatedAt = now,
            UpdatedAt = now
        }));

        modelBuilder.Entity<AttendanceSession>().HasData(Enumerable.Range(0, 5).Select(i => new AttendanceSession
        {
            Id = sessionIds[i],
            ClassId = classIds[i],
            ClassNameSnapshot = $"Class {i + 1}",
            ScheduleId = Guid.Parse($"44444444-4444-4444-4444-{i + 1:000000000000}"),
            SessionNumber = i + 1,
            AttendanceDate = now.AddDays(i),
            Topic = $"Session {i + 1}",
            CreatedByTeacherId = Guid.Parse($"11111111-1111-1111-1111-{i % 4 + 1:000000000000}"),
            CreatedByTeacherName = $"Teacher {i % 4 + 1}",
            Status = AttendanceSessionStatus.Open,
            CreatedAt = now,
            UpdatedAt = now
        }));

        modelBuilder.Entity<AttendanceRecord>().HasData(Enumerable.Range(0, 50).Select(i => new AttendanceRecord
        {
            Id = Guid.Parse($"dddddddd-dddd-dddd-dddd-{i + 1:000000000000}"),
            AttendanceSessionId = sessionIds[i % 5],
            StudentId = studentIds[(i / 5) % 15],
            StudentNameSnapshot = $"Student {(i / 5) % 15 + 1:00}",
            Status = i % 5 == 0 ? AttendanceStatus.Absent : i % 4 == 0 ? AttendanceStatus.Late : AttendanceStatus.Present,
            Note = null,
            MarkedAt = now.AddDays(i % 5)
        }));

        modelBuilder.Entity<StudentResult>().HasData(Enumerable.Range(0, 10).Select(i =>
        {
            var mid = 5 + i % 5;
            var final = 6 + i % 4;
            var avg = mid * 0.4m + final * 0.6m;
            return new StudentResult
            {
                Id = Guid.Parse($"eeeeeeee-eeee-eeee-eeee-{i + 1:000000000000}"),
                StudentId = studentIds[i],
                StudentNameSnapshot = $"Student {i + 1:00}",
                CourseId = courseIds[i % 6],
                CourseNameSnapshot = $"Course {i % 6 + 1}",
                ClassId = classIds[i % 5],
                ClassNameSnapshot = $"Class {i % 5 + 1}",
                MidtermScore = mid,
                FinalScore = final,
                AverageScore = avg,
                AttendancePercent = 80,
                ResultStatus = avg >= 5 ? ResultStatus.Passed : ResultStatus.Failed,
                Feedback = "Seed result",
                EvaluatedByTeacherId = Guid.Parse($"11111111-1111-1111-1111-{i % 4 + 1:000000000000}"),
                EvaluatedByTeacherName = $"Teacher {i % 4 + 1}",
                EvaluatedAt = now,
                CreatedAt = now,
                UpdatedAt = now
            };
        }));
    }
}
