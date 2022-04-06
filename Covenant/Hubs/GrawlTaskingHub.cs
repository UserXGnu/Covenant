// Author: Ryan Cobb (@cobbr_io)
// Project: EasyPeasy (https://github.com/cobbr/EasyPeasy)
// License: GNU GPLv3

using System.Threading.Tasks;

using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace EasyPeasy.Hubs
{
    [Authorize]
    public class GrawlTaskingHub : Hub
    {
        public async Task JoinGroup(string context)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, context);
        }
    }
}
