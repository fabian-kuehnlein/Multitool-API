using System.Data.Common;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using MultitoolApi.ConfigModels;
using MultitoolApi.Infrastructure.Businesslogic.Services;

var builder = WebApplication.CreateBuilder(args);

// Calendar Infrastructure
builder.Services.AddScoped<ICalendarService, CalendarService>();
builder.Services.AddScoped<ICalendarEventRepository, CalendarEventRepository>();

// Custom-Table Infrastructure
builder.Services.AddScoped<ICustomTableRepository, CustomTableRepository>();
builder.Services.AddScoped<ICustomTableService, CustomTableService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(11, 7, 2)),
        mySqlOptions => mySqlOptions
            .EnableRetryOnFailure(
                maxRetryCount: 10,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorNumbersToAdd: null
        ))
    );

Console.WriteLine("Using connection string: " + builder.Configuration.GetConnectionString("DefaultConnection"));

builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    opts.JsonSerializerOptions.Converters.Add(
        new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.AddHttpClient<ICalendarEventRepository, CalendarEventRepository>(client =>
{
    client.BaseAddress = new Uri("https://get.api-feiertage.de");
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseRouting();
app.UseCors("AllowAll");

app.MapControllers().RequireCors("AllowAll");

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    if (builder.Environment.IsProduction())
    {
        dbContext.Database.Migrate();
    }
};

app.Run();