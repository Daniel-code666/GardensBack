using GardensBack.Application.Common.Exceptions;
using GardensBack.Application.FirmwareUpdates.Abstractions;
using GardensBack.Application.FirmwareUpdates.Dtos;
using GardensBack.Domain.Entities;
using GardensBack.Domain.Enums;

namespace GardensBack.Application.FirmwareUpdates.Services
{
    public class DeviceFirmwareUpdateStatusService(IDeviceFirmwareUpdateRepository repository) : IDeviceFirmwareUpdateStatusService
    {
        private readonly IDeviceFirmwareUpdateRepository _repository = repository;

        public async Task ProcessStatus(string topicDeviceId, OtaUpdateStatusMessage message, CancellationToken ct)
        {
            message.Validate();

            if (!string.Equals(topicDeviceId, message.DeviceId, StringComparison.Ordinal))
                throw new ValidationException("El dispositivo del tópico no coincide con el payload OTA.");

            DeviceFirmwareUpdate update = await _repository.FindByRequest(topicDeviceId, message.RequestId, ct)
                ?? throw new NotFoundException("El intento de actualización de firmware no fue encontrado.");

            ValidateIdentity(update, message);

            if (update.Status is FirmwareUpdateStatus.Confirmed or FirmwareUpdateStatus.Failed)
                return;

            DateTime now = DateTime.UtcNow;
            update.ProgressPercentage = Math.Max(update.ProgressPercentage ?? 0, message.ProgressPercent);

            if (message.Status == FirmwareUpdateStatus.Failed)
            {
                update.Status = FirmwareUpdateStatus.Failed;
                update.CompletedAt = now;
                update.FailureCode = message.FailureCode;
                update.FailureMessage = null;
                await _repository.SaveChanges(ct);
                return;
            }

            if (message.Status == FirmwareUpdateStatus.Confirmed)
            {
                if (!string.Equals(message.CurrentVersion, update.FirmwareRelease.Version, StringComparison.Ordinal))
                    throw new ValidationException("El ESP confirmó una versión diferente a la publicación solicitada.");

                update.StartedAt ??= now;
                update.PendingValidationAt ??= now;
                update.DeviceConfirmedAt ??= now;
                update.Status = FirmwareUpdateStatus.PendingValidation;
                update.ProgressPercentage = 100;
                await _repository.SaveChanges(ct);
                await _repository.CompleteConfirmedIfReady(update.Id, now, ct);
                return;
            }

            if (update.DeviceConfirmedAt is not null || GetRank(message.Status) < GetRank(update.Status))
                return;

            switch (message.Status)
            {
                case FirmwareUpdateStatus.Pending:
                    update.Status = FirmwareUpdateStatus.Pending;
                    break;

                case FirmwareUpdateStatus.Downloading:
                    update.Status = FirmwareUpdateStatus.Downloading;
                    update.StartedAt ??= now;
                    break;

                case FirmwareUpdateStatus.PendingValidation:
                    update.Status = FirmwareUpdateStatus.PendingValidation;
                    update.StartedAt ??= now;
                    update.PendingValidationAt ??= now;
                    break;

                default:
                    throw new ValidationException("La transición de estado OTA no es válida.");
            }

            await _repository.SaveChanges(ct);
        }

        private static void ValidateIdentity(DeviceFirmwareUpdate update, OtaUpdateStatusMessage message)
        {
            if (!update.Active)
                throw new ValidationException("El intento de actualización no está activo.");

            if (!string.Equals(update.FirmwareRelease.ReleaseId, message.ReleaseId, StringComparison.Ordinal))
                throw new ValidationException("El ReleaseId del estado no coincide con el intento registrado.");

            if (!string.IsNullOrEmpty(message.TargetVersion) && !string.Equals(update.FirmwareRelease.Version, message.TargetVersion, StringComparison.Ordinal))
                throw new ValidationException("La versión objetivo del estado no coincide con la publicación registrada.");
        }

        private static int GetRank(FirmwareUpdateStatus status)
            => status switch
            {
                FirmwareUpdateStatus.Pending => 0,
                FirmwareUpdateStatus.Downloading => 1,
                FirmwareUpdateStatus.PendingValidation => 2,
                FirmwareUpdateStatus.Confirmed => 3,
                FirmwareUpdateStatus.Failed => 4,
                _ => throw new ValidationException("El estado OTA no es válido.")
            };

    }
}
