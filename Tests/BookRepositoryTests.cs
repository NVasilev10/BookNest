using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using BookNest.Data;
using BookNest.Models;

namespace BookNest.Tests
{
    public class BookRepositoryTests
    {
        private ApplicationDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "BookNestTest_" + Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task GetBooks_ShouldReturnAllBooks()
        {
            // Arrange
            using (var context = GetDbContext())
            {
                context.Authors.Add(new Author { Id = 1, Name = "Test Author" });
                context.Categories.Add(new Category { Id = 1, Name = "Test Category" });
                context.Books.Add(new Book 
                { 
                    Id = 1, 
                    Title = "Test Book", 
                    AuthorId = 1, 
                    CategoryId = 1 
                });
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = GetDbContext())
            {
                var books = await context.Books.ToListAsync();
                // Assert
                Assert.Single(books);
            }
        }

        [Fact]
        public async Task AddBook_ShouldIncreaseCount()
        {
            // Arrange
            using (var context = GetDbContext())
            {
                var author = new Author { Id = 1, Name = "Test Author" };
                var category = new Category { Id = 1, Name = "Test Category" };
                context.Authors.Add(author);
                context.Categories.Add(category);
                await context.SaveChangesAsync();

                var book = new Book 
                { 
                    Title = "New Book", 
                    AuthorId = author.Id, 
                    CategoryId = category.Id 
                };

                // Act
                context.Books.Add(book);
                await context.SaveChangesAsync();

                // Assert
                Assert.Single(context.Books);
            }
        }

        [Fact]
        public async Task SearchBooks_ByTitle_ShouldReturnMatches()
        {
            // Arrange
            using (var context = GetDbContext())
            {
                var author = new Author { Id = 1, Name = "Author" };
                var category = new Category { Id = 1, Name = "Category" };
                context.Authors.Add(author);
                context.Categories.Add(category);
                context.Books.AddRange(
                    new Book { Title = "Harry Potter", AuthorId = 1, CategoryId = 1 },
                    new Book { Title = "The Hobbit", AuthorId = 1, CategoryId = 1 }
                );
                await context.SaveChangesAsync();

                // Act
                var results = context.Books
                    .Where(b => b.Title.Contains("Harry"))
                    .ToList();

                // Assert
                Assert.Single(results);
                Assert.Equal("Harry Potter", results.First().Title);
            }
        }
    }
}
