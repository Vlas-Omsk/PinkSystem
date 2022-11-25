using System;
using PinkNet;

namespace BotsCommon.Bots
{
    public interface IApiAdapterFactory
    {
        IApiAdapter Create(NetscapeCookieReader cookieReader, Proxy proxy, string userAgent);
    }
}
