using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Security.Claims;
using System.Text.Json;

namespace BBooking.Blazor.Services
{
    /// <summary>
    /// Provider personalizzato per la gestione dello stato di autenticazione in Blazor WebAssembly.
    /// Utilizza il LocalStorage per memorizzare e recuperare il token JWT.
    /// </summary>
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());

        public CustomAuthStateProvider(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        /// <summary>
        /// Recupera lo stato di autenticazione corrente dell'utente.
        /// </summary>
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                // Recupera il token dal LocalStorage
                var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");

                if (string.IsNullOrWhiteSpace(token))
                {
                    return new AuthenticationState(_anonymous);
                }

                // Crea l'identità dell'utente dai claim contenuti nel token
                var claims = ParseClaimsFromJwt(token);
                var identity = new ClaimsIdentity(claims, "jwt");
                var user = new ClaimsPrincipal(identity);

                return new AuthenticationState(user);
            }
            catch
            {
                // In caso di errore (es. token malformato o problemi JS), restituisce utente anonimo
                return new AuthenticationState(_anonymous);
            }
        }

        /// <summary>
        /// Gestisce il login salvando il token e notificando il cambiamento di stato a Blazor.
        /// </summary>
        /// <param name="token">Il token JWT ricevuto dal server.</param>
        public async Task NotificaSitoLogin(string token)
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "authToken", token);
            
            var claims = ParseClaimsFromJwt(token);
            var identity = new ClaimsIdentity(claims, "jwt");
            var user = new ClaimsPrincipal(identity);
            
            var authState = Task.FromResult(new AuthenticationState(user));
            NotifyAuthenticationStateChanged(authState);
        }

        /// <summary>
        /// Gestisce il logout rimuovendo il token e notificando il cambiamento di stato a Blazor.
        /// </summary>
        public async Task NotificaSitoLogout()
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "authToken");
            
            var authState = Task.FromResult(new AuthenticationState(_anonymous));
            NotifyAuthenticationStateChanged(authState);
        }

        /// <summary>
        /// Metodo ausiliario per estrarre i claim dal payload di un token JWT (Base64).
        /// </summary>
        private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var claims = new List<Claim>();
            var payload = jwt.Split('.')[1];

            var jsonBytes = ParseBase64WithoutPadding(payload);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

            if (keyValuePairs != null)
            {
                foreach (var kvp in keyValuePairs)
                {
                    var key = kvp.Key;
                    var value = kvp.Value;

                    // Normalizzazione delle chiavi comuni dei claim
                    if (key == "sub" || key == ClaimTypes.NameIdentifier)
                    {
                        claims.Add(new Claim(ClaimTypes.NameIdentifier, value.ToString() ?? ""));
                    }
                    else if (key == "email" || key == ClaimTypes.Email)
                    {
                        claims.Add(new Claim(ClaimTypes.Email, value.ToString() ?? ""));
                    }
                    else if (key == "role" || key == ClaimTypes.Role)
                    {
                        if (value is JsonElement element && element.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var item in element.EnumerateArray())
                            {
                                claims.Add(new Claim(ClaimTypes.Role, item.ToString()));
                            }
                        }
                        else
                        {
                            claims.Add(new Claim(ClaimTypes.Role, value.ToString() ?? ""));
                        }
                    }
                }
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
