using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using ThuHaiDuong.Domain.Entities;
using ThuHaiDuong.Domain.InterfaceRepositories;
using ThuHaiDuong.Domain.Validation;
using BCryptNet = BCrypt.Net.BCrypt;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using ThuHaiDuong.Application.InterfaceService;
using ThuHaiDuong.Application.Payloads.ResultModels.User;
using ThuHaiDuong.Application.Payloads.Responses;
using ThuHaiDuong.Application.Payloads.InputModels.Admin.Auth;

namespace ThuHaiDuong.Application.ImplementService
{
    public class AuthService : IAuthService
    {
        private readonly IBaseRepository<User> _baseUserRepository;
        private readonly IConfiguration _configuration;
        private readonly IUserRepository _userRepository;
        private readonly IBaseRepository<RefreshToken> _baseRefreshTokenRepository;
        private readonly ICurrentUserService _currentUserService;
        public AuthService(IBaseRepository<User> baseUserRepository, IConfiguration configuration, IUserRepository userRepository, 
            IBaseRepository<RefreshToken> baseRefreshTokenRepository, ICurrentUserService currentUserService)
        {
            _baseUserRepository = baseUserRepository;
            _configuration = configuration;
            _userRepository = userRepository;
            _baseRefreshTokenRepository = baseRefreshTokenRepository;
            _currentUserService = currentUserService;
        }

        #region Private Methods
        private JwtSecurityToken GetToken(List<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:SecretKey"]));
            _ = int.TryParse(_configuration["JWT:TokenValidityInHours"], out int tokenValidityInHours);
            var expirationUTC = DateTime.Now.AddHours(tokenValidityInHours);

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: expirationUTC,
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );
            return token;
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
            }
            return Convert.ToBase64String(randomNumber);
        }
        #endregion

        public async Task<ResponseObject<LoginResult>> GetJwtTokenAsync(User user)
        {
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.MobilePhone, user.PhoneNumber ?? ""),
                new Claim("UserId", user.Id.ToString()),
            };

            var jwtToken = GetToken(authClaims);
            var refreshToken = GenerateRefreshToken();
            if (!int.TryParse(_configuration["JWT:RefreshTokenValidity"], out int refreshTokenValidity))
            {
                refreshTokenValidity = 24;
            }

            var rf = new RefreshToken
            {
                UserId = user.Id,
                Token = refreshToken,
                ExpiryTime = DateTime.UtcNow.AddHours(refreshTokenValidity),
                IsRevoked = false,
                CreatedAt = DateTime.UtcNow
            };

            await _baseRefreshTokenRepository.CreateAsync(rf);

            return new ResponseObject<LoginResult>
            {
                Status = 200,
                Message = "Create token successfully!",
                Data = new LoginResult
                {
                    AccessToken = new JwtSecurityTokenHandler().WriteToken(jwtToken),
                    RefreshToken = refreshToken,
                }
            };
        }


        public async Task<LoginResult> LoginAsync(LoginInput request)
        {
            var user = await _baseUserRepository.GetAsync(x => x.Email == request.Email);
            if (user == null)
            {
                throw new ResponseErrorObject(
                    "Email không hợp lệ.",
                    StatusCodes.Status400BadRequest
                );
            }

            bool checkPass = BCryptNet.Verify(request.Password, user.Password);
            if (!checkPass)
            {
                throw new ResponseErrorObject(
                    "Password không hợp lệ.",
                    StatusCodes.Status400BadRequest
                );
            }

            var jwtTokenResponse = await GetJwtTokenAsync(user);
            if (jwtTokenResponse.Data == null)
            {
                throw new ResponseErrorObject(
                    "Token generation failed",
                    StatusCodes.Status500InternalServerError
                );
            }
            
            return new LoginResult
            {
                User = new UserResult
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Avatar = user.Avatar,
                    Email = user.Email,
                    FullName = user.FullName,
                    PhoneNumber = user.PhoneNumber,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt,
                    DeletedAt = user.DeletedAt,
                },
                AccessToken = jwtTokenResponse.Data.AccessToken,
                RefreshToken = jwtTokenResponse.Data.RefreshToken
            };
        }
        
        public async Task<UserResult> RegisterAsync(RegisterInput request)
        {
            if (!ValidateInput.IsValidEmail(request.Email))
            {
                throw new ResponseErrorObject(
                    "Invalid email.",
                    StatusCodes.Status400BadRequest
                );
            }

            if (await _userRepository.GetUserByEmail(request.Email) != null)
            {
                throw new ResponseErrorObject(
                    "This email address is already in use.",
                    StatusCodes.Status400BadRequest
                );
            }

            if (await _userRepository.GetUserByUsername(request.UserName) != null)
            {
                throw new ResponseErrorObject(
                    "This username is already in use.",
                    StatusCodes.Status400BadRequest
                );
            }

            try
            {
                var user = new User
                {
                    Avatar =
                        "https://static.vecteezy.com/system/resources/previews/009/292/244/original/default-avatar-icon-of-social-media-user-vector.jpg",
                    PhoneNumber = request.PhoneNumber ?? "",
                    FullName = request.FullName,
                    Password = BCryptNet.HashPassword(request.Password),
                    UserName = request.UserName,
                    Email = request.Email,
                    CreatedAt = DateTime.UtcNow,
                };

                user = await _baseUserRepository.CreateAsync(user);

            
                return new UserResult
                {
                    Id = user.Id,
                    Avatar = user.Avatar,
                    FullName = user.FullName,
                    UserName = user.UserName,
                    PhoneNumber = user.PhoneNumber,
                    Email = user.Email,
                };
            }
            catch (Exception e)
            {
                throw new ResponseErrorObject(e.Message, StatusCodes.Status400BadRequest);
            }
        }
        
        public async Task LogoutAsync()
        {
            var userId = _currentUserService.GetUserId();
            if (!userId.HasValue)
            {
                throw new ResponseErrorObject(
                    "User is not authenticated",
                    StatusCodes.Status401Unauthorized
                );
            }
            await _userRepository.RevokeRefreshTokensAsync(userId.Value);
        }

        public async Task<UserResult> GetUserInfoAsync()
        {
            var userId = _currentUserService.GetUserId();
            if (!userId.HasValue)
            {
                throw new ResponseErrorObject(
                    "User is not authenticated",
                    StatusCodes.Status401Unauthorized
                );
            }

            var user = await _baseUserRepository.BuildQueryable(new List<string> {}, u => u.Id == userId.Value)
                .FirstOrDefaultAsync();
            if (user == null)
            {
                throw new ResponseErrorObject(
                    "User not found.",
                    StatusCodes.Status404NotFound
                );
            }

            return new UserResult
            {
                Id = user.Id,
                UserName = user.UserName,
                Avatar = user.Avatar,
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                DeletedAt = user.DeletedAt,
            };
        }

        public async Task<ResponseObject<UserResult>> UpdateProfileAsync(Guid userId, UpdateProfileInput request)
        {
            try
            {
                var currentUserId = _currentUserService.GetUserId();
                if (!currentUserId.HasValue)
                {
                    throw new ResponseErrorObject(
                        "User is not authenticated",
                        StatusCodes.Status401Unauthorized
                    );
                }

                if (currentUserId != userId)
                {
                    throw new ResponseErrorObject(
                        "You are not allowed to update this user",
                        StatusCodes.Status403Forbidden
                    );
                }


                var existingUser = await _baseUserRepository.GetByIdAsync(userId);
                if (existingUser == null)
                {
                    throw new ResponseErrorObject("User not found", StatusCodes.Status404NotFound);
                }

                var userByEmail = await _userRepository.GetUserByEmail(request.Email);
                if (!string.IsNullOrWhiteSpace(request.Email) &&
                    userByEmail != null && userByEmail.Id != existingUser.Id)
                {
                    throw new ResponseErrorObject("Email already exists", StatusCodes.Status400BadRequest);
                }


                existingUser.FullName = request.FullName;
                existingUser.Email = request.Email;
                existingUser.PhoneNumber = request.PhoneNumber;
                existingUser.UpdatedAt = DateTime.UtcNow;

                await _userRepository.UpdateAsync(existingUser);


                var responseData =  new UserResult
                {
                    Id = existingUser.Id,
                    UserName = existingUser.UserName,
                    Avatar = existingUser.Avatar,
                    Email = existingUser.Email,
                    FullName = existingUser.FullName,
                    PhoneNumber = existingUser.PhoneNumber,
                    CreatedAt = existingUser.CreatedAt,
                    UpdatedAt = existingUser.UpdatedAt,
                    DeletedAt = existingUser.DeletedAt,
                };

                return new ResponseObject<UserResult>
                {
                    Status = StatusCodes.Status200OK,
                    Message = "User updated successfully",
                    Data = responseData
                };
            }
            catch (Exception ex)
            {
                throw new ResponseErrorObject(ex.Message, StatusCodes.Status400BadRequest);
            }
        }


        public async Task<ResponseObject<UserResult>> ChangePasswordAsync(Guid userId, ChangePasswordInput request)
        {
            try
            {
                var user = await _baseUserRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    throw new ResponseErrorObject("User not found", StatusCodes.Status404NotFound);
                }

                bool checkPass = BCryptNet.Verify(request.CurrentPassword, user.Password);
                if (!checkPass)
                {
                    throw new ResponseErrorObject("Incorrect old password", StatusCodes.Status400BadRequest);
                }

                if (!request.NewPassword.Equals(request.ConfirmPassword))
                {
                    throw new ResponseErrorObject("Passwords do not match", StatusCodes.Status400BadRequest);
                }

                user.Password = BCryptNet.HashPassword(request.NewPassword);
                user.UpdatedAt = DateTime.Now;
                await _baseUserRepository.UpdateAsync(user);
                var responseData = new UserResult
                {
                    Id = user.Id,
                    Avatar = user.Avatar,
                    FullName = user.FullName,
                    UserName = user.UserName,
                    PhoneNumber = user.PhoneNumber,
                    Email = user.Email,
                };
                
                return new ResponseObject<UserResult>
                {
                    Status = StatusCodes.Status200OK,
                    Message = "Password changed successfully",
                    Data = responseData
                };
            }
            catch (Exception ex)
            {
                throw new ResponseErrorObject(ex.Message, StatusCodes.Status400BadRequest);
            }
        }

        
    }
}
