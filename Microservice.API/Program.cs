using Microservice.API;
using Microservice.Application;
using Microservice.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Registrar servicios de aplicación e infraestructura
builder.Services.AddApiServices(builder.Configuration, builder.Environment);
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

// Add exception handling middleware (Modern .NET 10 approach using IExceptionHandler)
app.UseExceptionHandler(_ => { });

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        // v1.0 - Current stable version
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1.0 (Stable)");
        
        // v2.0 - Enhanced version
        c.SwaggerEndpoint("/swagger/v2/swagger.json", "API v2.0 (Preview)");
        
        c.RoutePrefix = string.Empty;
        c.DisplayRequestDuration();
        
        // Add version selector dropdown
        c.ConfigObject.AdditionalItems["showExtensions"] = true;
        c.ConfigObject.AdditionalItems["showCommonExtensions"] = true;
    });
}

//using (var scope = app.Services.CreateScope())
//{
//    var db = scope.ServiceProvider.GetRequiredService<ExampleDbContext>();

//    if (builder.Environment.IsDevelopment() || builder.Environment.EnvironmentName == "Testing")
//    {
//        await db.Database.EnsureDeletedAsync();
//        await db.Database.EnsureCreatedAsync();
//    }
//    else
//        await db.Database.MigrateAsync();

//    //DbInitializer.Seed(db);
//}

app.UseCors("DefaultCors");

//app.MapHealthChecks("/health");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();