var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add Swagger/OpenAPI generation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure CORS policies
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:7145")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Enable Swagger UI for development environment
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Serve default and static files
app.UseDefaultFiles();
app.UseStaticFiles();

// Redirect HTTP to HTTPS (if required)
app.UseHttpsRedirection();

// Enable CORS middleware
app.UseCors();

// Enable authorization middleware
app.UseAuthorization();

// Map API controllers
app.MapControllers();

// Start the application
app.Run();
