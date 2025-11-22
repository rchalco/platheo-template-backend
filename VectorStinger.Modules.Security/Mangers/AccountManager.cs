using FluentResults;
using Newtonsoft.Json;
using VectorStinger.Core.Domain.DataBase.Data;
using VectorStinger.Core.Domain.DataBase.Models;
using VectorStinger.Core.Domain.Managers.Security.AccountManager.ValidateToken;
using VectorStinger.Core.Domain.Managers.Security.AccountManager.VerifyCredential;
using VectorStinger.Core.Domain.Managers.Security.AccountManager.VerifyCredentialOAuth;
using VectorStinger.Core.Domain.Managers.Security.Enus;
using VectorStinger.Core.Interfaces.Infrastructure.Oauth;
using VectorStinger.Core.Interfaces.Managers.Security;
using VectorStinger.Foundation.Abstractions.Manager;
using VectorStinger.Infrastructure.DataAccess.Interface;
using VectorStinger.Infrastructure.DataAccess.Wrapper;

namespace VectorStinger.Modules.Security.Mangers
{
    public class AccountManager : BaseManager<DbearthBnbContext>, IAccountManager
    {
        private readonly IProviderAuthentication _providerAuthentication;
        public AccountManager(IRepository repository, IProviderAuthentication providerAuthentication)
            : base(repository)
        {
            _providerAuthentication = providerAuthentication;
        }

        public Result<VerifyCredentialResponseDTO> VerifyCredentialAsync(VerifyCredentialRequestDTO request)
        {
            var outputSuccess = new Parameter<bool>(true);
            var outputToken = new Parameter<string>(string.Empty, ParameterType.Out, 500);
            var outputTokenExpiration = new Parameter<DateTime>(DateTime.Now);
            var outputDetailResponse = new Parameter<string>(string.Empty, ParameterType.Out, 100);

            _repository.CallProcedure<object>("[seguridad].[spLogin]",
                request.User,
                request.Password,
                request.VersionApplication,
                outputToken,
                outputTokenExpiration,
                outputSuccess,
                outputDetailResponse);

            if (!outputSuccess.Value)
            {
                return Result.Fail(outputDetailResponse.Value);
            }

            var response = new VerifyCredentialResponseDTO(
                outputSuccess.Value,
                outputToken.Value,
                outputTokenExpiration.Value,
                outputDetailResponse.Value);

            return Result.Ok(response);
        }

        public Result<ValidateTokenResponse> ValidateTokenAsync(ValidateTokenRequest request)
        {
            var result = _repository.SimpleSelect<TSesione>(x => x.Token.ToString().Equals(request.Token));

            if (result == null || result.Count == 0)
            {
                return Result.Fail("Token inexistente");
            }

            if (result.First().FechaVigenciaHasta < DateTime.Now)
            {
                return Result.Fail("Token expirado");
            }

            var response = new ValidateTokenResponse(
                true,
                result.First().Token.ToString(),
                result.First().FechaVigenciaHasta);

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
            var userResult = _repository.SimpleSelect<TUsuario>(x => x.Proveedor == request.Provider.ToString() && x.ProveedorUserId == verifyResult.Value.UserId);

            // Si el usuario no existe, lo creamos
            if (userResult == null || userResult.Count == 0)
            {
                // primero creamos a la persona
                var person = new TPersona
                {
                    Nombres = verifyResult.Value.Name,
                    ApellidoMaterno = string.Empty, // Asumimos que no tenemos apellido
                    ApellidoPaterno = string.Empty, // Asumimos que no tenemos apellido
                    ComplementoCi = string.Empty,
                    Direccion = string.Empty,
                    DireccionUbicacionLatitud = string.Empty,
                    DireccionUbicacionLongitud = string.Empty,
                    DocumentoDeIdentidad = string.Empty, // Asumimos que no tenemos documento de identidad
                    Extension = string.Empty,
                    FechaRegistro = DateTime.Now,
                    Nacionalidad = string.Empty, // Asumimos que no tenemos nacionalidad   
                    FechaVigenciaHasta = null, // Asumimos que no tenemos fecha de vigencia
                    IdPersona = 0, // es un nuevo registro no tiene Id
                    IdSesion = 0, // Es un registro  autenticación, no de sesión
                    IdTipoPersona = 1, // Persona natural
                    Nit = string.Empty, // Asumimos que no tenemos NIT
                    RazonSocial = string.Empty, // Asumimos que no tenemos razón social
                    Telfono = string.Empty, // Asumimos que no tenemos teléfono
                    TipoDocumentoIdentidad = string.Empty, // Asumimos que no tenemos tipo de documento de identidad
                };

                _repository.SaveObject(new Entity<TPersona>
                {
                    EntityDB = person,
                    stateEntity = StateEntity.add
                });

                // Si la persona se creó correctamente, creamos el usuario
                var user = new TUsuario
                {
                    IdPersona = person.IdPersona,
                    Proveedor = request.Provider.ToString(),
                    ProveedorUserId = verifyResult.Value.UserId,
                    Usuario = verifyResult.Value.Email,
                    Activo = 1,
                    FotoUrl = verifyResult.Value.Picture,
                    FechaRegistro = DateTime.Now,
                    FechaVigenciaHasta = null, // Asumimos que no tenemos fecha de vigencia
                    IdUsuario = 0, // es un nuevo registro no tiene Id
                    DecodedTokenClaims = JsonConvert.SerializeObject(verifyResult.Value),
                    EsInvitado = false, // Asumimos que no es un usuario invitado
                    EsVerificado = true, // Asumimos que el usuario está verificado
                    FechaUltimoLogin = DateTime.Now,
                    IdRol = (long)RolEnum.Externo, // rol por defecto es 3 (Externo)
                    IdSesion = 0, // Es un registro autenticación, no de sesión
                    Password = string.Empty // Asumimos que no tenemos contraseña, ya que es un usuario de OAuth
                };

                _repository.SaveObject(new Entity<TUsuario>
                {
                    EntityDB = user,
                    stateEntity = StateEntity.add
                });

                userResult = [user];
            }

            // le creamos el token de sesión
            var session = new TSesione
            {
                IdUsuario = userResult.First().IdUsuario,
                Token = Guid.NewGuid(),
                FechaVigenciaHasta = DateTime.Now.AddMinutes(10), // Asumimos que el token es válido por 1 día
                FechaRegistro = DateTime.Now,
                IdSesion = 0 // es un nuevo registro no tiene Id
            };

            _repository.SaveObject(new Entity<TSesione>
            {
                EntityDB = session,
                stateEntity = StateEntity.add
            });

            // Asignamos los valores de respuesta
            var response = new VerifyCredentialOAuthResponseDTO(
                true,
                userResult.First().IdUsuario,
                session.IdSesion,
                session.Token.ToString(),
                session.FechaVigenciaHasta,
                "Usuario autenticado correctamente",
                verifyResult.Value.Name,
                verifyResult.Value.Picture);

            return Result.Ok(response);
        }
    }
}
