using GardensBack.Application.DeviceProvisioning.Abstractions;
using GardensBack.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace GardensBack.Infrastructure.Persistance.Repositories
{
    using Microsoft.EntityFrameworkCore;

    public class DeviceProvisioningRepository(ApplicationDbContext db) : IDeviceProvisioningRepository
    {
        private readonly ApplicationDbContext _db = db;

        public Task AddDevice(MicrocontrollerDevice device, CancellationToken ct)
        {
            return _db.MicrocontrollerDevices.AddAsync(device, ct).AsTask();
        }

        public Task AddMqttCredential(MqttCredential credential, CancellationToken ct)
        {
            return _db.MqttCredentials.AddAsync(credential, ct).AsTask();
        }

        public Task<MicrocontrollerDevice?> FindByDeviceId(string deviceId, CancellationToken ct)
        {
            return _db.MicrocontrollerDevices
                .Include(x => x.User)
                .Include(x => x.DeviceClaim)
                .SingleOrDefaultAsync(x => x.DeviceId == deviceId, ct);
        }

        public async Task<IReadOnlyList<MicrocontrollerDevice>> FindIdentityMatches(string deviceId,string hardwareId,string mqttClientId,CancellationToken ct)
        {
            return await _db.MicrocontrollerDevices
                .Include(x => x.User)
                .Include(x => x.DeviceClaim)
                .Where(x => x.DeviceId == deviceId ||
                            x.HardwareId == hardwareId ||
                            x.MqttClientId == mqttClientId)
                .ToListAsync(ct);
        }

        public Task<DeviceClaim?> FindClaimByBootstrapRequestId(string bootstrapRequestId, CancellationToken ct)
        {
            return _db.DeviceClaims
                .Include(x => x.MicroCtrlDevice)
                .ThenInclude(x => x.User)
                .SingleOrDefaultAsync(x => x.BootstrapRequestId == bootstrapRequestId, ct);
        }

        public Task<DeviceClaim?> FindClaimByRequestId(string requestId, CancellationToken ct)
        {
            return _db.DeviceClaims
                .Include(x => x.MicroCtrlDevice)
                .ThenInclude(x => x.User)
                .SingleOrDefaultAsync(x => x.RequestId == requestId, ct);
        }

        public Task<DeviceClaim?> FindClaimByTokenHash(string tokenHash, CancellationToken ct)
        {
            return _db.DeviceClaims
                .Include(x => x.MicroCtrlDevice)
                .ThenInclude(x => x.User)
                .Include(x => x.MicroCtrlDevice)
                .ThenInclude(x => x.MqttCredential)
                .SingleOrDefaultAsync(x => x.ProvisioningTokenHash == tokenHash, ct);
        }

        public Task<int> SaveChanges(CancellationToken ct)
        {
            return _db.SaveChangesAsync(ct);
        }
    }
}
