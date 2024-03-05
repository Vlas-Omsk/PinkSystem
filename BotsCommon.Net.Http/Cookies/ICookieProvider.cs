using BotsCommon.IO.Content;
using System.Collections.Generic;
using System.Net;

namespace BotsCommon.Net.Http.Cookies
{
    public interface ICookieProvider
    {
        bool IsSupported(IContentReader reader);
        IEnumerable<Cookie> ReadAll(IContentReader reader);
    }
}
