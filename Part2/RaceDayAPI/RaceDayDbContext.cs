using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Models;

namespace RaceDayAPI.Data
{
    public class RaceDayDbContext : DbContext
    {
        public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options)
            : base(options)
        {
        }

        // DbSets - one for each entity
        public DbSet<User> Users { get; set; }
        public DbSet<Participant> Participants { get; set; }
        public DbSet<Organiser> Organisers { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Entry> Entries { get; set; }
        public DbSet<Result> Results { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Unique constraints
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Participant>()
                .HasIndex(p => p.IDNumber)
                .IsUnique();

            modelBuilder.Entity<Participant>()
                .HasIndex(p => p.UserID)
                .IsUnique();

            modelBuilder.Entity<Organiser>()
                .HasIndex(o => o.UserID)
                .IsUnique();

            modelBuilder.Entity<Organiser>()
                .HasIndex(o => o.RegistrationNumber)
                .IsUnique()
                .HasFilter("[RegistrationNumber] IS NOT NULL");

            // One-to-One: User -> Participant
            modelBuilder.Entity<Participant>()
                .HasOne(p => p.User)
                .WithOne(u => u.Participant)
                .HasForeignKey<Participant>(p => p.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            // One-to-One: User -> Organiser
            modelBuilder.Entity<Organiser>()
                .HasOne(o => o.User)
                .WithOne(u => u.Organiser)
                .HasForeignKey<Organiser>(o => o.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            // One-to-Many: Organiser -> Events
            modelBuilder.Entity<Event>()
                .HasOne(e => e.Organiser)
                .WithMany(o => o.Events)
                .HasForeignKey(e => e.OrganiserID)
                .OnDelete(DeleteBehavior.Cascade);

            // One-to-Many: Event -> Categories
            modelBuilder.Entity<Category>()
                .HasOne(c => c.Event)
                .WithMany(e => e.Categories)
                .HasForeignKey(c => c.EventID)
                .OnDelete(DeleteBehavior.Cascade);

            // One-to-Many: Participant -> Entries
            modelBuilder.Entity<Entry>()
                .HasOne(e => e.Participant)
                .WithMany(p => p.Entries)
                .HasForeignKey(e => e.ParticipantID)
                .OnDelete(DeleteBehavior.Cascade);

            // One-to-Many: Event -> Entries
            modelBuilder.Entity<Entry>()
                .HasOne(e => e.Event)
                .WithMany(ev => ev.Entries)
                .HasForeignKey(e => e.EventID)
                .OnDelete(DeleteBehavior.NoAction);

            // One-to-Many: Category -> Entries
            modelBuilder.Entity<Entry>()
                .HasOne(e => e.Category)
                .WithMany(c => c.Entries)
                .HasForeignKey(e => e.CategoryID)
                .OnDelete(DeleteBehavior.NoAction);

            // One-to-One: Entry -> Result
            modelBuilder.Entity<Result>()
                .HasOne(r => r.Entry)
                .WithOne(e => e.Result)
                .HasForeignKey<Result>(r => r.EntryID)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
