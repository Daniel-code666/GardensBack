using GardensBack.Application.DeviceInventory.Dtos;

namespace GardensBack.Realtime
{
    /// <summary>
    /// Interfaz para la conexión desde el front
    /// </summary>
    public interface ITelemetryClient
    {
        /// <summary>
        /// Contrato del cliente
        /// </summary>
        /// <param name="telemetry"></param>
        /// <returns></returns>
        Task TelemetryReceived(TelemetryRealtimeDto telemetry);
    }
}
