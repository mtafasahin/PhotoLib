using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


// Add Hangfire with PostgreSQL
builder.Services.AddHangfire(config =>
    config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
          .UseSimpleAssemblyNameTypeSerializer()
          .UseRecommendedSerializerSettings()
           .UsePostgreSqlStorage(builder.Configuration.GetConnectionString("HangfireConnection"), new PostgreSqlStorageOptions() {
            InvisibilityTimeout = TimeSpan.FromHours(3)
          }));

// Add the Hangfire server
builder.Services.AddHangfireServer();

builder.Services.AddScoped<ImageProcessingService>();
builder.Services.AddScoped<ImageSimilarityService>();

var app = builder.Build();

// "D:/resimler" dizinini tarayıcıya sunmak
var imagePath = @"D:/resimler"; // Resimlerin bulunduğu dizin
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(imagePath),
    RequestPath = "/images" // URL'de bu path üzerinden erişim sağlanacak
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

// Hangfire Dashboard
app.UseHangfireDashboard();


app.Run();
