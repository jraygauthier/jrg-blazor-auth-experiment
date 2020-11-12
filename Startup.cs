using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using blazor_auth_individual_experiment.Data;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Identity.UI.V4.Pages;

namespace blazor_auth_individual_experiment
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(
                    Configuration.GetConnectionString("DefaultConnection")));


            // NOTE: jrg: Expand.
            // services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
            //    .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddAuthentication(o =>
            {
                o.DefaultScheme = IdentityConstants.ApplicationScheme;
                o.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            })
            .AddIdentityCookies(o => { });

            var identityBuilder = services.AddIdentityCore<IdentityUser>(o =>
            {
                o.Stores.MaxLengthForKeys = 128;
                o.SignIn.RequireConfirmedAccount = true;
            });

            // NOTE: jrg: Expand (WIP, use overrides below instead)
            identityBuilder.AddDefaultUI();

            // NOTE: jrg: Expand
            // identityBuilder
            //     .AddSignInManager();
            services.AddHttpContextAccessor();
            services.AddScoped<ISecurityStampValidator, SecurityStampValidator<IdentityUser>>();
            services.AddScoped<ITwoFactorSecurityStampValidator, TwoFactorSecurityStampValidator<IdentityUser>>();
            services.AddScoped<SignInManager<IdentityUser>>();

            // TODO: For some reason our own `MyIdentityBuilderUIExtensions.ConfigureApplicationPartManager`
            // is unable to properly load the UI. This is why we kept the `AddDefaultUI`
            // and override its components.
            // services
            //     .AddMvc()
            //         .ConfigureApplicationPartManager(partManager =>
            //         {
            //             MyIdentityBuilderUIExtensions.ConfigureApplicationPartManager(partManager);
            //         });
            services
                .ConfigureOptions<MyIdentityDefaultUIConfigureOptions<IdentityUser>>()
                .AddTransient<IEmailSender, MyEmailSender>();

            // NOTE: jrg: Expand
            // .AddDefaultTokenProviders()
            identityBuilder
                .AddTokenProvider<DataProtectorTokenProvider<IdentityUser>>(TokenOptions.DefaultProvider)
                .AddTokenProvider<PhoneNumberTokenProvider<IdentityUser>>(TokenOptions.DefaultEmailProvider)
                .AddTokenProvider<EmailTokenProvider<IdentityUser>>(TokenOptions.DefaultPhoneProvider)
                .AddTokenProvider<AuthenticatorTokenProvider<IdentityUser>>(TokenOptions.DefaultAuthenticatorProvider);


            // NOTE: jrg: Expand
            //   .AddEntityFrameworkStores<ApplicationDbContext>();
            services.AddScoped<IUserStore<IdentityUser>, UserStore<IdentityUser, IdentityRole, ApplicationDbContext, string, IdentityUserClaim<string>, IdentityUserRole<string>, IdentityUserLogin<string>, IdentityUserToken<string>, IdentityRoleClaim<string>>>();
            services.AddScoped<IRoleStore<IdentityRole>, RoleStore<IdentityRole, ApplicationDbContext, string, IdentityUserRole<string>, IdentityRoleClaim<string>>>();

            // NOTE: jrg: Could also have been:
            // services.AddScoped<IUserStore<IdentityUser>, UserOnlyStore<IdentityUser, ApplicationDbContext, string, IdentityUserClaim<string>, IdentityUserLogin<string>, IdentityUserToken<string>>>();


            services.AddRazorPages();
            services.AddServerSideBlazor();


            // NOTE: jrg: Simplify.
            // services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<IdentityUser>>();
            services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();


            services.AddScoped<WeatherForecastService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
