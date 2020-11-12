using System;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using blazor_auth_individual_experiment.Data;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using blazor_auth_individual_experiment.Areas.Identity;

namespace blazor_auth_individual_experiment
{
    static class MyExpandedIdentityReplicaServiceCollectionExtensions
    {
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

            services.AddAuthentication(o =>
            {
                o.DefaultScheme = IdentityConstants.ApplicationScheme;
                o.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            })
            .AddIdentityCookies(o => { });

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

            Action<IdentityOptions> setupAction = o =>
            {
                o.Stores.MaxLengthForKeys = 128;
                o.SignIn.RequireConfirmedAccount = true;
            };
            services.Configure(setupAction);


            // NOTE: jrg: Expand (WIP, use overrides below instead)
            var identityBuilder = new IdentityBuilder(typeof(TUser), services);
            identityBuilder.AddDefaultUI();
            // TODO: For some reason our expanded version of `AddMyExpandedIdentityUI`
            // has trouble loading UI components. This is why we still need
            // the `AddDefaultUI` call above.
            services.AddMyExpandedIdentityUI<TUser>();

            // NOTE: jrg: Expand
            // .AddDefaultTokenProviders()
            identityBuilder
                .AddTokenProvider<DataProtectorTokenProvider<TUser>>(TokenOptions.DefaultProvider)
                .AddTokenProvider<PhoneNumberTokenProvider<TUser>>(TokenOptions.DefaultEmailProvider)
                .AddTokenProvider<EmailTokenProvider<TUser>>(TokenOptions.DefaultPhoneProvider)
                .AddTokenProvider<AuthenticatorTokenProvider<TUser>>(TokenOptions.DefaultAuthenticatorProvider);


            // NOTE: jrg: Expand
            //   .AddEntityFrameworkStores<TContext>();
            services.AddScoped<IUserStore<TUser>, UserStore<TUser, TRole, TContext, TKey, IdentityUserClaim<TKey>, IdentityUserRole<TKey>, IdentityUserLogin<TKey>, IdentityUserToken<TKey>, IdentityRoleClaim<TKey>>>();
            services.AddScoped<IRoleStore<TRole>, RoleStore<TRole, TContext, TKey, IdentityUserRole<TKey>, IdentityRoleClaim<TKey>>>();

            // NOTE: jrg: Could also have been:
            // services.AddScoped<IUserStore<TUser>, UserOnlyStore<TUser, TContext, TKey, IdentityUserClaim<TKey>, IdentityUserLogin<TKey>, IdentityUserToken<TKey>>>();


            services.AddRazorPages();
            services.AddServerSideBlazor();


            // NOTE: jrg: Simplify.
            services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<TUser>>();
            // services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();

            return services;
        }
    }
}

