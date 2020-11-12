Readme
======

A .NET core blazor experiment with authentication.


I started from [Blazor Server project template] (
`dotnet new blazorserver -au Individual`), incrementally
expanding and simplifying the `services.AddDefaultIdentity<IdentityUser>`
`Startup.cs` call so that I could understand what's occuring under the
blanket and replace the various part with customizations.

All expansion and simplification have the `NOTE: jrg:` mention in comment
followed by the original call in comment and followed by its expansion.

[Blazor Server project template]: https://docs.microsoft.com/en-us/aspnet/core/blazor/security/server/?view=aspnetcore-3.1&tabs=netcore-cli#blazor-server-project-template

