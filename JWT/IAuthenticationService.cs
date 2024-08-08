using EntityFramework.Models;

namespace EntityFramework.JWT
{
    public interface IAuthenticationService
    {
        Task<Tuple<string, DateTime>> LogInAttempts(LogIn logIn);
        Task<Tuple<string, DateTime>> CreateAccount(Accounts useraccount);
        Task<bool> LogOutAttempt(string RefreshToken);
        Task<Tuple<string, DateTime>> GetAccessToken(string RefreshToken);
    }
}
