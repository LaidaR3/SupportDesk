using Microsoft.EntityFrameworkCore;
using SupportDesk.Api.Models;

namespace SupportDesk.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Agent> Agents => Set<Agent>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<Comment> Comments => Set<Comment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Agent>()
            .HasIndex(a => a.Email)
            .IsUnique();

        modelBuilder.Entity<Ticket>()
            .HasIndex(t => t.Reference)
            .IsUnique();

        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.AssignedAgent)
            .WithMany(a => a.Tickets)
            .HasForeignKey(t => t.AssignedAgentId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Comment>()
            .HasOne(c => c.Ticket)
            .WithMany(t => t.Comments)
            .HasForeignKey(c => c.TicketId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}