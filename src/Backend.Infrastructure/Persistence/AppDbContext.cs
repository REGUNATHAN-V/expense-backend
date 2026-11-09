using Microsoft.EntityFrameworkCore;
using Backend.Application.Interfaces.Services;
using Backend.Domain.Entities;

namespace Backend.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    private readonly ICurrentUserService _currentUser;

    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        ICurrentUserService currentUser) : base(options)
    {
        _currentUser = currentUser;
    }

    public DbSet<UserEntity> Users => Set<UserEntity>();
    public DbSet<CategoryEntity> Categories => Set<CategoryEntity>();
    public DbSet<LeaveTypeEntity> LeaveTypes => Set<LeaveTypeEntity>();


    public DbSet<ClaimEntity> Claims => Set<ClaimEntity>();
    public DbSet<ClaimItemEntity> ClaimItems => Set<ClaimItemEntity>();


    public override async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.UserId;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
                entry.Entity.CreatedBy = userId;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
                entry.Entity.UpdatedBy = userId;

                entry.Property(e => e.CreatedAt).IsModified = false;
                entry.Property(e => e.CreatedBy).IsModified = false;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(AppDbContext).Assembly);

        modelBuilder.Entity<ClaimEntity>()
            .HasMany(claim => claim.Items)
            .WithOne()
            .HasForeignKey(item => item.ClaimId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}