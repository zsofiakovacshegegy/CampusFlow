namespace CampusFlow.WebAPI.Contracts;

public record CourseResponse(long Id, string Code, string Title, int Credits, int Capacity, int EnrolledCount);

public record CreateCourseRequest(string Code, string Title, int Credits, int Capacity);

public record UpdateCourseRequest(string Title, int Credits, int Capacity);
