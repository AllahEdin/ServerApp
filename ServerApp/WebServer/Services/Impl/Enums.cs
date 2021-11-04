using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace WebServer.Services.Impl
{
    public enum ServerState
    {
        Start,
        WaitForAccept,
        AcceptedSocket,
        StartListen
    }

    public enum ServerTrigger
    {
        StartAccept,
        NewSocketAccepted
    }
}