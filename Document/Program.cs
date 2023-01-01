using Document.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Session;
using Document.Helpers;
using Rotativa.AspNetCore;
using Google.Apis.Util;
using Utilities = Document.Helpers.Utilities;

var builder = WebApplication.CreateBuilder(args);
var helper = new Utilities();
var guid = Guid.NewGuid().ToString();
string file = $@"C:\EDoc\Log_ConnectionString_{DateTime.Today:yyyyMMdd}";
helper.WriteLog(file, $">> {DateTime.Now:yyyy-MM-dd HH:mm:ss},INFO,ConnectionString,{guid}");
helper.WriteLog(file, $">> {Environment.GetEnvironmentVariable("RequestDB", EnvironmentVariableTarget.Machine)}");

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<RequestContext>(options => options.UseSqlServer(System.Environment.GetEnvironmentVariable("RequestDB", EnvironmentVariableTarget.Machine), builder =>
            {
                builder.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
            }
    ));
builder.Services.AddDistributedMemoryCache();

builder.Services.AddScoped<IUtilities, Utilities>();

builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".AdventureWorks.Session";
    options.IdleTimeout = TimeSpan.FromSeconds(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error/Index");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();
app.UseSession();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

var env = app.Environment;
RotativaConfiguration.Setup((Microsoft.AspNetCore.Hosting.IHostingEnvironment)env);
app.Run();
