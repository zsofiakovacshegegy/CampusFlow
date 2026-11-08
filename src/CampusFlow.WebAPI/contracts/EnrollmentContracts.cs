using System.ComponentModel.DataAnnotations;

namespace CampusFlow.WebAPI.Contracts;

public record EnrollmentResponse(int CourseId, int StudentId, DateTimeOffset? EnrolledAt);

public record EnrollStudentRequest([Range(1, int.MaxValue)] int StudentId);
