using CampusFlow.WebAPI.Contracts;
using CampusFlow.WebAPI.Data;
using CampusFlow.WebAPI.models;
using Microsoft.EntityFrameworkCore;

namespace CampusFlow.WebAPI.Services;

public class EnrollmentService(CampusFlowDbContext db, TimeProvider timeProvider, IEnumerable<IEnrollmentRule> rules) : IEnrollmentService
{
    public async Task<EnrollmentResult> EnrollAsync(int courseId, int studentId)
    {
        var course = await db.Courses
            .Where(c => c.Id == courseId)
            .Select(c => new { c.Capacity, EnrolledCount = c.Enrollments.Count })
            .FirstOrDefaultAsync();

        if (course is null)
        {
            return EnrollmentResult.Failure(EnrollmentErrorKind.NotFound, $"Course {courseId} does not exist.");
        }

        var student = await db.Students
            .Where(s => s.Id == studentId)
            .Select(s => new
            {
                s.FirstName,
                s.LastName,
                EnrollmentCount = s.Enrollments.Count,
                AlreadyEnrolled = s.Enrollments.Any(e => e.CourseId == courseId)
            })
            .FirstOrDefaultAsync();

        if (student is null)
        {
            return EnrollmentResult.Failure(EnrollmentErrorKind.NotFound, $"Student {studentId} does not exist.");
        }

        if (student.AlreadyEnrolled)
        {
            return EnrollmentResult.Failure(EnrollmentErrorKind.Conflict, "The student is already enrolled in this course.");
        }

        if (course.EnrolledCount >= course.Capacity)
        {
            return EnrollmentResult.Failure(EnrollmentErrorKind.Conflict, "The course has no free places.");
        }

                var context = new EnrollmentContext(
            course.Capacity, course.EnrolledCount, student.EnrollmentCount, student.AlreadyEnrolled);

        var error = rules
            .Select(rule => rule.Check(context))
            .FirstOrDefault(error => error is not null);

        if (error is not null)
        {
            return EnrollmentResult.Failure(error);
        }

        var enrollment  = new Enrollment
        {
            CourseId = courseId,
            StudentId = studentId,
            EnrolledAt = timeProvider.GetUtcNow().UtcDateTime
        };

        db.Enrollments.Add(enrollment);
        await db.SaveChangesAsync();

        return EnrollmentResult.Success(new EnrollmentResponse(
            courseId, studentId, enrollment.EnrolledAt ?? DateTimeOffset.MinValue));
    }
}
