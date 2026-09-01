namespace MovieVault.Models
{
    /// <summary>
    /// Result of an authentication operation (login, account creation, or token refresh).
    /// AccessToken is the short-lived JWT sent on every authenticated request.
    /// RefreshToken is a long-lived opaque value used only to obtain a new AccessToken.
    /// </summary>
    public class AuthResult
    {
        public bool Succeeded { get; init; }
        public string? AccessToken { get; init; }
        public DateTime? AccessTokenExpiry { get; init; }
        public string? RefreshToken { get; init; }
        public DateTime? RefreshTokenExpiry { get; init; }

        public static AuthResult Failure() => new() { Succeeded = false };

        public static AuthResult Success(string accessToken, DateTime accessTokenExpiry, string refreshToken, DateTime refreshTokenExpiry) =>
            new()
            {
                Succeeded = true,
                AccessToken = accessToken,
                AccessTokenExpiry = accessTokenExpiry,
                RefreshToken = refreshToken,
                RefreshTokenExpiry = refreshTokenExpiry
            };
    }
}
