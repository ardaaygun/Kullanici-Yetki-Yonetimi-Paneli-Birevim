using KullaniciYonetimi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.
builder.Services.AddControllersWithViews();//Mvc mimarisi kullanabilmek için uygulammaya eklenmelidir bu sayade uygulama mvc davranışı sergileyebilir.
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        // Yetkisiz biri gizli bir sayfaya girmeye çalışırsa sistem onu otomatik buraya postalar
        options.LoginPath = "/Account/Login";

        // Çıkış yapma işlemi için kullanılacak adres
        options.LogoutPath = "/Account/Logout";

        // Giriş yapmış ama örneğin "Admin" yetkisi olmayan biri o sayfaya girmeye çalışırsa buraya düşer
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
