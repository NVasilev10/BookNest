using Xunit;
using Microsoft.EntityFrameworkCore;
using BookNest.Data;
using BookNest.Models;

namespace BookNest.Tests
{
    public class ReviewRepositoryTests
    {
        private ApplicationDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "BookNestTest_" + Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task AddReview_ShouldSucceed()
        {
            // Arrange
            using (var context = GetDbContext())
            {
                var author = new Author { Name = "Test Author" };
                var category = new Category { Name = "Test Category" };
                context.Authors.Add(author);
                context.Categories.Add(category);
                var book = new Book 
                { 
                    Title = "Test Book",
                    AuthorId = author.Id,
                    CategoryId = category.Id
                };
                context.Books.Add(book);
                await context.SaveChangesAsync();

                var review = new Review
                {
                    BookId = book.Id,
                    Rating = 5,
                    Comment = "Great book!"
                };

                // Act
                context.Reviews.Add(review);
                await context.SaveChangesAsync();

                // Assert
                var savedReview = await context.Reviews.FirstOrDefaultAsync(r => r.BookId == book.Id);
                Assert.NotNull(savedReview);
                Assert.Equal(5, savedReview.Rating);
            }
        }

        [Fact]
        public async Task GetReviewsByBook_ShouldReturnOnlyBookReviews()
        {
            // Arrange
            using (var context = GetDbContext())
            {
                var author = new Author { Name = "Author" };
                var category = new Category { Name = "Category" };
                context.Authors.Add(author);
                context.Categories.Add(category);

                var book1 = new Book { Title = "Book 1", AuthorId = author.Id, CategoryId = category.Id };
                var book2 = new Book { Title = "Book 2", AuthorId = author.Id, CategoryId = category.Id };
                context.Books.AddRange(book1, book2);
                await context.SaveChangesAsync();

                context.Reviews.AddRange(
                    new Review { BookId = book1.Id, Rating = 5, Comment = "Great" },
                    new Review { BookId = book1.Id, Rating = 4, Comment = "Good" },
                    new Review { BookId = book2.Id, Rating = 3, Comment = "OK" }
                );
                await context.SaveChangesAsync();

                // Act
                var reviews = context.Reviews.Where(r => r.BookId == book1.Id).ToList();

                // Assert
                Assert.Equal(2, reviews.Count());
            }
        }

        [Fact]
        public async Task CalculateAverageRating_ShouldBeCorrect()
        {
            // Arrange
            using (var context = GetDbContext())
            {
                var author = new Author { Name = "Author" };
                var category = new Category { Name = "Category" };
                context.Authors.Add(author);
                context.Categories.Add(category);

                var book = new Book { Title = "Book", AuthorId = author.Id, CategoryId = category.Id };
                context.Books.Add(book);
                await context.SaveChangesAsync();

                context.Reviews.AddRange(
                    new Review { BookId = book.Id, Rating = 5 },
                    new Review { BookId = book.Id, Rating = 3 },
                    new Review { BookId = book.Id, Rating = 4 }
                );
                await context.SaveChangesAsync();

                // Act
                var reviews = context.Reviews.Where(r => r.BookId == book.Id).ToList();
                var averageRating = reviews.Average(r => r.Rating);

                // Assert
                Assert.Equal(4, averageRating);
            }
        }
    }
}
