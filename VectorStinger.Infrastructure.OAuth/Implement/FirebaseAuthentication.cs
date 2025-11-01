using VectorStinger.Core.Domain.Infrastructure.Oauth;
using VectorStinger.Core.Interfaces.Infrastructure.Oauth;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using FluentResults;
using Google.Apis.Auth.OAuth2;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace VectorStinger.Infrastructure.OAuth.Implement
{
    public class FirebaseAuthentication : IProviderAuthentication
    {
        public FirebaseAuthentication()
        {
            if (FirebaseApp.DefaultInstance == null)
            {
                // Obtener el path de ejecución del assembly
                var basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? AppContext.BaseDirectory;
                var credentialsPath = Path.Combine(basePath, "Resources", "firebase-adminsdk.json");

                if (!File.Exists(credentialsPath))
                    throw new FileNotFoundException($"No se encontró el archivo de credenciales de Firebase en: {credentialsPath}");

                FirebaseApp.Create(new AppOptions
                {
                    Credential = GoogleCredential.FromFile(credentialsPath)
                });
            }
        }

        public async Task<Result<UserFromProvider>> AuthenticateAsync(string providerName, string accessToken)
        {
            UserFromProvider user = new UserFromProvider();
            try
            {
                FirebaseToken decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(accessToken);
                // Extraer claims y propiedades estándar
                string iss = decodedToken.Claims.TryGetValue("iss", out var issVal) ? issVal?.ToString() ?? string.Empty : string.Empty;
                string aud = decodedToken.Claims.TryGetValue("aud", out var audVal) ? audVal?.ToString() ?? string.Empty : string.Empty;
                long authTime = decodedToken.Claims.TryGetValue("auth_time", out var authTimeVal) && long.TryParse(authTimeVal?.ToString(), out var at) ? at : 0;
                string userId = decodedToken.Claims.TryGetValue("user_id", out var userIdVal) ? userIdVal?.ToString() ?? string.Empty : decodedToken.Uid ?? string.Empty;
                string sub = decodedToken.Claims.TryGetValue("sub", out var subVal) ? subVal?.ToString() ?? string.Empty : string.Empty;
                string email = decodedToken.Claims.TryGetValue("email", out var e) ? e?.ToString() ?? string.Empty : string.Empty;
                bool emailVerified = decodedToken.Claims.TryGetValue("email_verified", out var ev) && bool.TryParse(ev?.ToString(), out var verified) ? verified : false;
                string name = decodedToken.Claims.TryGetValue("name", out var n) ? n?.ToString() ?? string.Empty : string.Empty;
                string picture = decodedToken.Claims.TryGetValue("picture", out var p) ? p?.ToString() ?? string.Empty : string.Empty;

                // Extraer información de firebase
                string signInProvider = decodedToken.Subject ?? providerName ?? "unknown";

                // Construir el objeto de salida
                user = new UserFromProvider
                {
                    UserId = userId,
                    Email = email,
                    Name = name,
                    Picture = picture,
                    EmailVerified = emailVerified,
                    ProviderInfo = new ProviderInfo
                    {
                        SignInProvider = signInProvider
                    }
                };
            }
            catch (FirebaseAdmin.Auth.FirebaseAuthException firebaseException)
            {
                return Result.Fail(firebaseException.Message);
            }
            return user;
        }
    }
}
