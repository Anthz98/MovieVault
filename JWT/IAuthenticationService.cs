using MovieVault.Models;

namespace MovieVault.JWT
{
    public interface IAuthenticationService
    {
        Task<AuthResult> LogInAttempts(LogIn logIn);
        Task<AuthResult> CreateAccount(Accounts useraccount);
        Task<bool> LogOutAttempt(string username);
        Task<AuthResult> RefreshAccessToken(string refreshToken);
    }
}
