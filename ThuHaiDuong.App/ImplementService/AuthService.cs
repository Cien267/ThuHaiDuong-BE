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
using ThuHaiDuong.Application.Payloads.InputModels.Auth;

namespace ThuHaiDuong.Application.ImplementService
{
    public class AuthService : IAuthService
    {
        private readonly IBaseRepository<User> _baseUserRepository;
        private readonly IConfiguration _configuration;
        private readonly IUserRepository _userRepository;
        private readonly IBaseRepository<Permissions> _basePermissionsRepository;
        private readonly IBaseRepository<Role> _baseRoleRepository;
        private readonly IBaseRepository<RefreshToken> _baseRefreshTokenRepository;
        private readonly ICurrentUserService _currentUserService;
        public AuthService(IBaseRepository<User> baseUserRepository, IConfiguration configuration, IUserRepository userRepository, 
            IBaseRepository<Permissions> basePermissionsRepository, IBaseRepository<Role> baseRoleRepository, 
            IBaseRepository<RefreshToken> baseRefreshTokenRepository, ICurrentUserService currentUserService)
        {
            _baseUserRepository = baseUserRepository;
            _configuration = configuration;
            _userRepository = userRepository;
            _basePermissionsRepository = basePermissionsRepository;
            _baseRoleRepository = baseRoleRepository;
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
            var permissions = await _basePermissionsRepository.GetAllAsync(x => x.UserId == user.Id);
            var roles = await _baseRoleRepository.GetAllAsync();

            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.MobilePhone, user.PhoneNumber ?? ""),
                new Claim("UserId", user.Id.ToString()),
                new Claim("BrokerageId", user.BrokerageId?.ToString() ?? ""),
            };

            foreach (var permission in permissions)
            {
                foreach (var role in roles)
                {
                    if (role.Id == permission.RoleId)
                    {
                        authClaims.Add(new Claim("Permission", role.RoleName));
                    }
                }
            }

            var userRoles = await _userRepository.GetRolesOfUserAsync(user);
            foreach (var item in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, item));
            }

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
                    "Incorrect email.",
                    StatusCodes.Status400BadRequest
                );
            }

            bool checkPass = BCryptNet.Verify(request.Password, user.Password);
            if (!checkPass)
            {
                throw new ResponseErrorObject(
                    "Incorrect password.",
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
            
            var userRoles = await _userRepository.GetRolesOfUserAsync(user);

            return new LoginResult
            {
                User = new UserResult
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Avatar = user.Avatar,
                    DateOfBirth = user.DateOfBirth,
                    Email = user.Email,
                    FullName = user.FullName,
                    PhoneNumber = user.PhoneNumber,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt,
                    DeletedAt = user.DeletedAt,
                    Roles = userRoles.ToList()
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
                    DateOfBirth = request.DateOfBirth ?? DateTime.UtcNow,
                    PhoneNumber = request.PhoneNumber ?? "",
                    FullName = request.FullName,
                    Password = BCryptNet.HashPassword(request.Password),
                    UserName = request.UserName,
                    Email = request.Email,
                    HasOnboarded = true,
                    CreatedAt = DateTime.UtcNow,
                };

                user = await _baseUserRepository.CreateAsync(user);

                bool roleAdded = await _userRepository.AddRoleToUserAsync(user, new List<string> { "Administrator" });
            
                if (!roleAdded)
                {
                    throw new InvalidOperationException("Failed to assign user role.");
                }

                /*var confirmEmail = new ConfirmEmail
                {
                    IsActive = true,
                    ConfirmCode = GenerateCodeActive(),
                    ExpiryTime = DateTime.UtcNow.AddMinutes(10),
                    IsConfirmed = false,
                    UserId = user.Id
                };

                await _baseConfirmEmailRepository.CreateAsync(confirmEmail);

                var message = new EmailMessage(
                    new[] { request.Email },
                    "Confirm your account",
                    $"Your confirmation code is: {confirmEmail.ConfirmCode}"
                );

                _emailService.SendEmail(message);*/

                var userRoles = await _userRepository.GetRolesOfUserAsync(user);
                return new UserResult
                {
                    Id = user.Id,
                    Avatar = user.Avatar,
                    DateOfBirth = user.DateOfBirth,
                    FullName = user.FullName,
                    UserName = user.UserName,
                    PhoneNumber = user.PhoneNumber,
                    Email = user.Email,
                    Roles = userRoles.ToList()
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

            var user = await _baseUserRepository.BuildQueryable(new List<string> { "Brokerage" }, u => u.Id == userId.Value)
                .FirstOrDefaultAsync();
            if (user == null)
            {
                throw new ResponseErrorObject(
                    "User not found.",
                    StatusCodes.Status404NotFound
                );
            }

            var userRoles = await _userRepository.GetRolesOfUserAsync(user);

            return new UserResult
            {
                Id = user.Id,
                UserName = user.UserName,
                Avatar = user.Avatar,
                DateOfBirth = user.DateOfBirth,
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                BrokerageId = user.BrokerageId,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                DeletedAt = user.DeletedAt,
                Roles = userRoles.ToList()
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

                if(request.DateOfBirth == null)
                {
                    request.DateOfBirth = existingUser.DateOfBirth;
                }


                existingUser.FullName = request.FullName;
                existingUser.DateOfBirth = request.DateOfBirth;
                existingUser.Email = request.Email;
                existingUser.PhoneNumber = request.PhoneNumber;
                existingUser.UpdatedAt = DateTime.UtcNow;

                await _userRepository.UpdateAsync(existingUser);

                var roles = await _userRepository.GetRolesOfUserAsync(existingUser);

                var responseData =  new UserResult
                {
                    Id = existingUser.Id,
                    UserName = existingUser.UserName,
                    Avatar = existingUser.Avatar,
                    DateOfBirth = existingUser.DateOfBirth,
                    Email = existingUser.Email,
                    FullName = existingUser.FullName,
                    PhoneNumber = existingUser.PhoneNumber,
                    BrokerageId = existingUser.BrokerageId,
                    CreatedAt = existingUser.CreatedAt,
                    UpdatedAt = existingUser.UpdatedAt,
                    DeletedAt = existingUser.DeletedAt,
                    Roles = roles.ToList()
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
                var roles = await _userRepository.GetRolesOfUserAsync(user);
                var responseData = new UserResult
                {
                    Id = user.Id,
                    Avatar = user.Avatar,
                    DateOfBirth = user.DateOfBirth,
                    FullName = user.FullName,
                    UserName = user.UserName,
                    PhoneNumber = user.PhoneNumber,
                    Email = user.Email,
                    Roles = roles.ToList()
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
