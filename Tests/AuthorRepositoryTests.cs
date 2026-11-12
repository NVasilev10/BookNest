using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using BookNest.Data;
using BookNest.Models;

namespace BookNest.Tests
{
    public class AuthorRepositoryTests
    {
        private ApplicationDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "BookNestTest_" + Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task GetAuthors_ShouldReturnAllAuthors()
        {
            // Arrange
            using (var context = GetDbContext())
            {
                context.Authors.Add(new Author { Id = 1, Name = "Author 1" });
                context.Authors.Add(new Author { Id = 2, Name = "Author 2" });
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = GetDbContext())
            {
                var authors = await context.Authors.ToListAsync();

                // Assert
                Assert.Equal(2, authors.Count());
            }
        }

        [Fact]
        public async Task AddAuthor_ShouldSucceed()
        {
            // Arrange
            using (var context = GetDbContext())
            {
                var author = new Author 
                { 
                    Name = "New Author",
                    Biography = "A new author biography"
                };

                // Act
                context.Authors.Add(author);
                await context.SaveChangesAsync();

                // Assert
                var savedAuthor = await context.Authors.FirstOrDefaultAsync(a => a.Name == "New Author");
                Assert.NotNull(savedAuthor);
                Assert.Equal("New Author", savedAuthor.Name);
            }
        }

        [Fact]
        public async Task DeleteAuthor_ShouldRemoveFromDatabase()
        {
            // Arrange
            using (var context = GetDbContext())
            {
                var author = new Author { Name = "Author to Delete" };
                context.Authors.Add(author);
                await context.SaveChangesAsync();
                var authorId = author.Id;

                // Act
                var toDelete = context.Authors.Find(authorId);
                context.Authors.Remove(toDelete!);
                await context.SaveChangesAsync();

                // Assert
                var count = await context.Authors.CountAsync();
                Assert.Equal(0, count);
            }
        }

        [Fact]
        public async Task SearchAuthors_ByName_ShouldReturnMatches()
        {
            // Arrange
            using (var context = GetDbContext())
            {
                context.Authors.AddRange(
                    new Author { Name = "Harper Lee" },
                    new Author { Name = "J.K. Rowling" },
                    new Author { Name = "J.R.R. Tolkien" }
                );
                await context.SaveChangesAsync();

                // Act
                var results = context.Authors
                    .Where(a => a.Name.Contains("J"))
                    .ToList();

                // Assert
                Assert.Equal(2, results.Count());
            }
        }
    }
}
