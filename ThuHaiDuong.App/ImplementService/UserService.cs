using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using ThuHaiDuong.Domain.Entities;
using ThuHaiDuong.Domain.InterfaceRepositories;
using ThuHaiDuong.Application.InterfaceService;
using ThuHaiDuong.Application.Payloads.InputModels.User;
using ThuHaiDuong.Application.Payloads.Responses;
using ThuHaiDuong.Shared.Models;
using ThuHaiDuong.Application.Payloads.ResultModels.User;
using BCryptNet = BCrypt.Net.BCrypt;

namespace ThuHaiDuong.Application.ImplementService
{
    public class UserService : IUserService
    {
        private readonly IBaseRepository<User> _baseUserRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly Guid? _brokerageId;
        private IEnumerable<string> _currentUserRole;
        public UserService(
            IBaseRepository<User> baseUserRepository, 
            IUserRepository userRepository, 
            ICurrentUserService currentUserService)
        {
            _baseUserRepository = baseUserRepository;
            _userRepository = userRepository;
            _baseUserRepository = baseUserRepository;
            _currentUserService = currentUserService;
            var brokerageId = _currentUserService.GetBrokerageId();
            if (!brokerageId.HasValue)
            {
                throw new UnauthorizedAccessException("User is not associated with a brokerage");
            }
            _brokerageId = brokerageId.Value;
            _currentUserRole = _currentUserService.GetRoles();
        }

        public async Task<List<UserResult>> GetAllUserAsync()
        {
            var users = await _baseUserRepository
                .BuildQueryable(
                    new List<string> { "Brokerage" },
                    null
                )
                .Select(UserResult.FromUser)
                .ToListAsync();
    
            return users;
        }
        public async Task<PagedResult<UserResult>> GetListUsersAsync(UserQuery userQuery)
        {
            var query = _baseUserRepository.BuildQueryable(
                new List<string> { "Brokerage", "Permissions", "Permissions.Role"}, 
                null
            );
            
            if (!string.IsNullOrEmpty(userQuery.FullName))
            {
                var fullName = userQuery.FullName.ToLower();
                query = query.Where(u => u.FullName.ToLower().Contains(fullName));
            }
            
            if (!string.IsNullOrEmpty(userQuery.Email))
            {
                var email = userQuery.Email.ToLower();
                query = query.Where(u => u.Email != null && u.Email.ToLower().Contains(email));
            }
            
            if (!string.IsNullOrEmpty(userQuery.PhoneNumber))
            {
                var phone = userQuery.PhoneNumber.ToLower();
                query = query.Where(u => u.PhoneNumber.ToLower().Contains(phone));
            }
            
            var totalCount = await query.CountAsync();
            
            query = _baseUserRepository.ApplySorting(query, userQuery.SortBy, userQuery.SortDescending);
            
            var results = await query
                .Skip((userQuery.PageNumber - 1) * userQuery.PageSize)
                .Take(userQuery.PageSize)
                .Select(UserResult.FromUser)
                .ToListAsync();
            
            return new PagedResult<UserResult>(results, totalCount, userQuery.PageNumber, userQuery.PageSize);
        }

        public async Task<UserResult> GetUserByIdAsync(Guid id)
        {
            var user = await _baseUserRepository
                .BuildQueryable(
                    new List<string> { "Brokerage", "Permissions", "Permissions.Role"}, 
                    p => p.Id == id
                )
                .Select(UserResult.FromUser)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                throw new ResponseErrorObject("User not found", StatusCodes.Status404NotFound);
            }

            return user;
        }
        
        public async Task DeleteUserAsync(Guid id)
        {
            if (!_currentUserRole.Contains("Administrator"))
            {
                throw new ResponseErrorObject("Insufficient permissions", StatusCodes.Status403Forbidden);
            }
            
            var user = await GetUserByIdAsync(id);
            
            await _baseUserRepository.DeleteAsync(id);
        }

        public async Task<UserResult> CreateUserAsync(CreateUserInput request)
        {
            if (!_currentUserRole.Contains("Administrator"))
            {
                throw new ResponseErrorObject("Insufficient permissions", StatusCodes.Status403Forbidden);
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
            
            return await GetUserByIdAsync(user.Id);
        }

        public async Task<UserResult> UpdateUserAsync(Guid userId, UpdateUserInput request)
        {
            if (!_currentUserRole.Contains("Administrator"))
            {
                throw new ResponseErrorObject("Insufficient permissions", StatusCodes.Status403Forbidden);
            }
            
            var user = await _baseUserRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new ResponseErrorObject("User not found", StatusCodes.Status404NotFound);
            }

            if (!string.IsNullOrEmpty(request.Email) && request.Email != user.Email)
            {
                var emailExists = await _baseUserRepository.GetAsync(p => p.Email == request.Email && p.Id != userId);
                if (emailExists != null)
                {
                    throw new ResponseErrorObject("Email already exists", StatusCodes.Status400BadRequest);
                }
            }

            bool isChangingPassword = !string.IsNullOrWhiteSpace(request.NewPassword);

            if (isChangingPassword)
            {
                if (string.IsNullOrWhiteSpace(request.CurrentPassword))
                {
                    throw new ResponseErrorObject("Current password is required to set a new password", StatusCodes.Status400BadRequest);
                }

                bool isOldPassValid = BCryptNet.Verify(request.CurrentPassword, user.Password);
                if (!isOldPassValid)
                {
                    throw new ResponseErrorObject("Incorrect current password", StatusCodes.Status400BadRequest);
                }

                if (request.NewPassword != request.ConfirmPassword)
                {
                    throw new ResponseErrorObject("New passwords do not match", StatusCodes.Status400BadRequest);
                }

                user.Password = BCryptNet.HashPassword(request.NewPassword);
            }
            
            user.FullName = request.FullName;
            user.Email = request.Email;
            user.PhoneNumber = request.PhoneNumber;
            user.UpdatedAt = DateTime.UtcNow;

            await _baseUserRepository.UpdateAsync(user);

            return await GetUserByIdAsync(user.Id);
        }
    }
}
