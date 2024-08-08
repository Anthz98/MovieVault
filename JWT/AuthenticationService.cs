using DnsClient;
using EntityFramework.Context;
using EntityFramework.Models;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace EntityFramework.JWT
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


        public async Task<Tuple<string, DateTime>> LogInAttempts(LogIn logIn)
        {
            //var _Filter =  Builders<Accounts>.Filter.Eq(a => a.Username, logIn.username) &
            //                    Builders<Accounts>.Filter.Eq(a => a.Password, logIn.password);

            //var account = _context.Accounts.Find(_Filter);
            var account = _context.Accounts.FirstOrDefault(i => i.Username == logIn.username && i.Password == logIn.password);//.Find(_Filter);

            if (account == null)
            {
                return new Tuple<string, DateTime>("Invalid", DateTime.Now);
            }
            else
            {
                var tokens = GenerateToken(string.Concat(logIn.username, ":", logIn.password,":",DateTime.Now.ToString())); //refresh token

                account.IsLoggedIn = true;
                account.LogInAttempts++;
                account.RefreshToken = string.Concat("Bearer ", tokens.Item1);
                account.RefreshTokenExpiryTime = tokens.Item2.ToString();
                await _context.SaveChangesAsync();

                return tokens;
            }
        }


        public async Task<Tuple<string, DateTime>> CreateAccount(Accounts useraccount)
        {

            var account = _context.Accounts.FirstOrDefault(i => i.Username == useraccount.Username && i.Password == useraccount.Password);//.Find(_Filter);

            if (account != null)
            {
                return new Tuple<string, DateTime>("Invalid", DateTime.Now);
            }
            else
            {
                var tokens = GenerateToken(string.Concat(useraccount.Username, ":", useraccount.Password, ":", DateTime.Now.ToString())); //refresh token
                useraccount.RefreshToken = string.Concat("Bearer ", tokens.Item1);
                useraccount.RefreshTokenExpiryTime = tokens.Item2.ToString();

                await _context.Accounts.AddAsync(useraccount);
                await _context.SaveChangesAsync();
                return tokens;
            }
        }

        public async Task<bool> LogOutAttempt(string RefreshToken)
        {
            var account = _context.Accounts.FirstOrDefault(i => i.RefreshToken == RefreshToken);

            if (account == null || !account.IsLoggedIn)
            {
                return false;
            }
            else
            {
                account.IsLoggedIn = false;
                account.RefreshToken = null;
                await _context.SaveChangesAsync();
                return true;
            }
        }



        public async Task<Tuple<string, DateTime>> GetAccessToken(string RefreshToken)
        {
            var account = _context.Accounts.FirstOrDefault(i =>i.RefreshToken == RefreshToken);

            if (account == null || Convert.ToDateTime(account.RefreshTokenExpiryTime) >= DateTime.Now)
            {
                return new Tuple<string, DateTime>("Invalid", DateTime.Now);
            }
            else
            {
                return GenerateToken(string.Concat(account.Username, ":", account.Password, ":", DateTime.Now.ToString())); //access token
            }
        }

        private Tuple<string, DateTime> GenerateToken(string username)
        {
            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var ex = DateTime.Now.AddMinutes(Convert.ToInt32(_configuration["Jwt:TokenLifeSpan"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: ex,
                signingCredentials: creds);

            return new Tuple<string, DateTime>(new JwtSecurityTokenHandler().WriteToken(token), ex);
        }

        private Tuple<string, DateTime> GenerateAccessToken(string username)
        {
            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var ex = DateTime.Now.AddMinutes(Convert.ToInt32(_configuration["Jwt:RefreshTokenLifeSpan"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: ex,
                signingCredentials: creds);

            return new Tuple<string, DateTime>(new JwtSecurityTokenHandler().WriteToken(token), ex);
        }

    }
}
