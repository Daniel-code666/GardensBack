using GardensBack.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GardensBack.Infrastructure.Persistance
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
        // registro de entidades
        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<MicrocontrollerDevice> MicrocontrollerDevices => Set<MicrocontrollerDevice>();
        public DbSet<Device> Devices => Set<Device>();
        public DbSet<DeviceTelemetry> DeviceTelemetries => Set<DeviceTelemetry>();
        public DbSet<DeviceCommand> DeviceCommands => Set<DeviceCommand>();
        public DbSet<DeviceClaim> DeviceClaims => Set<DeviceClaim>();
        public DbSet<MqttCredential> MqttCredentials => Set<MqttCredential>();
        public DbSet<FirmwareRelease> FirmwareReleases => Set<FirmwareRelease>();
        public DbSet<DeviceFirmwareUpdate> FirmwareUpdates => Set<DeviceFirmwareUpdate>();
        public DbSet<DeviceConnectivityConfiguration> DeviceConnectivityConfigurations => Set<DeviceConnectivityConfiguration>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }

        // sobrescritura para datos de auditoría
        private void ApplyAudit()
        {
            var now = DateTime.UtcNow;

            foreach(var entry in ChangeTracker.Entries<BaseEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Property(x => x.Active).CurrentValue = true;
                    entry.Property(x => x.CreationDate).CurrentValue = now;
                    entry.Property(x => x.UpdateDate).CurrentValue = null;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Property(x => x.CreationDate).IsModified = false;
                    entry.Property(x => x.UpdateDate).CurrentValue = now;
                }
            }
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            ApplyAudit();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            ApplyAudit();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
    }
}
