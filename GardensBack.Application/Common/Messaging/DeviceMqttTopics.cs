namespace GardensBack.Application.Common.Messaging
{
    /// <summary>
    /// Define los segmentos y construye los tópicos MQTT utilizados por los dispositivos.
    /// </summary>
    public static class DeviceMqttTopics
    {
        public const string Root = "gardens";
        public const string SingleLevelWildcard = "+";

        public const string Telemetry = "telemetry";
        public const string Actions = "actions";
        public const string ActionsResponse = "actions-response";
        public const string Inventory = "inventory";
        public const string Control = "control";
        public const string ControlStatus = "control-status";

        public const string TelemetrySubscription = Root + "/" + SingleLevelWildcard + "/" + Telemetry;
        public const string ActionsSubscription = Root + "/" + SingleLevelWildcard + "/" + Actions;
        public const string ActionsResponseSubscription = Root + "/" + SingleLevelWildcard + "/" + ActionsResponse;
        public const string InventorySubscription = Root + "/" + SingleLevelWildcard + "/" + Inventory;
        public const string ControlSubscription = Root + "/" + SingleLevelWildcard + "/" + Control;
        public const string ControlStatusSubscription = Root + "/" + SingleLevelWildcard + "/" + ControlStatus;

        public static string BuildTelemetry(string deviceId) => Build(deviceId, Telemetry);

        public static string BuildActions(string deviceId) => Build(deviceId, Actions);

        public static string BuildActionsResponse(string deviceId) => Build(deviceId, ActionsResponse);

        public static string BuildInventory(string deviceId) => Build(deviceId, Inventory);

        public static string BuildControl(string deviceId) => Build(deviceId, Control);

        public static string BuildControlStatus(string deviceId) => Build(deviceId, ControlStatus);

        public static bool IsValidDeviceId(string? deviceId)
        {
            return !string.IsNullOrWhiteSpace(deviceId) &&
                   deviceId.Length <= 64 &&
                   deviceId.All(character =>
                       char.IsAsciiLetterOrDigit(character) ||
                       character == '-' ||
                       character == '_');
        }

        private static string Build(string deviceId, string topic)
        {
            if (!IsValidDeviceId(deviceId))
                throw new ArgumentException("El identificador del dispositivo no es válido.", nameof(deviceId));

            return $"{Root}/{deviceId}/{topic}";
        }
    }
}
