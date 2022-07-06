using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using BookStore.Core.Interfaces.Services;
using BookStore.Core.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace BookStoreUI.Services
{
    public class TokenAuthenticationService : AuthenticationStateProvider, ILoginServices
    {
        private static readonly string TokenKey = "TokenKey";
        private AuthenticationState Anonymous => new (new ClaimsPrincipal(new ClaimsIdentity()));
        private readonly HttpClient _httpClient;
        private readonly ProtectedSessionStorage _sessionStorage;

        public TokenAuthenticationService(
            HttpClient httpClient,
            ProtectedSessionStorage sessionStorage)
        {
            _httpClient = httpClient;
            _sessionStorage = sessionStorage;
        }
        
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var result = await _sessionStorage.GetAsync<string>("TokenKey");
            var token = result.Success ? result.Value : null;
            
            if (string.IsNullOrEmpty(token))
            {
                return Anonymous;
            }

            return BuildAuthenticationState(token);
        }

        public async void Login(string token)
        {
            await _sessionStorage.SetAsync(TokenKey, token);
            var authState = BuildAuthenticationState(token);
            NotifyAuthenticationStateChanged(Task.FromResult(authState));
        }

        public async Task<UserDetail> GetUserDetails()
        {
            var result = await _sessionStorage.GetAsync<string>("TokenKey");
            var token = result.Success ? result.Value : null;
            var authState = BuildAuthenticationState(token);
            var details = new UserDetail();
            details.UserId = authState.User.Claims.FirstOrDefault(e => e.Type == "Id")?.Value.ToString();
            details.UserName = authState.User.Claims.FirstOrDefault(e => e.Type == JwtRegisteredClaimNames.NameId)?.Value.ToString();
            details.FullName = authState.User.Claims.FirstOrDefault(e => e.Type == "FullName")?.Value.ToString();
            details.Email = authState.User.Claims.FirstOrDefault(e => e.Type == JwtRegisteredClaimNames.Email)?.Value.ToString();

            return details;
        }
        
        public async Task LogOut()
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
            await _sessionStorage.DeleteAsync("TokenKey");
            NotifyAuthenticationStateChanged(Task.FromResult(Anonymous));
        }

        public AuthenticationState BuildAuthenticationState(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt")));
        }

        private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var claims = new List<Claim>();
            var payload = jwt.Split('.')[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);
            
            if (keyValuePairs != null)
            {
                keyValuePairs.TryGetValue("role", out object roles);

                if (roles != null)
                {
                    if (roles.ToString()!.Trim().StartsWith("["))
                    {
                        var parsedRoles = JsonSerializer.Deserialize<string[]>(roles.ToString()!);
                        if (parsedRoles != null)
                        {
                            foreach (var item in parsedRoles)
                            {
                                claims.Add(new Claim(ClaimTypes.Role, item));
                            }
                        }
                    }
                    else
                    {
                        claims.Add(new Claim(ClaimTypes.Role, roles.ToString()!));
                    }

                    keyValuePairs.Remove("role");
                }

                claims.AddRange(keyValuePairs.Select((kvp =>
                    new Claim(kvp.Key, kvp.Value.ToString() ?? string.Empty))));
            }

            return claims;
        }

        private byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }

            return Convert.FromBase64String(base64);
        }
    }
}
