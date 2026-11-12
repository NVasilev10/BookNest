using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using BookNest.Models;

namespace BookNest.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
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

        public DbSet<BookFavorite> BookFavorites { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<BookFavorite>()
                .HasKey(f => new { f.UserId, f.BookId });

            // Configure many-to-many relationship between Book and BookCollection
            modelBuilder.Entity<Book>()
                .HasMany(b => b.Collections)
                .WithMany(c => c.Books)
                .UsingEntity(j => j.ToTable("BookBookCollection"));

            // Configure Review to ApplicationUser relationship
            modelBuilder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure BookCollection to ApplicationUser relationship
            modelBuilder.Entity<BookCollection>()
                .HasOne(bc => bc.User)
                .WithMany(u => u.BookCollections)
                .HasForeignKey(bc => bc.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
