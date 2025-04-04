using Microsoft.EntityFrameworkCore;
using project_DBA_VISO.Models.Data;
using project_DBA_VISO.Services.Contract;
using project_DBA_VISO.Services.Inplementation;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using project_DBA_VISO.Services.Implementation;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
//builder.Services.AddControllersWithViews();

// Add services to the container.
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
});

// Add services to the container.

builder.Services.AddDbContext<DBContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("DBContext")));
//builder.Services.AddControllersWithViews();

builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

// Register services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProductService, ProductService>(); // Add this line


builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => 
    {
        options.LoginPath = "/User/Signin/";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
    });

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(
        new ResponseCacheAttribute
        {
            NoStore = true,
            Location = ResponseCacheLocation.None,
        });
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
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

//app.UseStatusCodePagesWithRedirects("/Home/Error");
app.UseStatusCodePagesWithReExecute("/Home/Error");


app.MapControllerRoute(
    name: "User",
    pattern: "{controller=User}/{action=Signin}/{id?}");

app.MapControllerRoute(
    name: "Home",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "Product",
    pattern: "{controller=Product}/{action=Productlist}/{id?}");

app.MapControllerRoute(
    name: "Brand",
    pattern: "{controller=Brand}/{action=Brandlist}/{id?}");

app.MapControllerRoute(
    name: "Category",
    pattern: "{controller=Category}/{action=Categorylist}/{id?}");

app.MapControllerRoute(
    name: "Sales",
    pattern: "{controller=Sales}/{action=Saleslist}/{id?}");

app.MapControllerRoute(
    name: "Purchase",
    pattern: "{controller=Purchase}/{action=Purchaselist}/{id?}");

app.MapControllerRoute(
    name: "Report",
    pattern: "{controller=Report}/{action=Reportlist}/{id?}");

app.MapControllerRoute(
    name: "Paperbin",
    pattern: "{controller=Paperbin}/{action=Productpaper}/{id?}");

app.MapControllerRoute(
    name: "People",
    pattern: "{controller=People}/{action=Clientlist}/{id?}");

IWebHostEnvironment env = app.Environment;
Rotativa.AspNetCore.RotativaConfiguration.Setup(env.WebRootPath, "../Rotativa/Windows");

app.Run();
