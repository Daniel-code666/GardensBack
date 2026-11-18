namespace GardensBack.Realtime
{
    /// <summary>
    /// Clase para construir los hubs por grupo
    /// </summary>
    public class TelemetryHubGroups
    {
        /// <summary>
        /// construye la cadena de texto para identificar el hub de microcontrolador
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        public static string ForMicrocontroller(string deviceId)
            => $"telemetry:microcontroller:{deviceId}";
    }
}
