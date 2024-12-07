using Microsoft.EntityFrameworkCore;
using MyShop1.Data;

var builder = WebApplication.CreateBuilder(args);

// ��������� �������� ���� ������
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// ��������� ������� ��� ���������
app.MapControllerRoute(
    name: "categories",
    pattern: "Categories/{action=Index}/{id?}",
    defaults: new { controller = "Categories", action = "Index" });

// ��������� �������� �������
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
