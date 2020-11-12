using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ApplicationParts;

namespace blazor_auth_individual_experiment
{

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    sealed class MyIdentityDefaultUIAttribute : Attribute
    {
        public MyIdentityDefaultUIAttribute(Type implementationTemplate)
        {
            Template = implementationTemplate;
        }

        public Type Template { get; }
    }

    internal class MyIdentityPageModelConvention<TUser> : IPageApplicationModelConvention where TUser : class
    {
        public void Apply(PageApplicationModel model)
        {
            var defaultUIAttribute = model.ModelType.GetCustomAttribute<MyIdentityDefaultUIAttribute>();
            if (defaultUIAttribute == null)
            {
                return;
            }

            ValidateTemplate(defaultUIAttribute.Template);
            var templateInstance = defaultUIAttribute.Template.MakeGenericType(typeof(TUser));
            model.ModelType = templateInstance.GetTypeInfo();
        }

        private void ValidateTemplate(Type template)
        {
            if (template.IsAbstract || !template.IsGenericTypeDefinition)
            {
                throw new InvalidOperationException("Implementation type can't be abstract or non generic.");
            }
            var genericArguments = template.GetGenericArguments();
            if (genericArguments.Length != 1)
            {
                throw new InvalidOperationException("Implementation type contains wrong generic arity.");
            }
        }
    }

    class MyExternalLoginsPageFilter<TUser> : IAsyncPageFilter where TUser : class
    {
        public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
        {
            var result = await next();
            if (result.Result is PageResult page)
            {
                var signInManager = context.HttpContext.RequestServices.GetRequiredService<SignInManager<TUser>>();
                var schemes = await signInManager.GetExternalAuthenticationSchemesAsync();
                var hasExternalLogins = schemes.Any();

                page.ViewData["ManageNav.HasExternalLogins"] = hasExternalLogins;
            }
        }

        public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context)
        {
            return Task.CompletedTask;
        }
    }

    internal class MyIdentityDefaultUIConfigureOptions<TUser> :
        IPostConfigureOptions<RazorPagesOptions>,
        IConfigureNamedOptions<CookieAuthenticationOptions> where TUser : class
    {
        private const string IdentityUIDefaultAreaName = "Identity";

        public MyIdentityDefaultUIConfigureOptions(
            IWebHostEnvironment environment)        {
            Environment = environment;
        }

        public IWebHostEnvironment Environment { get; }

        public void PostConfigure(string name, RazorPagesOptions options)
        {
            name = name ?? throw new ArgumentNullException(nameof(name));
            options = options ?? throw new ArgumentNullException(nameof(options));

            options.Conventions.AuthorizeAreaFolder(IdentityUIDefaultAreaName, "/Account/Manage");
            options.Conventions.AuthorizeAreaPage(IdentityUIDefaultAreaName, "/Account/Logout");
            var convention = new MyIdentityPageModelConvention<TUser>();
            options.Conventions.AddAreaFolderApplicationModelConvention(
                IdentityUIDefaultAreaName,
                "/",
                pam => convention.Apply(pam));
            options.Conventions.AddAreaFolderApplicationModelConvention(
                IdentityUIDefaultAreaName,
                "/Account/Manage",
                pam => pam.Filters.Add(new MyExternalLoginsPageFilter<TUser>()));
        }

        public void Configure(CookieAuthenticationOptions options) {
            // Nothing to do here as Configure(string name, CookieAuthenticationOptions options) is hte one setting things up.
        }

        public void Configure(string name, CookieAuthenticationOptions options)
        {
            name = name ?? throw new ArgumentNullException(nameof(name));
            options = options ?? throw new ArgumentNullException(nameof(options));

            if (string.Equals(IdentityConstants.ApplicationScheme, name, StringComparison.Ordinal))
            {
                options.LoginPath = $"/{IdentityUIDefaultAreaName}/Account/Login";
                options.LogoutPath = $"/{IdentityUIDefaultAreaName}/Account/Logout";
                options.AccessDeniedPath = $"/{IdentityUIDefaultAreaName}/Account/AccessDenied";
            }
        }
    }

    public class MyEmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string message)
        {
            return Task.CompletedTask;
        }
    }


    class MyIdentityBuilderUIExtensions
    {
        // NOTE: jrg: From `src/Identity/UI/src/IdentityBuilderUIExtensions.cs::AddRelatedParts`
        public static void ConfigureApplicationPartManager(ApplicationPartManager partManager)
        {
            var thisAssembly = typeof(IdentityBuilderUIExtensions).Assembly;
            var relatedAssemblies = RelatedAssemblyAttribute.GetRelatedAssemblies(thisAssembly, throwOnError: true);
            var relatedParts = relatedAssemblies.ToDictionary(
                ra => ra,
                CompiledRazorAssemblyApplicationPartFactory.GetDefaultApplicationParts);

            // From `_assemblyMap[UIFramework.Bootstrap4]`.
            var selectedFrameworkAssembly = "Microsoft.AspNetCore.Identity.UI.Views.V4";

            foreach (var kvp in relatedParts)
            {
                var assemblyName = kvp.Key.GetName().Name;
                if (!IsAssemblyForFramework(selectedFrameworkAssembly, assemblyName))
                {
                    RemoveParts(partManager, kvp.Value);
                }
                else
                {
                    AddParts(partManager, kvp.Value);
                }
            }

            bool IsAssemblyForFramework(string frameworkAssembly, string assemblyName) =>
                string.Equals(assemblyName, frameworkAssembly, StringComparison.OrdinalIgnoreCase);

            void RemoveParts(
                ApplicationPartManager manager,
                IEnumerable<ApplicationPart> partsToRemove)
            {
                for (var i = 0; i < manager.ApplicationParts.Count; i++)
                {
                    var part = manager.ApplicationParts[i];
                    if (partsToRemove.Any(p => string.Equals(
                            p.Name,
                            part.Name,
                            StringComparison.OrdinalIgnoreCase)))
                    {
                        manager.ApplicationParts.Remove(part);
                    }
                }
            }

            void AddParts(
                ApplicationPartManager manager,
                IEnumerable<ApplicationPart> partsToAdd)
            {
                foreach (var part in partsToAdd)
                {
                    if (!manager.ApplicationParts.Any(p => p.GetType() == part.GetType() &&
                        string.Equals(p.Name, part.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        manager.ApplicationParts.Add(part);
                    }
                }
            }
        }

    }
}