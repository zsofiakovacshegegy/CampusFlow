namespace CampusFlow.WebAPI.Services;

public record EnrollmentContext(int Capacity, int EnrolledCount, int StudentEnrollmentCount, bool AlreadyEnrolled);

public interface IEnrollmentRule
{
    EnrollmentError? Check(EnrollmentContext context);
}

public class NotAlreadyEnrolledRule : IEnrollmentRule
{
    public EnrollmentError? Check(EnrollmentContext context) =>
        context.AlreadyEnrolled
            ? new EnrollmentError(EnrollmentErrorKind.Conflict, "The student is already enrolled in this course.")
            : null;
}

public class CapacityRule : IEnrollmentRule
{
    public EnrollmentError? Check(EnrollmentContext context) =>
        context.EnrolledCount >= context.Capacity
            ? new EnrollmentError(EnrollmentErrorKind.Conflict, "The course has no free places.")
            : null;
}

public class MaxCoursesPerStudentRule : IEnrollmentRule
{
    private const int MaxCourses = 3;

    public EnrollmentError? Check(EnrollmentContext context) =>
        context.StudentEnrollmentCount >= MaxCourses
            ? new EnrollmentError(EnrollmentErrorKind.Conflict, $"A student can enroll in {MaxCourses} courses only.")
            : null;
}
