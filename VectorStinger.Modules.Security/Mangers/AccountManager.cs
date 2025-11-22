using FluentResults;
using VectorStinger.Core.Domain.DataBase.DataContext;
using VectorStinger.Core.Domain.DataBase.Models;
using VectorStinger.Core.Domain.Managers.Security.AccountManager.ValidateToken;
using VectorStinger.Core.Domain.Managers.Security.AccountManager.VerifyCredentialOAuth;
using VectorStinger.Core.Interfaces.Infrastructure.Oauth;
using VectorStinger.Core.Interfaces.Managers.Security;
using VectorStinger.Foundation.Abstractions.Manager;
using VectorStinger.Infrastructure.DataAccess.Interface;
using VectorStinger.Infrastructure.DataAccess.Wrapper;

namespace VectorStinger.Modules.Security.Mangers
{
    public class AccountManager : BaseManager<BdPlatheoTemplateContext>, IAccountManager
    {
        private readonly IProviderAuthentication _providerAuthentication;
        public AccountManager(IRepository repository, IProviderAuthentication providerAuthentication)
            : base(repository)
        {
            _providerAuthentication = providerAuthentication;
        }

        public Result<ValidateTokenResponse> ValidateTokenAsync(ValidateTokenRequest request)
        {
            var result = _repository.SimpleSelect<Session>(x => x.Token.ToString().Equals(request.Token));

            if (result == null || result.Count == 0)
            {
                return Result.Fail("Token inexistente");
            }

            if (result.First().CreatedAt.AddMinutes(30) < DateTime.Now)
            {
                return Result.Fail("Token expirado");
            }

            var response = new ValidateTokenResponse
            {
                IsValid = true,
                TimeExpired = result.First().CreatedAt.AddMinutes(30),
                Token = result.First().Token.ToString()
            };

            return Result.Ok(response);
        }

        public async Task<Result<VerifyCredentialOAuthResponseDTO>> VerifyCredentialOAuthAsync(VerifyCredentialOAuthRequestDTO request)
        {
            // Obtenemos el token y revisamos la informacion del usuario
            var verifyResult = await _providerAuthentication.AuthenticateAsync(request.Provider.ToString(), request.Token);
            if (verifyResult.IsFailed)
            {
                return Result.Fail<VerifyCredentialOAuthResponseDTO>(verifyResult.Errors);
            }

            //Verificamos si el usuario ya existe en la base de datos
            var userResult = _repository.SimpleSelect<User>(x => x.OauthProvider == request.Provider.ToString() && x.OauthId == verifyResult.Value.UserId);

            // Si el usuario no existe, lo creamos
            if (userResult == null || userResult.Count == 0)
            {
                // Si la persona se creó correctamente, creamos el usuario
                var user = new User
                {
                    OauthId = verifyResult.Value.UserId,
                    OauthProvider = request.Provider.ToString(),
                    OauthToken = request.Token,
                    CreatedAt = DateTime.Now,
                    Email = verifyResult.Value.Email,
                    FullName = verifyResult.Value.Name,
                    UserId = 0 // es un nuevo registro no tiene Id
                };

                _repository.SaveObject(new Entity<User>
                {
                    EntityDB = user,
                    stateEntity = StateEntity.add
                });

                userResult = [user];
            }

            // le creamos el token de sesión
            var session = new Session
            {
                CreatedAt = DateTime.Now,
                UserId = userResult.First().UserId,
                Provider = request.Provider.ToString(),
                Token = Guid.NewGuid().ToString(),
                SessionId = 0, // es un nuevo registro no tiene Id
            };

            _repository.SaveObject(new Entity<Session>
            {
                EntityDB = session,
                stateEntity = StateEntity.add
            });

            // Asignamos los valores de respuesta
            var response = new VerifyCredentialOAuthResponseDTO
            {
                Expiration = session.CreatedAt.AddMinutes(30),
                IsValid = true,
                Token = session.Token.ToString(),
                Message = "Usuario autenticado correctamente",
                NamePerson = verifyResult.Value.Name,
                PictureUrl = verifyResult.Value.Picture,
                IdSesion = session.SessionId,
                IdUser = userResult.First().UserId
            };

            return Result.Ok(response);
        }
    }
}
