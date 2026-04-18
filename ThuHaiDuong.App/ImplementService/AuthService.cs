using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using ThuHaiDuong.Domain.Entities;
using ThuHaiDuong.Domain.InterfaceRepositories;
using System.Text.RegularExpressions;
using ThuHaiDuong.Application.InterfaceService;
using ThuHaiDuong.Application.Payloads.Responses;
using ThuHaiDuong.Application.Payloads.InputModels.Auth;
using ThuHaiDuong.Application.Payloads.ResultModels.Auth;

namespace ThuHaiDuong.Application.ImplementService;

public class AuthService : IAuthService
{
    private readonly IBaseRepository<User>         _userRepo;
    private readonly IBaseRepository<RefreshToken> _tokenRepo;
    private readonly IAuthRepository               _authRepo;
    private readonly IJwtService                   _jwtService;
    private readonly IGoogleAuthService            _googleAuth;
    private readonly IConfiguration                _config;
 
    public AuthService(
        IBaseRepository<User>         userRepo,
        IBaseRepository<RefreshToken> tokenRepo,
        IAuthRepository               authRepo,
        IJwtService                   jwtService,
        IGoogleAuthService            googleAuth,
        IConfiguration                config)
    {
        _userRepo   = userRepo;
        _tokenRepo  = tokenRepo;
        _authRepo   = authRepo;
        _jwtService = jwtService;
        _googleAuth = googleAuth;
        _config     = config;
    }
 
    // ── REGISTER (Reader only) ────────────────────────────────────────────────
 
    public async Task<AuthResult> RegisterAsync(
        RegisterInput input, string? ipAddress, string? userAgent)
    {
        if (await _authRepo.EmailExistsAsync(input.Email))
            throw new ResponseErrorObject(
                "Email is already in use.", StatusCodes.Status409Conflict);
 
        if (await _authRepo.UserNameExistsAsync(input.UserName))
            throw new ResponseErrorObject(
                "Username is already taken.", StatusCodes.Status409Conflict);
 
        var user = new User
        {
            UserName = input.UserName.Trim(),
            Email    = input.Email.ToLower().Trim(),
            Password = BCrypt.Net.BCrypt.HashPassword(input.Password),
            FullName = input.FullName?.Trim(),
            Role     = "Reader",
            IsActive = true,
        };
 
        await _userRepo.CreateAsync(user);
 
        return await IssueTokensAsync(user, ipAddress, userAgent);
    }
 
    // ── LOGIN CLIENT (Reader only) ────────────────────────────────────────────
 
    public async Task<AuthResult> ClientLoginAsync(
        LoginInput input, string? ipAddress, string? userAgent)
    {
        var user = await _authRepo.GetByEmailAsync(input.Email)
            ?? throw new ResponseErrorObject(
                "Invalid email or password.", StatusCodes.Status401Unauthorized);
 
        // Chặn Staff cố login vào client portal
        if (user.Role != "Reader")
            throw new ResponseErrorObject(
                "Please use the admin portal to login.",
                StatusCodes.Status403Forbidden);
 
        if (!user.IsActive)
            throw new ResponseErrorObject(
                "Account is disabled. Please contact support.",
                StatusCodes.Status403Forbidden);
 
        if (!BCrypt.Net.BCrypt.Verify(input.Password, user.Password))
            throw new ResponseErrorObject(
                "Invalid email or password.", StatusCodes.Status401Unauthorized);
 
        user.LastLoginAt = DateTime.UtcNow;
        await _userRepo.UpdateAsync(user);
 
        return await IssueTokensAsync(user, ipAddress, userAgent);
    }
 
    // ── LOGIN ADMIN (Staff only) ──────────────────────────────────────────────
 
    public async Task<AuthResult> AdminLoginAsync(
        LoginInput input, string? ipAddress, string? userAgent)
    {
        var user = await _authRepo.GetByEmailAsync(input.Email)
            ?? throw new ResponseErrorObject(
                "Invalid email or password.", StatusCodes.Status401Unauthorized);
 
        // Chặn Reader cố login vào admin portal
        if (user.Role == "Reader")
            throw new ResponseErrorObject(
                "You do not have permission to access the admin portal.",
                StatusCodes.Status403Forbidden);
 
        if (!user.IsActive)
            throw new ResponseErrorObject(
                "Account is disabled. Please contact your administrator.",
                StatusCodes.Status403Forbidden);
 
        if (!BCrypt.Net.BCrypt.Verify(input.Password, user.Password))
            throw new ResponseErrorObject(
                "Invalid email or password.", StatusCodes.Status401Unauthorized);
 
        user.LastLoginAt = DateTime.UtcNow;
        await _userRepo.UpdateAsync(user);
 
        return await IssueTokensAsync(user, ipAddress, userAgent);
    }
 
    // ── GOOGLE LOGIN (Reader only) ────────────────────────────────────────────
 
    public async Task<AuthResult> GoogleLoginAsync(
        GoogleLoginInput input, string? ipAddress, string? userAgent)
    {
        var googleUser = await _googleAuth.VerifyIdTokenAsync(input.IdToken);
 
        var user = await _authRepo.GetByEmailAsync(googleUser.Email);
 
        if (user == null)
        {
            // Tự động tạo account mới cho Reader khi lần đầu Google login
            var userName = await GenerateUniqueUserNameAsync(googleUser.Email);
 
            user = new User
            {
                UserName    = userName,
                Email       = googleUser.Email.ToLower().Trim(),
                Password    = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()),
                // Password random — user này chỉ login bằng Google
                // Nếu sau này muốn đổi sang password thì dùng "Forgot Password"
                FullName    = googleUser.Name,
                Avatar      = googleUser.Picture,
                Role        = "Reader",
                IsActive    = true,
                LastLoginAt = DateTime.UtcNow,
            };
 
            await _userRepo.CreateAsync(user);
        }
        else
        {
            // Account đã tồn tại → kiểm tra role
            // Không cho staff (Admin/Contributor) login Google ở client portal
            if (user.Role is not "Reader")
                throw new ResponseErrorObject(
                    "Staff accounts must use email and password to login.",
                    StatusCodes.Status403Forbidden);
 
            if (!user.IsActive)
                throw new ResponseErrorObject(
                    "Account is disabled.", StatusCodes.Status403Forbidden);
 
            // Sync avatar nếu Google cập nhật ảnh mới
            if (googleUser.Picture != null && user.Avatar != googleUser.Picture)
            {
                user.Avatar      = googleUser.Picture;
                user.LastLoginAt = DateTime.UtcNow;
                await _userRepo.UpdateAsync(user);
            }
            else
            {
                user.LastLoginAt = DateTime.UtcNow;
                await _userRepo.UpdateAsync(user);
            }
        }
 
        return await IssueTokensAsync(user, ipAddress, userAgent);
    }
 
    // ── CREATE STAFF (SuperAdmin only) ───────────────────────────────────────
 
    public async Task<UserAuthInfo> CreateStaffAsync(CreateStaffInput input)
    {
        // Validate role — không cho tạo SuperAdmin qua API
        if (input.Role is not ("Contributor" or "Admin"))
            throw new ResponseErrorObject(
                "Role must be 'Contributor' or 'Admin'.",
                StatusCodes.Status400BadRequest);
 
        if (await _authRepo.EmailExistsAsync(input.Email))
            throw new ResponseErrorObject(
                "Email is already in use.", StatusCodes.Status409Conflict);
 
        if (await _authRepo.UserNameExistsAsync(input.UserName))
            throw new ResponseErrorObject(
                "Username is already taken.", StatusCodes.Status409Conflict);
 
        var user = new User
        {
            UserName = input.UserName.Trim(),
            Email    = input.Email.ToLower().Trim(),
            Password = BCrypt.Net.BCrypt.HashPassword(input.Password),
            FullName = input.FullName?.Trim(),
            Role     = input.Role,
            IsActive = true,
        };
 
        await _userRepo.CreateAsync(user);
 
        return ToAuthInfo(user);
    }
 
    // ── REFRESH TOKEN ─────────────────────────────────────────────────────────
 
    public async Task<AuthResult> RefreshTokenAsync(
        RefreshTokenInput input, string? ipAddress, string? userAgent)
    {
        var existing = await _authRepo.GetByTokenValueAsync(input.RefreshToken)
            ?? throw new ResponseErrorObject(
                "Invalid refresh token.", StatusCodes.Status401Unauthorized);
 
        if (existing.IsRevoked)
            throw new ResponseErrorObject(
                "Refresh token has been revoked.", StatusCodes.Status401Unauthorized);
 
        if (existing.ExpiresAt <= DateTime.UtcNow)
            throw new ResponseErrorObject(
                "Refresh token has expired. Please login again.",
                StatusCodes.Status401Unauthorized);
 
        if (!existing.User.IsActive)
            throw new ResponseErrorObject(
                "Account is disabled.", StatusCodes.Status403Forbidden);
 
        return await IssueTokensAsync(existing.User, ipAddress, userAgent);
    }
 
    // ── LOGOUT ────────────────────────────────────────────────────────────────
 
    public async Task LogoutAsync(Guid userId)
    {
        await _authRepo.RevokeAllUserTokensAsync(userId);
    }
 
    // ── CHANGE PASSWORD ───────────────────────────────────────────────────────
 
    public async Task ChangePasswordAsync(Guid userId, ChangePasswordInput input)
    {
        var user = await _userRepo.GetByIdAsync(userId)
            ?? throw new ResponseErrorObject(
                "User not found.", StatusCodes.Status404NotFound);
 
        if (!BCrypt.Net.BCrypt.Verify(input.CurrentPassword, user.Password))
            throw new ResponseErrorObject(
                "Current password is incorrect.", StatusCodes.Status400BadRequest);
 
        user.Password = BCrypt.Net.BCrypt.HashPassword(input.NewPassword);
        await _userRepo.UpdateAsync(user);
 
        // Revoke tất cả refresh token → buộc login lại trên tất cả thiết bị
        await _authRepo.RevokeAllUserTokensAsync(userId);
    }
 
    // ── PRIVATE HELPERS ───────────────────────────────────────────────────────
 
    // Revoke token cũ → tạo access token + refresh token mới
    private async Task<AuthResult> IssueTokensAsync(
        User user, string? ipAddress, string? userAgent)
    {
        // Revoke tất cả token cũ (1 user = 1 refresh token)
        await _authRepo.RevokeAllUserTokensAsync(user.Id);
 
        var accessToken      = _jwtService.GenerateAccessToken(user);
        var refreshTokenValue = _jwtService.GenerateRefreshToken();
 
        var refreshExpiryDays = int.Parse(
            _config["Jwt:RefreshTokenExpiryDays"] ?? "30");
 
        var refreshToken = new RefreshToken
        {
            UserId    = user.Id,
            Token     = refreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(refreshExpiryDays),
        };
 
        await _tokenRepo.CreateAsync(refreshToken);
 
        var accessExpiryMinutes = int.Parse(
            _config["Jwt:AccessTokenExpiryMinutes"] ?? "60");
 
        return new AuthResult
        {
            AccessToken          = accessToken,
            RefreshToken         = refreshTokenValue,
            AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(accessExpiryMinutes),
            User                 = ToAuthInfo(user),
        };
    }
 
    // Generate username từ email prefix, thêm random suffix nếu đã tồn tại
    private async Task<string> GenerateUniqueUserNameAsync(string email)
    {
        var baseUserName = email.Split('@')[0]
            .Replace(".", "_")
            .Replace("+", "_")
            .ToLower();
 
        baseUserName = Regex.Replace(baseUserName, @"[^a-z0-9_]", "");
        baseUserName = baseUserName[..Math.Min(baseUserName.Length, 80)];
 
        var candidate = baseUserName;
        var attempts  = 0;
 
        while (await _authRepo.UserNameExistsAsync(candidate))
        {
            attempts++;
            candidate = $"{baseUserName}_{Random.Shared.Next(1000, 9999)}";
 
            if (attempts > 10)
                candidate = $"user_{Guid.NewGuid():N}"[..20];
        }
 
        return candidate;
    }
 
    private static UserAuthInfo ToAuthInfo(User user) => new()
    {
        Id       = user.Id,
        UserName = user.UserName,
        Email    = user.Email,
        FullName = user.FullName,
        Avatar   = user.Avatar,
        Role     = user.Role,
    };
}
