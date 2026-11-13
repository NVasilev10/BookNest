using BookNest.Data;
using BookNest.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;

var appInstanceId = Guid.NewGuid().ToString("N");
var builder = WebApplication.CreateBuilder(args);

// Configure the URLs
builder.WebHost.UseUrls("https://localhost:7281", "http://localhost:5112");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultUI()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnSigningIn = context =>
    {
        if (!context.Properties.IsPersistent)
        {
            context.Properties.Items["BookNestAppInstance"] = appInstanceId;
        }
        return Task.CompletedTask;
    };
    options.Events.OnValidatePrincipal = async context =>
    {
        if (!context.Properties.IsPersistent &&
            (!context.Properties.Items.TryGetValue("BookNestAppInstance", out var instanceId) || instanceId != appInstanceId))
        {
            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            return;
        }

        await SecurityStampValidator.ValidatePrincipalAsync(context);
    };
});

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// Seed database with initial data
try 
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // Apply migrations
        dbContext.Database.Migrate();

        // Seed Roles
        var roles = new[] { "User", "Administrator" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Seed Authors
        if (!dbContext.Authors.Any())
        {
            var authors = new List<Author>
            {
                new Author 
                { 
                    Name = "Harper Lee", 
                    Biography = "American novelist best known for To Kill a Mockingbird" 
                },
                new Author 
                { 
                    Name = "J.K. Rowling", 
                    Biography = "British author of the Harry Potter series" 
                },
                new Author 
                { 
                    Name = "George Orwell", 
                    Biography = "English novelist and critic" 
                },
                new Author 
                { 
                    Name = "Jane Austen", 
                    Biography = "English novelist known for romantic fiction" 
                },
                new Author 
                { 
                    Name = "Stephen King", 
                    Biography = "American bestselling author of horror and thriller novels" 
                }
            };
            dbContext.Authors.AddRange(authors);
            dbContext.SaveChanges();
        }

        // Seed Categories
        if (!dbContext.Categories.Any())
        {
            var categories = new List<Category>
            {
                new Category 
                { 
                    Name = "Fiction", 
                    Description = "Fictional stories and novels" 
                },
                new Category 
                { 
                    Name = "Mystery", 
                    Description = "Mystery and detective stories" 
                },
                new Category 
                { 
                    Name = "Fantasy", 
                    Description = "Fantasy and magical worlds" 
                },
                new Category 
                { 
                    Name = "Romance", 
                    Description = "Romantic stories" 
                },
                new Category 
                { 
                    Name = "Science Fiction", 
                    Description = "Science fiction and futuristic stories" 
                },
                new Category 
                { 
                    Name = "Horror", 
                    Description = "Scary and horror stories" 
                }
            };
            dbContext.Categories.AddRange(categories);
            dbContext.SaveChanges();
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error seeding database: {ex.Message}");
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseStatusCodePagesWithReExecute("/error/{0}");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages();

app.MapControllerRoute(
    name: "areaRoute",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();app.Run();