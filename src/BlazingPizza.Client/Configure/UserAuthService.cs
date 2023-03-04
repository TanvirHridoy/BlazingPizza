using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Authorization;

namespace BlazingPizza.Client.Configure
{
    public class UserAuthService : IUserAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthenticationStateProvider _authProvider;
        public UserAuthService(HttpClient httpClient,AuthenticationStateProvider authenticationStateProvider)
        {
            _httpClient = httpClient;
            _authProvider = authenticationStateProvider;

        }

        public async Task<bool> IsAdminAsync()
        {
            var autostate = await _authProvider.GetAuthenticationStateAsync();
            var user = autostate != null ? autostate.User : null;
            if (user != null && String.IsNullOrEmpty(user.Identity.Name) == false)
            {
                var roles = await _httpClient.GetFromJsonAsync<List<string>>($"/roles/{user.Identity.Name}");
                return roles.Contains("Admin");
            }
            else
            {
                return false;
            }
           
        }
        public  bool IsAdmin()
        {
            var autostate =  _authProvider.GetAuthenticationStateAsync().Result;
            var user = autostate != null ? autostate.User : null;
            if (user != null && String.IsNullOrEmpty(user.Identity.Name) == false)
            {
                var roles =  _httpClient.GetFromJsonAsync<List<string>>($"/roles/{user.Identity.Name}").Result;
                return roles.Contains("Admin");
            }
            else
            {
                return false;
            }

        }

    }
}
