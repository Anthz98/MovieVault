using DnsClient;
using EntityFramework.JWT;
using EntityFramework.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EntityFramework.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class AccountController : ControllerBase
    {
        private IAuthenticationService _authbusiness;


        public AccountController(IAuthenticationService authbusiness)
        {
            _authbusiness = authbusiness;
        }





        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LogIn model)
        {
            // Authenticate user and generate token
            var token = await _authbusiness.LogInAttempts(model);


            if (token.Item1 == "Invalid")
            {
                //return Unauthorized();
                return Ok(new GlobalResponse
                {
                    code = 1,
                    message = "Wrong credentials or account doesn't exist"
                });
            }
            else
            {
               Response.Headers.Add("Token", token.Item1);
                Response.Headers.Add("TokenExpiry", token.Item2.ToString());

                //return Ok(new { Token = token.Item1 , ExpiryDate = token.Item2 });
                return Ok(new GlobalResponse
                {
                    code = 0,
                    message = "Success",
                });
            }
        }


        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> AccountCreation(Accounts account)
        {
            // Authenticate user and generate token
            var token = await _authbusiness.CreateAccount(account);


            if (token.Item1 == "Invalid")
            {
                //return Unauthorized();
                return Ok(new GlobalResponse
                {
                    code = 1,
                    message = "Account already exists"
                });
            }
            else
            {
                Response.Headers.Add("Token", token.Item1);
                Response.Headers.Add("TokenExpiry", token.Item2.ToString());

                //return Ok(new { Token = token.Item1 , ExpiryDate = token.Item2 });
                return Ok(new GlobalResponse
                {
                    code = 0,
                    message = "Success",
                });
            }
        }


        [HttpGet]
        [Authorize]
        public async Task<IActionResult> LogOut()
        {
            if (Request.Headers.TryGetValue("Authorization", out var RefreshToken))
            {
                var IsLoggedOut = await _authbusiness.LogOutAttempt(RefreshToken.FirstOrDefault());

                if (!IsLoggedOut) { return Ok(new GlobalResponse { code = 1, message = "Invalid Token Or User does not exist" }); }

                return Ok(new GlobalResponse
                {
                    code = 0,
                    message = "Success",
                });
            }
            return Ok(new GlobalResponse { code = 1, message = "Invalid Token Or User does not exist" });

        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAccessToken()
        {
            if (Request.Headers.TryGetValue("Authorization", out var RefreshToken))
            {
                var token = await _authbusiness.GetAccessToken(RefreshToken.FirstOrDefault());

                if (token.Item1 == "Invalid") { return Ok(new GlobalResponse { code = 1, message = "Invalid Token Or User does not exist" }); }

                Response.Headers.Add("Token", token.Item1);
                Response.Headers.Add("TokenExpiry", token.Item2.ToString());


                return Ok(new GlobalResponse
                {
                    code = 0,
                    message = "Success",
                });
            }
            return Ok(new GlobalResponse { code = 1, message = "Invalid Token Or User does not exist" });

        }
    }
}
