using System.Security.Claims;
using Api.Framework;
using Api.Framework.Extensions;
using Api.Framework.Helper;
using Api.Framework.Models;
using Api.Framework.Result;
using AutoMapper;
using HappyNotes.Dto;
using HappyNotes.Entities;
using HappyNotes.Models;
using HappyNotes.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace HappyNotes.Api.Controllers
{
    [Authorize]
    public class AccountController(
        IOptions<JwtConfig> jwtConfig,
        IRepositoryBase<User> userRepository,
        IAccountService accountService,
        IMapper mapper,
        User currentUser)
        : BaseController
    {
        private readonly JwtConfig _jwtConfig = jwtConfig.Value;

        /// <summary>
        /// Refresh token
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public SuccessfulResult<JwtToken> RefreshToken()
        {
            var claims = _GetClaims(currentUser.Id, currentUser.Username, currentUser.Email);

            var token = TokenHelper.JwtTokenGenerator(claims, _jwtConfig.Issuer, _jwtConfig.SymmetricSecurityKey, 7);
            return new SuccessfulResult<JwtToken>(new JwtToken {Token = token,});
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ApiResult<JwtToken>> Register(RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                throw new ArgumentException("Username or Password cannot be empty");
            }

            request.Username = request.Username.Trim();
            request.Email = request.Email.Trim().ToLower();

            var user = await userRepository.GetFirstOrDefaultAsync(where =>
                where.Username.Equals(request.Username.Trim()));
            if (user != null)
            {
                throw new Exception($"Username {request.Username.Trim()} was already taken by someone else");
            }

            string gravatar = String.Empty;
            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                user = await userRepository.GetFirstOrDefaultAsync(w => w.Email.Equals(request.Email.Trim().ToLower()));
                if (user != null)
                {
                    throw new Exception($"Email {request.Email.Trim()} was already being used by another account");
                }

                gravatar = GravatarHelper.GetGravatarUrl(request.Email);
            }

            var salt = SaltGenerator.GenerateSaltString(64);
            var password = CommonHelper.CalculateSha256Hash(request.Password + salt);
            var newUser = new User()
            {
                Username = request.Username,
                Email = request.Email,
                Gravatar = gravatar,
                Salt = salt,
                Password = password,
                CreateAt = DateTime.Now.ToUnixTimestamp(),
            };

            var id = await userRepository.InsertReturnIdentityAsync(newUser);

            var claims = _GetClaims(id, request.Username, request.Email);
            var jwtToken = TokenHelper.JwtTokenGenerator(claims, _jwtConfig.Issuer, _jwtConfig.SymmetricSecurityKey, 7);

            return new SuccessfulResult<JwtToken>(new JwtToken {Token = jwtToken,});
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ApiResult<JwtToken>> Login(LoginRequest loginRequest)
        {
            if (string.IsNullOrWhiteSpace(loginRequest.Username) || string.IsNullOrWhiteSpace(loginRequest.Password))
            {
                throw new ArgumentException("Username or Password cannot be empty");
            }

            var validateResult = await accountService.IsValidLogin(loginRequest.Username, loginRequest.Password);
            if (!validateResult)
            {
                throw new Exception("Username or Password is incorrect");
            }

            var user = await userRepository.GetFirstOrDefaultAsync(
                where => where.Username.Equals(loginRequest.Username));
            if (user == null)
            {
                throw new Exception("Unexpected error, user not found.");
            }

            if (user.DeleteAt != null)
            {
                throw new Exception("Sorry, your account has been deleted");
            }

            var claims = _GetClaims(user.Id, user.Username, user.Email);
            var jwtToken = TokenHelper.JwtTokenGenerator(claims, _jwtConfig.Issuer, _jwtConfig.SymmetricSecurityKey, 7);

            return new SuccessfulResult<JwtToken>(new JwtToken {Token = jwtToken,});
        }

        [HttpGet]
        public async Task<ApiResult<UserDto>> MyInformation()
        {
            var user = await userRepository.GetFirstOrDefaultAsync(where => where.Id == currentUser.Id);
            var userDto = mapper.Map<UserDto>(user);
            return new SuccessfulResult<UserDto>(userDto);
        }

        private static Claim[] _GetClaims(long id, string username, string email)
        {
            var claims = new Claim[]
            {
                new(ClaimTypes.Name, username),
                new(ClaimTypes.Email, email),
                new(ClaimTypes.NameIdentifier, id.ToString()),
            };
            return claims;
        }
    }
}