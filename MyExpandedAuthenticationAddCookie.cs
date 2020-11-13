using System;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using System.Text.Encodings.Web;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Net.Http.Headers;


namespace blazor_auth_individual_experiment
{
    // A simple wrapper around `CookieAuthenticationHandler` that allows
    // us to trap and inspect every steps of the authentication process
    // so that we better understand what's occuring.
    // For the actual implementation, see:
    //  -  `aspnetcore/src/Security/Authentication/Cookies/src/CookieAuthenticationHandler.cs`
    //  -  <https://github.com/dotnet/aspnetcore/blob/master/src/Security/Authentication/Cookies/src/CookieAuthenticationHandler.cs>.
    public class MyCookieAuthenticationHandler : CookieAuthenticationHandler
    {
        public MyCookieAuthenticationHandler(IOptionsMonitor<CookieAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
         :  base(options, logger, encoder, clock)
        {
        }

        protected override Task InitializeHandlerAsync()
        {
            var authScheme = Scheme.Name; // Always `"Identity.Application"`.
            var cookieOptions = Options.Cookie.Build(Context);
            // On every page load.
            return base.InitializeHandlerAsync();
        }

        protected override async Task FinishResponseAsync()
        {
            await base.FinishResponseAsync();

            // Called `Options.CookieManager.AppendResponseCookie(
            //     Context,
            //     Options.Cookie.Name,
            //     cookieValue,
            //     signInContext.CookieOptions
            // )`.

            // Called `base.ApplyHeaders`.
            //  -> `base.Response.Headers[HeaderNames.CacheControl] = HeaderValueNoCache`;
            //  -> `base.Response.Headers[HeaderNames.Pragma] = HeaderValueNoCache`;
            //  -> `base.Response.Headers[HeaderNames.Expires] = HeaderValueEpocDate`;
            //  When `properties.RedirectUri` -> `Events.RedirectToReturnUrl`

            var responseHeaders = Response.Headers;
            var requestQuery = Request.Query;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var authScheme = Scheme.Name; // Always `"Identity.Application"`.
            var cookieOptions = Options.Cookie.Build(Context);
            // `cookie == null` when not logged in.
            var cookie = Options.CookieManager.GetRequestCookie(Context, Options.Cookie.Name);

            var responseHeaders = Response.Headers;
            // Both these headers are always empty.
            var responseHeaderSetCookie =  responseHeaders[HeaderNames.SetCookie];
            var responseHeaderCookie =  responseHeaders[HeaderNames.Cookie];

            var requestHeaders = Request.Headers;
            // The `HeaderNames.SetCookie` is always empty.
            var requestHeaderSetCookie =  requestHeaders[HeaderNames.SetCookie];
            // The `HeaderNames.Cookie` is non empty when user is logged-in.
            var requestHeaderCookie =  requestHeaders[HeaderNames.Cookie];

            var requestQuery = Request.Query;

            // On every page load, we need to know whether we're authentitcated.
            var authResult = await base.HandleAuthenticateAsync();

            var ticket = authResult.Ticket;
            var changedRequestQuery = Request.Query;

            return authResult;
        }

        protected async override Task HandleSignInAsync(ClaimsPrincipal user, AuthenticationProperties properties)
        {
            var authScheme = Scheme.Name; // Always `"Identity.Application"`.
            var cookieOptions = Options.Cookie.Build(Context);
            var cookie = Options.CookieManager.GetRequestCookie(Context, Options.Cookie.Name);
            var responseHeaders = Response.Headers;
            var requestQuery = Request.Query;


            // During the call to `SignInManager<IdentityUser>::PasswordSignInAsync`
            // or `SignInManager<IdentityUser>::SignInAsync`.
            await base.HandleSignInAsync(user, properties);

            // `ticket` is retrieved from `EnsureCookieTicket`
            //   -> `cookie = base.Options.CookieManager.GetRequestCookie(Context, Options.Cookie.Name);`
            //   -> `ticket = base.Options.TicketDataFormat.Unprotect(cookie, GetTlsTokenBinding());`
            //   When `base.Options.SessionStore`
            //      -> ticket = await Options.SessionStore.RetrieveAsync(_sessionKey);

            // `ticket` created
            //   -> `ticket = new AuthenticationTicket(signInContext.Principal, signInContext.Properties, signInContext.Scheme.Name);`
            //   When `base.Options.SessionStore`
            //      -> `base.Options.SessionStore.StoreAsync(ticket);`
            //      -> `ticket` recreated with `SessionIdClaim`
            //          -> `ticket = new AuthenticationTicket(principal, null, Scheme.Name)`
            // `cookieValue = base.Options.TicketDataFormat.Protect(ticket, GetTlsTokenBinding());`

            // Called `base.Options.CookieManager.AppendResponseCookie(
            //     Context,
            //     Options.Cookie.Name,
            //     cookieValue,
            //     signInContext.CookieOptions
            // )`.

            // Called `base.ApplyHeaders`.
            //  -> `base.Response.Headers[HeaderNames.CacheControl] = HeaderValueNoCache`;
            //  -> `base.Response.Headers[HeaderNames.Pragma] = HeaderValueNoCache`;
            //  -> `base.Response.Headers[HeaderNames.Expires] = HeaderValueEpocDate`;
            //  When `properties.RedirectUri` -> `Events.RedirectToReturnUrl`
            //  (requires have `base.Options.LoginPath` set).

            // Called `base.Logger.AuthenticationSchemeSignedIn(base.Scheme.Name);`

            // `changedCookie == null`.
            var changedCookie = Options.CookieManager.GetRequestCookie(Context, Options.Cookie.Name);
            var changedResponseHeaders = Response.Headers;

            // Cookie is stored in the `HeaderNames.SetCookie` header.
            var changedResponseHeaderSetCookie =  changedResponseHeaders[HeaderNames.SetCookie];
            // The `HeaderNames.Cookie` header is empty.
            var changedResponseHeaderCookie =  changedResponseHeaders[HeaderNames.Cookie];

            var changedRequestQuery = Request.Query;
        }

        protected async override Task HandleSignOutAsync(AuthenticationProperties properties)
        {
            var authScheme = Scheme.Name; // Always `"Identity.Application"`.
            var cookieOptions = Options.Cookie.Build(Context);

            var responseHeaders = Response.Headers;
            var requestQuery = Request.Query;

            // During the call to `SignInManager<IdentityUser>::SignOutAsync`.
            await base.HandleSignOutAsync(properties);

            // When `Options.SessionStore`.
            //  ->  `await Options.SessionStore.RemoveAsync(_sessionKey);`

            // Called `base.Events.SigningOut(context);`

            // Called `base.Options.CookieManager.DeleteCookie(
            //     Context,
            //     Options.Cookie.Name,
            //     context.CookieOptions
            // )`;

            // Called `base.ApplyHeaders`.
            //  -> `base.Response.Headers[HeaderNames.CacheControl] = HeaderValueNoCache`;
            //  -> `base.Response.Headers[HeaderNames.Pragma] = HeaderValueNoCache`;
            //  -> `base.Response.Headers[HeaderNames.Expires] = HeaderValueEpocDate`;
            //  When `properties.RedirectUri` -> `Events.RedirectToReturnUrl`
            //  (requires have `base.Options.LogoutPath` set).

            // Called `base.Logger.AuthenticationSchemeSignedOut(base.Scheme.Name);`

            var changedCookie = Options.CookieManager.GetRequestCookie(Context, Options.Cookie.Name);
            var changedResponseHeaders = Response.Headers;
            var changedRequestQuery = Request.Query;
        }

        protected override async Task HandleForbiddenAsync(AuthenticationProperties properties)
        {
            var authScheme = Scheme.Name;
            var cookieOptions = Options.Cookie.Build(Context);
            await base.HandleForbiddenAsync(properties);
            //  -> Builds an uri with `Options.AccessDeniedPath` and `properties.RedirectUri`.
            //  -> Redirect to this uri through `base.Events.RedirectToAccessDenied`.
        }

        protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            var authScheme = Scheme.Name;
            var cookieOptions = Options.Cookie.Build(Context);
            await base.HandleChallengeAsync(properties);
            //  -> Builds an uri with `base.Options.LoginPath` and `properties.RedirectUri`.
            //  -> Redirect to this uri through `base.Events.RedirectToLogin`.
        }

    }

    static class MyExpandedAuthenticationAddCookie
    {
        // NOTE: jrg: Expansion of `CookieExtensions.AddCookie`.
        public static AuthenticationBuilder AddMyCookieAuthenticationHandler(this AuthenticationBuilder builder, string authenticationScheme, Action<CookieAuthenticationOptions> configureOptions)
        {
            string displayName = null;
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IPostConfigureOptions<CookieAuthenticationOptions>, PostConfigureCookieAuthenticationOptions>());
            builder.Services.AddOptions<CookieAuthenticationOptions>(authenticationScheme).Validate(o => o.Cookie.Expiration == null, "Cookie.Expiration is ignored, use ExpireTimeSpan instead.");
            return builder.AddScheme<CookieAuthenticationOptions, MyCookieAuthenticationHandler>(authenticationScheme, displayName, configureOptions);
        }
    }
}