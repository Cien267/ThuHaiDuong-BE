using ThuHaiDuong.Domain.Entities;
using ThuHaiDuong.Domain.InterfaceRepositories;
using Microsoft.EntityFrameworkCore;
using ThuHaiDuong.Infrastructure.DataContext;

namespace ThuHaiDuong.Infrastructure.ImplementRepositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        #region
        private Task<bool> CompareStringAsync(string str1, string str2)
        {
            return Task.FromResult(string.Equals(str1.ToLowerInvariant(), str2.ToLowerInvariant()));
        }

        private async Task<bool> IsStringInListAsync(string inputString, List<string> listString)
        {
            if (inputString == null)
            {
                throw new ArgumentNullException(nameof(inputString));
            }
            if (listString == null)
            {
                throw new ArgumentNullException(nameof(inputString));
            }
            foreach (var item in listString)
            {
                if (await CompareStringAsync(inputString, item))
                {
                    return true;
                }
            }
            return false;
        }
        #endregion

        public async Task<bool> AddRoleToUserAsync(User user, List<string> listRoles)
        {
            if (user == null)
            {
                return false;
            }

            if (listRoles == null || !listRoles.Any())
            {
                return false;
            }

            var distinctRoles = listRoles.Distinct().ToList();
    
            var roleItems = await _context.Roles
                .Where(r => distinctRoles.Contains(r.RoleCode))
                .ToListAsync();

            var missingRoles = distinctRoles.Except(roleItems.Select(r => r.RoleCode)).ToList();
            if (missingRoles.Any())
            {
                throw new InvalidOperationException($"The following roles do not exist: {string.Join(", ", missingRoles)}");
            }

            await _context.Permissions
                .Where(p => p.UserId == user.Id)
                .ExecuteDeleteAsync();

            var newPermissions = roleItems
                .Select(role => new Permissions
                {
                    RoleId = role.Id,
                    UserId = user.Id
                })
                .ToList();

            await _context.Permissions.AddRangeAsync(newPermissions);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<string>> GetRolesOfUserAsync(User user)
        {
            var roles = new List<string>();

            var listRoles = await _context.Permissions
                                          .Where(x => x.UserId == user.Id)
                                          .Select(x => x.RoleId)
                                          .Distinct()
                                          .ToListAsync();

            var rolesData = await _context.Roles
                                          .Where(r => listRoles.Contains(r.Id))
                                          .ToListAsync();

            roles.AddRange(rolesData.Select(r => r.RoleCode));

            return roles.AsEnumerable();
        }
        
        public async Task RevokeRefreshTokensAsync(Guid userId)
        {
            var tokens = await _context.RefreshTokens
                .Where(x => x.UserId == userId)
                .ToListAsync();

            foreach (var token in tokens)
            {
                token.IsRevoked = true;
                token.RevokedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }


        public async Task<User> GetUserByEmail(string email)
        {
            var user = await _context.Users.AsNoTracking().SingleOrDefaultAsync(x =>x.Email.ToLower().Equals(email.ToLower()));
            return user;
        }

        public async Task<User> GetUserByUsername(string username)
        {
            var user = await _context.Users.SingleOrDefaultAsync(x => x.UserName.Equals(username)); 
            return user;
        }

        public async Task<User> UpdateAsync(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            _context.Users.Attach(user);

            _context.Entry(user).State = EntityState.Modified;

            await _context.SaveChangesAsync();
            return user;
        }
    }
}
