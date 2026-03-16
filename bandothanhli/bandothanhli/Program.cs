using bandothanhli;
using bandothanhli.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ── Services ──
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("bandothanhli")));

// ── Thêm Authentication ──
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opt =>
    {
        opt.LoginPath = "/Admin/Auth/DangNhap";
        opt.AccessDeniedPath = "/Admin/Auth/DangNhap";
        opt.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

var app = builder.Build();

// ── Pipeline ──
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();

// ── Thứ tự này quan trọng, Authentication phải trước Authorization ──
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

// ── Route Areas đặt TRƯỚC route default ──
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();