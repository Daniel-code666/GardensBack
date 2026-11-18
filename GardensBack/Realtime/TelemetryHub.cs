using GardensBack.Application.DeviceInventory.Abstractions;
using GardensBack.Application.DeviceInventory.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace GardensBack.Realtime
{
    /// <summary>
    /// hub de telemetría
    /// </summary>
    /// <param name="deviceInventoryService"></param>
    [Authorize]
    public sealed class TelemetryHub(IDeviceInventoryService deviceInventoryService) : Hub<ITelemetryClient>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="microcontrollerId"></param>
        /// <returns></returns>
        public async Task SubscribeToMicrocontroller(int microcontrollerId)
        {
            int userId = GetAuthenticatedUserId();

            MicrocontrollerWDevicesDto microcontroller = await deviceInventoryService.GetMicroControllerDeviceById(microcontrollerId, userId, Context.ConnectionAborted);

            await Groups.AddToGroupAsync(Context.ConnectionId,TelemetryHubGroups.ForMicrocontroller(microcontroller.DeviceId), Context.ConnectionAborted);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="microcontrollerId"></param>
        /// <returns></returns>
        public async Task UnsubscribeFromMicrocontroller(int microcontrollerId)
        {
            int userId = GetAuthenticatedUserId();

            MicrocontrollerWDevicesDto microcontroller = await deviceInventoryService.GetMicroControllerDeviceById(microcontrollerId, userId, Context.ConnectionAborted);

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, TelemetryHubGroups.ForMicrocontroller(microcontroller.DeviceId), Context.ConnectionAborted);
        }

        private int GetAuthenticatedUserId()
        {
            string? subject = Context.User?.FindFirstValue(JwtRegisteredClaimNames.Sub);

            if (!int.TryParse(subject, out int userId))
                throw new HubException("La identidad del usuario no es válida.");

            return userId;
        }
    }
}
