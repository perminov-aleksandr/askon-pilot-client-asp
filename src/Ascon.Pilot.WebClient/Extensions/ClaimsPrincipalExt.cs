using System;
using System.Net;
using System.Security.Claims;

namespace Ascon.Pilot.WebClient.Extensions
{
    public static class ClaimsPrincipalExt
    {
        public static CookieContainer GetCookieContainer(this ClaimsPrincipal principal, Uri baseAddress)
        {
            var cookieContainer = new CookieContainer();
            if (principal.HasClaim(x => x.Type == ClaimTypes.Sid))
            {
                var sidString = principal.FindFirst(x => x.Type == ClaimTypes.Sid).Value;
                cookieContainer.SetCookies(baseAddress, sidString);
                return cookieContainer;
            }
            else
            {
                throw new ArgumentException("There is no SID set at current ClaimsPrincipal");
            }
        }
    }
}