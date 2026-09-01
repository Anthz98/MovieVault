using MovieVault.Context;
using MovieVault.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MovieVault.JWT
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IConfiguration _configuration;
        private readonly MongoDbContext _context;

        public AuthenticationService(IConfiguration configuration, MongoDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        public async Task<AuthResult> LogInAttempts(LogIn logIn)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(i => i.Username == logIn.username);

            // BCrypt.Verify hashes the candidate password with the stored salt and compares
            // the result — the stored value is never compared as plain text.
            if (account == null || string.IsNullOrEmpty(account.Password) || !BCrypt.Net.BCrypt.Verify(logIn.password, account.Password))
            {
                return AuthResult.Failure();
            }

            var (accessToken, accessExpiry) = GenerateAccessToken(account.Username);
            var (refreshToken, refreshExpiry) = GenerateRefreshToken();

            account.IsLoggedIn = true;
            account.LogInAttempts++;
            account.RefreshToken = refreshToken;
            account.RefreshTokenExpiryTime = refreshExpiry.ToString("O");
            await _context.SaveChangesAsync();

            return AuthResult.Success(accessToken, accessExpiry, refreshToken, refreshExpiry);
        }

        public async Task<AuthResult> CreateAccount(Accounts useraccount)
        {
            // A duplicate check has to be on the identifying fields (username/email), not
            // username+password — checking the password here was never meaningful, since a
            // second person choosing the same password isn't the same account.
            var exists = await _context.Accounts.AnyAsync(i => i.Username == useraccount.Username || i.Email == useraccount.Email);
            if (exists)
            {
                return AuthResult.Failure();
            }

            useraccount.Password = BCrypt.Net.BCrypt.HashPassword(useraccount.Password);

            var (accessToken, accessExpiry) = GenerateAccessToken(useraccount.Username);
            var (refreshToken, refreshExpiry) = GenerateRefreshToken();

            useraccount.IsLoggedIn = true;
            useraccount.LogInAttempts = 1;
            useraccount.RefreshToken = refreshToken;
            useraccount.RefreshTokenExpiryTime = refreshExpiry.ToString("O");

            await _context.Accounts.AddAsync(useraccount);
            await _context.SaveChangesAsync();

            return AuthResult.Success(accessToken, accessExpiry, refreshToken, refreshExpiry);
        }

        public async Task<bool> LogOutAttempt(string username)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(i => i.Username == username);

            if (account == null || !account.IsLoggedIn)
            {
                return false;
            }

            account.IsLoggedIn = false;
            account.RefreshToken = null;
            account.RefreshTokenExpiryTime = null;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<AuthResult> RefreshAccessToken(string refreshToken)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(i => i.RefreshToken == refreshToken);

            // The original condition here was inverted: it treated a refresh token as
            // "Invalid" when its expiry was still in the future (i.e. while it was still
            // valid), and would have accepted it once actually expired. This now returns
            // failure only when the account/token is missing or the expiry has passed.
            var isExpired = account is null
                || string.IsNullOrEmpty(account.RefreshTokenExpiryTime)
                || !DateTime.TryParse(account.RefreshTokenExpiryTime, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var expiry)
                || expiry <= DateTime.UtcNow;

            if (isExpired || account is null)
            {
                return AuthResult.Failure();
            }

            var (accessToken, accessExpiry) = GenerateAccessToken(account!.Username);
            var (newRefreshToken, refreshExpiry) = GenerateRefreshToken(); // rotate on every use

            account.RefreshToken = newRefreshToken;
            account.RefreshTokenExpiryTime = refreshExpiry.ToString("O");
            await _context.SaveChangesAsync();

            return AuthResult.Success(accessToken, accessExpiry, newRefreshToken, refreshExpiry);
        }

        // Short-lived, signed JWT sent as the "Token" header and used to authenticate
        // every subsequent request via [Authorize].
        private (string AccessToken, DateTime Expiry) GenerateAccessToken(string username)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiry = DateTime.UtcNow.AddMinutes(Convert.ToInt32(_configuration["Jwt:TokenLifeSpan"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expiry,
                signingCredentials: creds);

            return (new JwtSecurityTokenHandler().WriteToken(token), expiry);
        }

        // Long-lived opaque random value, stored server-side and looked up by value.
        // It carries no claims and isn't a JWT — it exists purely to obtain a new
        // access token once the short-lived one expires.
        private (string RefreshToken, DateTime Expiry) GenerateRefreshToken()
        {
            var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var expiry = DateTime.UtcNow.AddMinutes(Convert.ToInt32(_configuration["Jwt:RefreshTokenLifeSpan"]));
            return (refreshToken, expiry);
        }
    }
}
