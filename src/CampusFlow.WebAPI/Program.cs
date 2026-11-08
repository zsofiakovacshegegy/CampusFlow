using System.Text.Json;
using ..database;
using CampusFlow.WebAPI.models;
using CampusFlow.WebAPI.Endpoints;
using Microsoft.EntityFrameworkCore;
using CampusFlow.WebAPI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddDbContext<CampusFlowDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("CampusFlow")));

builder.Services.AddSingleton<IEnrollmentRule, NotAlreadyEnrolledRule>();
builder.Services.AddSingleton<IEnrollmentRule, CapacityRule>();
builder.Services.AddSingleton<IEnrollmentRule, MaxCoursesPerStudentRule>();


builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<EnrollmentService>();
builder.Services.AddScoped<IEnrollmentService>(sp =>
    new LoggingEnrollmentService(
        sp.GetRequiredService<EnrollmentService>(),
        sp.GetRequiredService<ILogger<LoggingEnrollmentService>>()));


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CampusFlowDbContext>();

    if (db.Database.EnsureCreated())
    {
        var path = Path.Combine(app.Environment.ContentRootPath, "data", "SeedData.json");
        var seed = JsonSerializer.Deserialize<SeedData>(File.ReadAllText(path), JsonSerializerOptions.Web)!;

        db.Courses.AddRange(seed.Courses);
        db.Students.AddRange(seed.Students);
        db.Enrollments.AddRange(seed.Enrollments);
        db.SaveChanges();
    }
}


using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CampusFlowDbContext>();

    if (db.Database.EnsureCreated())
    {
        var path = Path.Combine(app.Environment.ContentRootPath, "Data", "seed-data.json");
        var seed = JsonSerializer.Deserialize<SeedData>(File.ReadAllText(path), JsonSerializerOptions.Web)!;

        db.Courses.AddRange(seed.Courses);
        db.Students.AddRange(seed.Students);
        db.Enrollments.AddRange(seed.Enrollments);
        db.SaveChanges();
    }
}


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.MapGet("/swagger", () => Results.Content("""
        <!doctype html>
        <html>
        <head>
          <title>CampusFlow API</title>
          <link rel="stylesheet" href="https://unpkg.com/swagger-ui-dist@5.33.1/swagger-ui.css" />
        </head>
        <body>
          <div id="swagger-ui"></div>
          <script src="https://unpkg.com/swagger-ui-dist@5.33.1/swagger-ui-bundle.js"></script>
          <script>
            SwaggerUIBundle({ url: "/openapi/v1.json", dom_id: "#swagger-ui" });
          </script>
        </body>
        </html>
        """, "text/html"))
        .ExcludeFromDescription();
}

app.UseHttpsRedirection();

//app.MapGet("/api/ping", () => TypedResults.Ok(new { Status = "ok", Time = DateTime.UtcNow }));
app.MapCourseEndpoints();
app.MapStudentEndpoints();
app.MapStatisticsEndpoints();

app.Run();
