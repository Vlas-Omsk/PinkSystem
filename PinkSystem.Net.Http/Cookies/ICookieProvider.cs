using System.Collections.Generic;
using System.Net;
using PinkSystem.IO.Content;

namespace PinkSystem.Net.Http.Cookies
{
    public interface ICookieProvider
    {
        bool IsSupported(IContentReader reader);
        IEnumerable<Cookie> ReadAll(IContentReader reader);
    }
}
