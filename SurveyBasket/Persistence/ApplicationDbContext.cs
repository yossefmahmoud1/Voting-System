using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace SurveyBasket.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options , IHttpContextAccessor httpContextAccessor) : IdentityDbContext<Application_User>(options)
{
    private readonly DbContextOptions<ApplicationDbContext> options = options;
    private readonly IHttpContextAccessor httpContextAccessor = httpContextAccessor;

    public DbSet<Poll> Polls { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        modelBuilder.Entity<Application_User>()
            .HasIndex(u => u.UserName)
            .IsUnique();

        modelBuilder.Entity<Application_User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Application_User>()
           .OwnsMany(x => x.refreshTokens)
           .ToTable("RefreshToken")
           .WithOwner()
           .HasForeignKey("Application_UserId");





        base.OnModelCreating(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<AuditableEntity>();


        foreach (var entry in entries)
        {
         var CurrentUserid=   httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier)!;
            if (entry.State == EntityState.Added)
            {
                entry.Property( x => x.CreatedById).CurrentValue = CurrentUserid;
                entry.Property( x => x.UpdatedById).CurrentValue = null;
                entry.Entity.CreatedAt = DateTime.UtcNow;
                entry.Entity.UpdatedAt = null;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Property( x => x.UpdatedById).CurrentValue = CurrentUserid;
                entry.Property( x => x.UpdatedAt).CurrentValue = DateTime.UtcNow;

            }


          
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}