using Microsoft.EntityFrameworkCore;

namespace practice.Models
{
    public class Context : DbContext
    {
        public Context(DbContextOptions<Context> options)
            : base(options)
        {
        }

        public DbSet<Client> Clients { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Schedule> Schedule { get; set; }
        public DbSet<Hall> Hall { get; set; }
        public DbSet<Movie> Movie { get; set; }
        public DbSet<Showing> Showings { get; set; }
        public DbSet<Genre> Genres { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Client>()
                .HasKey(c => c.client_id);
            modelBuilder.Entity<Ticket>()
                .HasKey(t => t.ticket_id);
            modelBuilder.Entity<Schedule>()
                .HasKey(s => s.schedule_id);
            modelBuilder.Entity<Hall>()
                .HasKey(h => h.hall_id);
            modelBuilder.Entity<Movie>()
                .HasKey(m => m.movie_id);
            modelBuilder.Entity<Showing>()
                .HasKey(s => s.showing_id);
            modelBuilder.Entity<Genre>()
                .HasKey(g => g.genre_id);
        }
    }

}
