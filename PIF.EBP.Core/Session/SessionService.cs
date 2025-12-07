using System;
using System.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Web;
using PIF.EBP.Core.Shared;
using Newtonsoft.Json.Linq;

namespace PIF.EBP.Core.Session
{
    public class SessionService : ISessionService
    {
        private readonly JwtSecurityTokenHandler tokenHandler;
        private SessionUser user;

        public SessionService()
        {
            tokenHandler = new JwtSecurityTokenHandler();
            user = new SessionUser();

            var requestHeaders = HttpContext.Current.Request.Headers;

            if (requestHeaders.AllKeys.Any(s => s.Equals("Authorization", StringComparison.OrdinalIgnoreCase)))
            {
                if (!string.IsNullOrEmpty(requestHeaders.GetValues("Authorization").FirstOrDefault()))
                {
                    var authHeader = requestHeaders.GetValues("Authorization").FirstOrDefault();

                    if (authHeader != null)
                    {
                        var jwtToken = authHeader.Split(' ').Last();

                        var jwtTokenObj = tokenHandler.ReadJwtToken(jwtToken);

                        user.ContactId = jwtTokenObj.Claims.FirstOrDefault(c => c.Type == TokenClaimsKeys.ContactId)?.Value;
                        user.CompanyId = jwtTokenObj.Claims.FirstOrDefault(c => c.Type == TokenClaimsKeys.CompanyId)?.Value;
                        user.RoleId = jwtTokenObj.Claims.FirstOrDefault(c => c.Type == TokenClaimsKeys.RoleId)?.Value;
                        user.Email = jwtTokenObj.Claims.FirstOrDefault(c => c.Type == TokenClaimsKeys.Email)?.Value;
                        user.Token =jwtToken;
                        user.IsResetToken = jwtTokenObj.Claims.Any(c => c.Type == TokenClaimsKeys.Reset);
                    }
                }
            }
        }

        public SessionUser GetCurrentUser()
        {
            return user;
        }

        public string GetContactId()
        {
            return user?.ContactId;
        }

        public string GetCompanyId()
        {
            return user?.CompanyId;
        }

        public string GetRoleId()
        {
            return user?.RoleId;
        }
        public string GetEmail()
        {
            return user?.Email;
        }
        public string GetToken()
        {
            return user?.Token;
        }
        public bool IsResetToken()
        {
            return user.IsResetToken;
        }

        public string GetLanguage()
        {
            var requestHeaders = HttpContext.Current.Request.Headers;

            if (requestHeaders.AllKeys.Any(s => s.Equals("lang", StringComparison.OrdinalIgnoreCase)))
            {
                var langheader = requestHeaders.GetValues("langheader").FirstOrDefault();

                return langheader;
            }
            else
            {
                return null;
            }
                
        }
    }
}
