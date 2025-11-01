using VectorStinger.Core.Domain.Infrastructure.Oauth;
using FluentResults;

namespace VectorStinger.Core.Interfaces.Infrastructure.Oauth
{
    public interface IProviderAuthentication
    {
        /// <summary>
        /// Método para autenticar al usuario con un proveedor externo.
        /// </summary>
        /// <param name="providerName">Nombre del proveedor de autenticación (ej. Google, Facebook, etc.)</param>
        /// <param name="accessToken">Token de acceso proporcionado por el proveedor.</param>
        /// <returns>Un objeto que representa al usuario autenticado.</returns>
        Task<Result<UserFromProvider>> AuthenticateAsync(string providerName, string accessToken);
    }

}
