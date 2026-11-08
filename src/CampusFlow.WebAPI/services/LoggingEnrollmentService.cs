using System.Diagnostics;

namespace CampusFlow.WebAPI.Services;

public class LoggingEnrollmentService(IEnrollmentService inner, ILogger<LoggingEnrollmentService> logger)
    : IEnrollmentService
{
    public async Task<EnrollmentResult> EnrollAsync(int courseId, int studentId)
    {
        var start = Stopwatch.GetTimestamp();
        var result = await inner.EnrollAsync(courseId, studentId);
        var elapsed = Stopwatch.GetElapsedTime(start);

        if (result.Error is null)
        {
            logger.LogInformation(
                "Enrolled student {StudentId} in course {CourseId} in {ElapsedMs} ms",
                studentId, courseId, elapsed.TotalMilliseconds);
        }
        else
        {
            logger.LogWarning(
                "Enrollment of student {StudentId} in course {CourseId} failed: {Message}",
                studentId, courseId, result.Error.Message);
        }

        return result;
    }
}
