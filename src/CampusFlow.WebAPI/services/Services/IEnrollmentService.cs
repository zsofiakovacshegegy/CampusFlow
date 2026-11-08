namespace CampusFlow.WebAPI.Services;

public interface IEnrollmentService
{
    Task<EnrollmentResult> EnrollAsync(int courseId, int studentId);
}
