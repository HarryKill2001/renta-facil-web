using Microsoft.EntityFrameworkCore;
using RentaFacil.Shared.Interfaces;
using BookingService.Data;
using BookingService.Repositories;
using BookingService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure Entity Framework
builder.Services.AddDbContext<BookingDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register repositories
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IReservationRepository, ReservationRepository>();

// Register HTTP clients for service communication
builder.Services.AddHttpClient("VehicleService", client =>
{
    var baseUrl = builder.Configuration["Services:VehicleService:BaseUrl"];
    client.BaseAddress = new Uri(baseUrl ?? "https://localhost:5003");
});

// Register business services
builder.Services.AddScoped<ICustomerBusinessService, CustomerBusinessService>();
builder.Services.AddScoped<IReservationBusinessService, ReservationBusinessService>();

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure CORS for frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");

app.UseAuthorization();
app.MapControllers();

// Ensure database is created and migrated
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<BookingDbContext>();
    context.Database.EnsureCreated();
    
    // Remove foreign key constraint that conflicts with microservices architecture
    try
    {
        context.Database.ExecuteSqlRaw("ALTER TABLE Reservations DROP CONSTRAINT FK_Reservations_Vehicles_VehicleId");
        app.Logger.LogInformation("Successfully removed FK_Reservations_Vehicles_VehicleId constraint");
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "Failed to remove FK constraint (may not exist): {Message}", ex.Message);
    }
}

app.Run();
