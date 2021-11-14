namespace OtfCli.Models.Responses
{
    public record LoginResponse(
        string Email,
        bool EmailVerified,
        string MemberId,
        string GivenName,
        string Locale,
        string HomeStudioId,
        string FamilyName,
        bool IsMigration,
        string JwtToken,
        DateTime Expiration,
        DateTime IssuedOn);
}