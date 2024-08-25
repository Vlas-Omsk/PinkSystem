using PinkSystem.IO.Content;
using System.Collections.Generic;
using System.Net;

namespace PinkSystem.Net.Http.Cookies
{
    public interface ICookieProvider
    {
        bool IsSupported(IContentReader reader);
        IEnumerable<Cookie> ReadAll(IContentReader reader);
    }
}
