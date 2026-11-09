using GardensBack.Application.DeviceInventory.Abstractions;
using GardensBack.Application.DeviceInventory.Dtos;
using System.Collections.Concurrent;

namespace GardensBack.Application.DeviceInventory.Services
{
    public class DeviceCommandResponseWaiter : IDeviceCommandResponseWaiter
    {
        private readonly ConcurrentDictionary<string, TaskCompletionSource<DeviceActionResultMessage>> _waiters = new();

        public async Task<DeviceActionResultMessage?> WaitAsync(
            string requestId,
            TimeSpan timeout,
            CancellationToken ct)
        {
            var completion = new TaskCompletionSource<DeviceActionResultMessage>(
                TaskCreationOptions.RunContinuationsAsynchronously);

            if (!_waiters.TryAdd(requestId, completion))
                throw new InvalidOperationException("Ya existe una espera para el mismo requestId.");

            using var timeoutSource = new CancellationTokenSource(timeout);
            using var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(ct, timeoutSource.Token);

            try
            {
                return await completion.Task.WaitAsync(linkedSource.Token);
            }
            catch (OperationCanceledException) when (!ct.IsCancellationRequested)
            {
                return null;
            }
            finally
            {
                _waiters.TryRemove(requestId, out _);
            }
        }

        public bool Complete(string requestId, DeviceActionResultMessage response)
            => _waiters.TryGetValue(requestId, out TaskCompletionSource<DeviceActionResultMessage>? completion) &&
               completion.TrySetResult(response);

        public void Cancel(string requestId)
        {
            if (_waiters.TryRemove(requestId, out TaskCompletionSource<DeviceActionResultMessage>? completion))
                completion.TrySetCanceled();
        }
    }
}
