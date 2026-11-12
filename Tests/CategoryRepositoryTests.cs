using Xunit;
using Microsoft.EntityFrameworkCore;
using BookNest.Data;
using BookNest.Models;

namespace BookNest.Tests
{
    public class CategoryRepositoryTests
    {
        private readonly string _databaseName = "BookNestTest_" + Guid.NewGuid();

        private ApplicationDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: _databaseName)
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task GetCategories_ShouldReturnAll()
        {
            // Arrange
            using (var context = GetDbContext())
            {
                context.Categories.AddRange(
                    new Category { Name = "Fiction" },
                    new Category { Name = "Mystery" },
                    new Category { Name = "Fantasy" }
                );
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = GetDbContext())
            {
                var categories = await context.Categories.ToListAsync();

                // Assert
                Assert.Equal(3, categories.Count());
            }
        }

        [Fact]
        public async Task AddCategory_ShouldIncreaseCount()
        {
            // Arrange
            using (var context = GetDbContext())
            {
                var category = new Category 
                { 
                    Name = "Science Fiction",
                    Description = "Science fiction books"
                };

                // Act
                context.Categories.Add(category);
                await context.SaveChangesAsync();

                // Assert
                var count = await context.Categories.CountAsync();
                Assert.Equal(1, count);
            }
        }

        [Fact]
        public async Task UpdateCategory_ShouldReflectChanges()
        {
            // Arrange
            using (var context = GetDbContext())
            {
                var category = new Category { Name = "Original Name" };
                context.Categories.Add(category);
                await context.SaveChangesAsync();
                var categoryId = category.Id;

                // Act
                var toUpdate = context.Categories.Find(categoryId);
                toUpdate!.Name = "Updated Name";
                await context.SaveChangesAsync();

                // Assert
                var updated = context.Categories.Find(categoryId);
                Assert.Equal("Updated Name", updated!.Name);
            }
        }
    }
}
