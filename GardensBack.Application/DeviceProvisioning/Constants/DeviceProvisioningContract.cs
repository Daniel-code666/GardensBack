namespace GardensBack.Application.DeviceProvisioning.Constants
{
    /// <summary>
    /// Centraliza los valores y límites del contrato de aprovisionamiento implementado por el ESP.
    /// </summary>
    public static class DeviceProvisioningContract
    {
        public const int Version = 1;
        public const string ClaimRegistrationType = "claim_registration";
        public const string ClaimResponseType = "claim_response";
        public const string MqttCredentialsRegistrationType = "mqtt_credentials_registration";
        public const string MqttCredentialsEnvelopeType = "mqtt_credentials_envelope";
        public const string DeviceConfigurationResultType = "device_configuration_result";
        public const string PendingStatus = "pending";

        public const string IdentifierPattern = @"^[A-Za-z0-9_.:-]+$";
        public const string HardwareIdPattern = @"^[0-9a-f]{12}$";
        public const string RequestIdPattern = @"^[0-9a-f]{32}$";
        public const string ClaimCodePattern = @"^[A-Z2-9]{12}$";
        public const string HexadecimalKeyPattern = @"^[0-9a-f]{64}$";
        public const string NoncePattern = @"^[0-9a-f]{24}$";
        public const string AuthenticationTagPattern = @"^[0-9a-f]{32}$";
        public const string HmacPattern = @"^[0-9a-f]{64}$";
        public const string CiphertextPattern = @"^(?:[0-9a-f]{2})+$";
        public const string ErrorCodePattern = @"^[a-z0-9_]+$";
        public const string FirmwareVersionPattern = @"^\d+\.\d+\.\d+(?:[-+][A-Za-z0-9.-]+)?$";

        public const int MaximumIdentifierLength = 128;
        public const int MaximumBootstrapRequestIdLength = 64;
        public const int MaximumErrorCodeLength = 64;
        public const int MaximumClaimLifetimeSeconds = 300;
        public const int MaximumBodySizeBytes = 8192;
    }
}
