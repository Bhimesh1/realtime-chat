using ChatApp.Server.Hubs;
using ChatApp.Server.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddSignalR();
builder.Services.AddSingleton<MessageValidator>();

var app = builder.Build();

app.UseCors("Frontend");

app.MapGet("/", () => "ChatApp.Server is running.");
app.MapHub<ChatHub>("/chatHub");

app.Run();
