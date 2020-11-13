using System;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace blazor_auth_individual_experiment
{

    static class MyExpandedAuthenticationAddCookie
    {
        // NOTE: jrg: Expansion of `CookieExtensions.AddCookie`.
        public static AuthenticationBuilder AddMyCookieAuthenticationHandler(this AuthenticationBuilder builder, string authenticationScheme, Action<CookieAuthenticationOptions> configureOptions)
        {
            string displayName = null;
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IPostConfigureOptions<CookieAuthenticationOptions>, PostConfigureCookieAuthenticationOptions>());
            builder.Services.AddOptions<CookieAuthenticationOptions>(authenticationScheme).Validate(o => o.Cookie.Expiration == null, "Cookie.Expiration is ignored, use ExpireTimeSpan instead.");
            return builder.AddScheme<CookieAuthenticationOptions, CookieAuthenticationHandler>(authenticationScheme, displayName, configureOptions);
        }
    }
}