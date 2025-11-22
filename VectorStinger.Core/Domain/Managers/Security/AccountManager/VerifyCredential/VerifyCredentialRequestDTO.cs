namespace VectorStinger.Core.Domain.Managers.Security.AccountManager.VerifyCredential
{
    public record VerifyCredentialRequestDTO(
        string User,
        string Password,
        string VersionApplication);
}
