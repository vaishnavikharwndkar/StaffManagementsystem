using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StaffTaskManagement.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Database")));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("ManagerOnly", policy => policy.RequireRole("Manager", "Admin"));
    options.AddPolicy("EmployeeOnly", policy => policy.RequireRole("Employee", "Manager", "Admin"));
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Database");
    try
    {
        context.Database.EnsureCreated();
        DbSeeder.Seed(context);
        var csb = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(
            builder.Configuration.GetConnectionString("Database") ?? string.Empty);
        logger.LogInformation(
            "Using SQL Server instance {DataSource}, database {Database}. Tables are created if they did not exist.",
            csb.DataSource,
            context.Database.GetDbConnection().Database);
    }
    catch (Exception ex)
    {
        logger.LogError(
            ex,
            "Could not connect to SQL Server. Set ConnectionStrings:Database in appsettings.json — e.g. (localdb)\\MSSQLLocalDB, or .\\SQLEXPRESS for SQL Express on this PC.");
        throw;
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
