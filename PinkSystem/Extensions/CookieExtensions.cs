using System;
using System.Net;

namespace PinkSystem
{
    public static class CookieExtensions
    {
        public static void MakeExpired(this Cookie self)
        {
            self.Expires = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1));
        }
    }
}
