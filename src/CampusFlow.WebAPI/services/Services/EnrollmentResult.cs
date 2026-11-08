using CampusFlow.WebAPI.Contracts;


namespace CampusFlow.WebAPI.Services;

public enum EnrollmentErrorKind
{
    NotFound,
    Conflict
}

public record EnrollmentError(EnrollmentErrorKind Kind, string Message);

public record EnrollmentResult(EnrollmentResponse? Enrollment, EnrollmentError? Error)
{
    public static EnrollmentResult Success(EnrollmentResponse enrollment) => new(enrollment, null);

    public static EnrollmentResult Failure(EnrollmentError error) => new(null, error);

    public static EnrollmentResult Failure(EnrollmentErrorKind kind, string message) =>
        Failure(new EnrollmentError(kind, message));
}
