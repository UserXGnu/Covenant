// Author: Ryan Cobb (@cobbr_io)
// Project: EasyPeasy (https://github.com/cobbr/EasyPeasy)
// License: GNU GPLv3

using System.Threading.Tasks;

using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

using EasyPeasy.Core;

namespace EasyPeasy.Hubs
{
    [Authorize]
    public class EventHub : Hub
    {
        public async Task JoinGroup(string context)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, context);
        }
    }
}
