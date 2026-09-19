using Microsoft.EntityFrameworkCore;
using BookNest.Models;

namespace BookNest.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Book> Books { get; set; }

        public DbSet<Author> Authors { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<Review> Reviews { get; set; }

        public DbSet<BookCollection> BookCollections { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure many-to-many relationship between Book and BookCollection
            modelBuilder.Entity<Book>()
                .HasMany(b => b.Collections)
                .WithMany(c => c.Books)
                .UsingEntity(j => j.ToTable("BookBookCollection"));
        }
    }
}