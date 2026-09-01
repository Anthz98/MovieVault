using MovieVault.JWT;
using MovieVault.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MovieVault.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class AccountController : ControllerBase
    {
        private readonly IAuthenticationService _authbusiness;

        public AccountController(IAuthenticationService authbusiness)
        {
            _authbusiness = authbusiness;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LogIn model)
        {
            var result = await _authbusiness.LogInAttempts(model);

            if (!result.Succeeded)
            {
                return Ok(new GlobalResponse
                {
                    code = 1,
                    message = "Wrong credentials or account doesn't exist"
                });
            }

            SetTokenHeaders(result);
            return Ok(new GlobalResponse
            {
                code = 0,
                message = "Success",
            });
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> AccountCreation(Accounts account)
        {
            var result = await _authbusiness.CreateAccount(account);

            if (!result.Succeeded)
            {
                return Ok(new GlobalResponse
                {
                    code = 1,
                    message = "Account already exists"
                });
            }

            SetTokenHeaders(result);
            return Ok(new GlobalResponse
            {
                code = 0,
                message = "Success",
            });
        }

        // Replaces the old GetAccessToken endpoint. It has to be anonymous and take the
        // refresh token explicitly in the body: the whole point of a refresh token is to get
        // a new access token once the old access token has expired, so this endpoint can't
        // require a still-valid access token (a still-valid [Authorize] Bearer token) to call.
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken(RefreshTokenRequest model)
        {
            var result = await _authbusiness.RefreshAccessToken(model.RefreshToken);

            if (!result.Succeeded)
            {
                return Ok(new GlobalResponse { code = 1, message = "Invalid or expired refresh token" });
            }

            SetTokenHeaders(result);
            return Ok(new GlobalResponse
            {
                code = 0,
                message = "Success",
            });
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> LogOut()
        {
            // The caller already proved who they are via the validated access token
            // (that's what [Authorize] just checked) — read the username from its claims
            // instead of re-parsing a header value by hand.
            var username = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }

            var isLoggedOut = await _authbusiness.LogOutAttempt(username);

            return Ok(isLoggedOut
                ? new GlobalResponse { code = 0, message = "Success" }
                : new GlobalResponse { code = 1, message = "Invalid Token Or User does not exist" });
        }

        private void SetTokenHeaders(AuthResult result)
        {
            Response.Headers["Token"] = result.AccessToken!;
            Response.Headers["TokenExpiry"] = result.AccessTokenExpiry!.Value.ToString("O");
            Response.Headers["RefreshToken"] = result.RefreshToken!;
            Response.Headers["RefreshTokenExpiry"] = result.RefreshTokenExpiry!.Value.ToString("O");
        }
    }
}
