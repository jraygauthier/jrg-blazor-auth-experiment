using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace blazor_auth_individual_experiment.Data
{
    public class WeatherForecastService
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        AuthenticationStateProvider _authStateProvider;
        UserManager<IdentityUser> _userMgr;

        public WeatherForecastService(
            AuthenticationStateProvider authStateProvider,
            UserManager<IdentityUser> userMgr)
        {
            _authStateProvider = authStateProvider;
            _userMgr = userMgr;
        }

        public async Task<WeatherForecast[]> GetForecastAsync(DateTime startDate)
        {
            // TODO: Move this into a separte current user service which
            // can provided either only the username / id (fast) and if
            // required the whole typed user.
            var state = await _authStateProvider.GetAuthenticationStateAsync();
            var userName = state.User.Identity.Name;
            var user = await _userMgr.GetUserAsync(state.User);

            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = startDate.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            }).ToArray();
        }
    }
}
