namespace VectorStinger.Core.Domain.Managers.Security.AccountManager.VerifyCredential
{
    public record VerifyCredentialRequestDTO
    {
        public required string User { get; set; } = string.Empty;
        public required string Password { get; set; } = string.Empty;
        public required string VersionApplication { get; set; } = string.Empty;
    }
}
