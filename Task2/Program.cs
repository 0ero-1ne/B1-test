using Task2.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<Task2DbContext>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

app.UseStaticFiles();

app.UseRouting();

app.UseEndpoints(e => e.MapControllers());

app.UseStatusCodePages();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();
