using System;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using blazor_auth_individual_experiment.Areas.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace blazor_auth_individual_experiment
{
    static class MyExpandedIdentityReplicaServiceCollectionExtensions
    {
        public static IServiceCollection AddMyTokenProvider<TProvider, TUser>(
                this IServiceCollection services, string providerName)
            where TUser : class
            where TProvider : class, IUserTwoFactorTokenProvider<TUser>
        {
            services.Configure<IdentityOptions>(options =>
            {
                options.Tokens.ProviderMap[providerName] = new TokenProviderDescriptor(typeof(TProvider));
            });
            services.AddTransient<TProvider>();

            return services;
        }

        public static IServiceCollection AddMyExpandedIdentityReplica<TUser, TRole, TContext, TKey>(
                this IServiceCollection services)
            where TUser : IdentityUser<TKey>
            where TRole : IdentityRole<TKey>
            where TContext : DbContext
            where TKey : IEquatable<TKey>
        {

            // NOTE: jrg: Expand.
            // services.AddDefaultIdentity<TUser>(options => options.SignIn.RequireConfirmedAccount = true)
            //    .AddEntityFrameworkStores<TContext>();

            // NOTE: jrg: Expand.
            // services.AddAuthentication(o =>
            // {
            //     o.DefaultScheme = IdentityConstants.ApplicationScheme;
            //     o.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            // })

            // NOTE: jrg: Expand.
            // services.AddAuthenticationCore();
            services.TryAddScoped<IAuthenticationService, AuthenticationService>();
            services.TryAddSingleton<IClaimsTransformation, NoopClaimsTransformation>(); // Can be replaced with scoped ones that use DbContext
            services.TryAddScoped<IAuthenticationHandlerProvider, AuthenticationHandlerProvider>();
            services.TryAddSingleton<IAuthenticationSchemeProvider, AuthenticationSchemeProvider>();
            // NOTE: jrg end `AddAuthenticationCore` expansion.

            services.AddDataProtection();
            services.AddWebEncoders();
            services.TryAddSingleton<ISystemClock, SystemClock>();
            services.Configure<AuthenticationOptions>(o =>
            {
                o.DefaultScheme = IdentityConstants.ApplicationScheme;
                o.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            });

            var authBuilder = new AuthenticationBuilder(services);
            // NOTE: jrg: end `AddAuthentication` expansion.

            authBuilder
            // NOTE: jrg: Expand.
            // .AddIdentityCookies(o => { });
                // "Identity.Application"
                .AddMyCookieAuthenticationHandler(IdentityConstants.ApplicationScheme, o =>
                {
                    o.LoginPath = new PathString("/Account/Login");
                    o.Events = new CookieAuthenticationEvents
                    {
                        OnValidatePrincipal = SecurityStampValidator.ValidatePrincipalAsync
                    };
                })
                // "Identity.External"
                .AddMyCookieAuthenticationHandler(IdentityConstants.ExternalScheme, o =>
                {
                    o.Cookie.Name = IdentityConstants.ExternalScheme;
                    o.ExpireTimeSpan = TimeSpan.FromMinutes(5);
                })
                // "Identity.TwoFactorRememberMe"
                .AddMyCookieAuthenticationHandler(IdentityConstants.TwoFactorRememberMeScheme, o =>
                {
                    o.Cookie.Name = IdentityConstants.TwoFactorRememberMeScheme;
                })
                // "Identity.TwoFactorUserId"
                .AddMyCookieAuthenticationHandler(IdentityConstants.TwoFactorUserIdScheme, o =>
                {
                    o.Cookie.Name = IdentityConstants.TwoFactorUserIdScheme;
                    o.ExpireTimeSpan = TimeSpan.FromMinutes(5);
                });
            // NOTE: jrg: End `AddIdentityCookies` expansion.

            // NOTE: jrg: Expand
            // var identityBuilder = services.AddIdentityCore<TUser>(o =>
            // {
            //     o.Stores.MaxLengthForKeys = 128;
            //     o.SignIn.RequireConfirmedAccount = true;
            // });
            // Services identity depends on
            services.AddOptions().AddLogging();

            // Services used by identity
            services.AddScoped<IUserValidator<TUser>, UserValidator<TUser>>();
            services.AddScoped<IPasswordValidator<TUser>, PasswordValidator<TUser>>();
            services.AddScoped<IPasswordHasher<TUser>, PasswordHasher<TUser>>();
            services.AddScoped<ILookupNormalizer, UpperInvariantLookupNormalizer>();
            services.AddScoped<IUserConfirmation<TUser>, DefaultUserConfirmation<TUser>>();
            // No interface for the error describer so we can add errors without rev'ing the interface
            services.AddScoped<IdentityErrorDescriber>();
            services.AddScoped<IUserClaimsPrincipalFactory<TUser>, UserClaimsPrincipalFactory<TUser>>();
            services.AddScoped<UserManager<TUser>>();

            services.Configure<IdentityOptions>(o =>
            {
                o.Stores.MaxLengthForKeys = 128;
                o.SignIn.RequireConfirmedAccount = true;
            });

            var identityBuilder = new IdentityBuilder(typeof(TUser), services);
            // NOTE: jrg: End `AddIdentityCore` expansion.

            // NOTE: jrg: Expand (WIP, use overrides below instead)
            identityBuilder.AddDefaultUI();
            // TODO: For some reason our expanded version of `AddMyExpandedIdentityUI`
            // has trouble loading UI components. This is why we still need
            // the `AddDefaultUI` call above.
            // services.AddMyExpandedIdentityUI<TUser>();
            // NOTE: jrg: End `AddDefaultUI` expansion.

            // NOTE: jrg: Expand
            // .AddDefaultTokenProviders()
            services
                .AddMyTokenProvider<DataProtectorTokenProvider<TUser>, TUser>(TokenOptions.DefaultProvider)
                .AddMyTokenProvider<PhoneNumberTokenProvider<TUser>, TUser>(TokenOptions.DefaultEmailProvider)
                .AddMyTokenProvider<EmailTokenProvider<TUser>, TUser>(TokenOptions.DefaultPhoneProvider)
                .AddMyTokenProvider<AuthenticatorTokenProvider<TUser>, TUser>(TokenOptions.DefaultAuthenticatorProvider);
            // NOTE: jrg: End `AddDefaultTokenProviders` expansion.

            // NOTE: jrg: End `AddDefaultIdentity` expansion.

            // NOTE: jrg: Expand
            //   .AddEntityFrameworkStores<TContext>();
            services.AddScoped<IUserStore<TUser>, UserStore<TUser, TRole, TContext, TKey, IdentityUserClaim<TKey>, IdentityUserRole<TKey>, IdentityUserLogin<TKey>, IdentityUserToken<TKey>, IdentityRoleClaim<TKey>>>();
            services.AddScoped<IRoleStore<TRole>, RoleStore<TRole, TContext, TKey, IdentityUserRole<TKey>, IdentityRoleClaim<TKey>>>();

            // NOTE: jrg: Could also have been:
            // services.AddScoped<IUserStore<TUser>, UserOnlyStore<TUser, TContext, TKey, IdentityUserClaim<TKey>, IdentityUserLogin<TKey>, IdentityUserToken<TKey>>>();
            // NOTE: jrg: End `AddEntityFrameworkStores` expansion.

            services.AddRazorPages();
            services.AddServerSideBlazor();


            // NOTE: jrg: Simplify.
            services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<TUser>>();
            // services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();

            return services;
        }
    }
}

