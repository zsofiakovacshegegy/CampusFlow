using CampusFlow.WebAPI.Contracts;
using CampusFlow.WebAPI.Data;
using CampusFlow.WebAPI.models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using CampusFlow.WebAPI.Services;

namespace CampusFlow.WebAPI.Endpoints;

public static class EnrollmentEndpoints
{
    public static RouteGroupBuilder MapEnrollmentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/courses/{courseId:int}/enrollments").WithTags("Enrollments");

        group.MapGet("/", GetEnrollments);
        group.MapPost("/", EnrollStudent);
        group.MapDelete("/{studentId:int}", RemoveEnrollment);

        return group;
    }

    static async Task<Results<Ok<List<EnrollmentResponse>>, NotFound>> GetEnrollments(
        int courseId, CampusFlowDbContext db)
    {
        if (!await db.Courses.AnyAsync(c => c.Id == courseId))
        {
            return TypedResults.NotFound();
        }

        var enrollments = await db.Enrollments
            .Where(e => e.CourseId == courseId)
            .OrderBy(e => e.StudentId)
            .Select(e => new EnrollmentResponse(
                (int)e.CourseId, (int)e.StudentId, e.EnrolledAt))
            .ToListAsync();

        return TypedResults.Ok(enrollments);
    }



    static async Task<Results<Created<EnrollmentResponse>, ProblemHttpResult>> EnrollStudent(
    int courseId, EnrollStudentRequest request, IEnrollmentService enrollmentService)
{
    var result = await enrollmentService.EnrollAsync(courseId, request.StudentId);

    if (result.Error is { } error)
    {
        var statusCode = error.Kind == EnrollmentErrorKind.NotFound
            ? StatusCodes.Status404NotFound
            : StatusCodes.Status409Conflict;

        return TypedResults.Problem(statusCode: statusCode, detail: error.Message);
    }

    return TypedResults.Created($"/api/courses/{courseId}/enrollments", result.Enrollment!);
}


    static async Task<Results<NoContent, NotFound>> RemoveEnrollment(
        int courseId, int studentId, CampusFlowDbContext db)
    {
        var deleted = await db.Enrollments
            .Where(e => e.CourseId == courseId && e.StudentId == studentId)
            .ExecuteDeleteAsync();

        return deleted == 0 ? TypedResults.NotFound() : TypedResults.NoContent();
    }
}