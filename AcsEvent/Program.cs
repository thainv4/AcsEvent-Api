using AcsEvent.Mapping;
using AcsEvent.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddDbContext<AcsEvent.Context.AcsEventDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddAutoMapper(_ => { }, typeof(GeneralProfile).Assembly);

builder.Services.AddHttpClient();
builder.Services.AddScoped<ThietBiService>();
builder.Services.AddScoped<HikvisionService>();
builder.Services.AddScoped<EmployeeService>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://192.168.200.56:8080", "http://localhost:8080")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.

    app.UseSwagger();
    app.UseSwaggerUI();


app.UseHttpsRedirection();

app.UseCors("AllowFrontend"); 

app.MapControllers();

app.Run();
