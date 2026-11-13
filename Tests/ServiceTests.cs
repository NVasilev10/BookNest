using BookNest.Data;
using BookNest.Models;
using BookNest.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BookNest.Tests;

public class ServiceTests
{
    [Fact]
    public async Task ToggleFavoriteAsync_TogglesFavoriteAndReturnsFalseForMissingBook()
    {
        await using var context = CreateContext();
        await SeedUserAndBookAsync(context, "user-1");
        var service = new BookCatalogService(context);

        Assert.True(await service.ToggleFavoriteAsync(1, "user-1"));
        Assert.True(await context.BookFavorites.AnyAsync(favorite => favorite.UserId == "user-1" && favorite.BookId == 1));

        Assert.True(await service.ToggleFavoriteAsync(1, "user-1"));
        Assert.False(await context.BookFavorites.AnyAsync());
        Assert.False(await service.ToggleFavoriteAsync(999, "user-1"));
    }

    [Fact]
    public async Task ReviewService_CreatesAndModeratesReview()
    {
        await using var context = CreateContext();
        await SeedUserAndBookAsync(context, "user-1");
        var service = new ReviewService(context);
        var review = new Review { Rating = 5, Comment = "Enjoyed it." };

        Assert.True(await service.CreateAsync(1, "user-1", review));
        Assert.Equal("user-1", review.UserId);
        Assert.Equal(1, review.BookId);
        Assert.False(await service.CreateAsync(999, "user-1", new Review { Rating = 4 }));

        Assert.True(await service.SetApprovalAsync(review.Id, false));
        Assert.False((await context.Reviews.FindAsync(review.Id))!.IsApproved);
        Assert.False(await service.SetApprovalAsync(999, false));

        Assert.True(await service.DeleteAsync(review.Id));
        Assert.False(await service.DeleteAsync(review.Id));
    }

    [Fact]
    public async Task BookCollectionService_EnforcesOwnershipAndPersistsChanges()
    {
        await using var context = CreateContext();
        await SeedUserAndBookAsync(context, "user-1");
        context.Users.Add(new ApplicationUser { Id = "user-2", UserName = "user-2" });
        await context.SaveChangesAsync();

        var service = new BookCollectionService(context);
        var collection = new BookCollection { Name = "Favorites" };
        await service.CreateAsync(collection, "user-1");

        Assert.Equal("user-1", collection.UserId);
        Assert.True(await service.AddBookAsync(1, collection.Id, "user-1"));
        Assert.False(await service.AddBookAsync(1, collection.Id, "user-2"));
        Assert.Single((await service.GetOwnedWithBooksAsync(collection.Id, "user-1"))!.Books);
        Assert.Null(await service.GetOwnedAsync(collection.Id, "user-2"));

        Assert.True(await service.UpdateAsync(collection.Id, "user-1", "Updated", "Description"));
        Assert.Equal("Updated", collection.Name);
        Assert.True(await service.RemoveBookAsync(1, collection.Id, "user-1"));
        Assert.Empty((await service.GetOwnedWithBooksAsync(collection.Id, "user-1"))!.Books);
        Assert.True(await service.DeleteAsync(collection.Id, "user-1"));
        Assert.False(await service.DeleteAsync(collection.Id, "user-1"));
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"BookNestServices_{Guid.NewGuid()}")
            .Options;

        return new ApplicationDbContext(options);
    }

    private static async Task SeedUserAndBookAsync(ApplicationDbContext context, string userId)
    {
        context.Users.Add(new ApplicationUser { Id = userId, UserName = userId });
        context.Authors.Add(new Author { Id = 1, Name = "Test Author" });
        context.Categories.Add(new Category { Id = 1, Name = "Test Category" });
        context.Books.Add(new Book
        {
            Id = 1,
            Title = "Test Book",
            PublishedYear = 2020,
            AuthorId = 1,
            CategoryId = 1
        });
        await context.SaveChangesAsync();
    }
}
