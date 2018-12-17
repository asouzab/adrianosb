using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Jwt;
using Mosaic.MOL.API.Web.Helpers;
using Owin;

[assembly: OwinStartup(typeof(Mosaic.MOL.API.Web.App_Start.Startup))]

namespace Mosaic.MOL.API.Web.App_Start
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=316888

            app.UseJwtBearerAuthentication(
                new JwtBearerAuthenticationOptions
                {
                    AuthenticationMode = AuthenticationMode.Active,
                    TokenValidationParameters = new TokenValidationParameters()
                    {
                        RequireExpirationTime = true,
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        //ValidAudience = ConfigHelper.GetAudience(),
                        //ValidIssuer = ConfigHelper.GetIssuer(),
                        IssuerSigningKey = ConfigHelper.GetSymmetricSecurityKey(),
                        ValidateLifetime = true
                        //ValidateIssuerSigningKey = true
                    }
                }
            );
        }
    }
}
