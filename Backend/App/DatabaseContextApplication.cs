using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Gemeinschaftsgipfel.Models;

public class DatabaseContextApplication(DbContextOptions<DatabaseContextApplication> options)
    : IdentityDbContext<User>(options)
{
    public DbSet<Topic> Topics => Set<Topic>();
    public DbSet<Vote> Votes => Set<Vote>();
    public DbSet<SupportTask> SupportTasks => Set<SupportTask>();
    public DbSet<SupportPromise> SupportPromises => Set<SupportPromise>();
    public DbSet<TopicComment> TopicComments => Set<TopicComment>();
    public DbSet<RideShare> RideShares => Set<RideShare>();
    public DbSet<RideShareReservation> RideShareReservations => Set<RideShareReservation>();
    public DbSet<RideShareComment> RideShareComments => Set<RideShareComment>();
    public DbSet<ForumEntry> ForumEntries => Set<ForumEntry>();
    public DbSet<ForumComment> ForumComments => Set<ForumComment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // RideShare cascade deletes
        modelBuilder.Entity<RideShareComment>()
            .HasOne(c => c.RideShare)
            .WithMany(r => r.Comments)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RideShareReservation>()
            .HasOne(r => r.RideShare)
            .WithMany(rs => rs.Reservations)
            .OnDelete(DeleteBehavior.Cascade);

        // ForumEntry cascade deletes
        modelBuilder.Entity<ForumComment>()
            .HasOne(c => c.ForumEntry)
            .WithMany(f => f.Comments)
            .OnDelete(DeleteBehavior.Cascade);

        // Topic cascade deletes
        modelBuilder.Entity<TopicComment>()
            .HasOne(c => c.Topic)
            .WithMany(t => t.Comments)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Vote>()
            .HasOne(v => v.Topic)
            .WithMany(t => t.Votes)
            .OnDelete(DeleteBehavior.Cascade);

        // SupportTask cascade deletes
        modelBuilder.Entity<SupportPromise>()
            .HasOne(p => p.SupportTask)
            .WithMany(st => st.SupportPromises)
            .OnDelete(DeleteBehavior.Cascade);
    }
}