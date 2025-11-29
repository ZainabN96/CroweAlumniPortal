using System.Text.Json.Serialization;
using CroweAlumniPortal.Data;
using CroweAlumniPortal.Data.IServices;
using CroweAlumniPortal.Data.Services;
using CroweAlumniPortal.Extenstions;
using CroweAlumniPortal.Helper;
using CroweAlumniPortal.Middlewares;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Add services to the container.
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(9);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddTransient<ExceptionMiddleware>(services =>
{
    var logger = services.GetRequiredService<ILogger<ExceptionMiddleware>>();
    var env = services.GetRequiredService<IWebHostEnvironment>();
    var next = services.GetRequiredService<RequestDelegate>();
    return new ExceptionMiddleware(next, logger, env);
});
builder.Services.AddHttpClient();
builder.Services.AddControllers();
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton(AutoMapperConfig.RegisterMappings());
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IMailService, MailService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IAlumniService, AlumniService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IEmailSender, EmailSenderAdapter>();
builder.Services.AddScoped<IAdminDirectory, DbAdminDirectory>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddSingleton<IUserIdProvider, ClaimUserIdProvider>();
builder.Services.AddControllers().AddJsonOptions(o =>
{
    o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

builder.Services.AddSignalR();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
        ctx.Context.Response.Headers.Append("Pragma", "no-cache");
        ctx.Context.Response.Headers.Append("Expires", "0");
    }
});

app.MapHub<NotificationHub>("/notificationHub");
app.MapHub<ChatHub>("/chathub");

app.UseRouting();

app.UseAuthorization();

app.ConfigureExceptionHandler();

app.UseSession();

app.MapControllerRoute(
    name: "default",
   pattern: "{controller=Dashboard}/{action=Dashboard}/{id?}");

app.Run();