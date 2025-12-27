using System.Security.Claims;
using VotingSystem.Abstraction.Consts;
using VotingSystem.Repositeryes.Interfaces;
using VotingSystem.Services.Interfaces;

namespace VotingSystem.Services.Implementation
{
    public class TokenClaimsBuilder : ITokenClaimsBuilder
    {
        private readonly IUserRepository _userRepository;
        private readonly ApplicationDbContext _context;

        public TokenClaimsBuilder(IUserRepository userRepository, ApplicationDbContext context)
        {
            _userRepository = userRepository;
            _context = context;
        }

        public async Task<List<Claim>> BuildClaimsAsync(Application_User user)
        {
            var roles = await _userRepository.GetRolesAsync(user);

            // Get permissions from roles
            var rolePermissions = await _context.Roles
                .Join(_context.RoleClaims,
                    role => role.Id,
                    claim => claim.RoleId,
                    (r, c) => new { r.Name, c.ClaimValue })
                .Where(x => roles.Contains(x.Name!))
                .Select(x => x.ClaimValue)
                .Distinct()
                .ToListAsync();

            // Get permissions directly assigned to user (UserClaims)
            var userPermissions = await _context.UserClaims
                .Where(x => x.UserId == user.Id && x.ClaimType == Permissions.Type)
                .Select(x => x.ClaimValue)
                .Distinct()
                .ToListAsync();

            // Combine both role permissions and direct user permissions
            var allPermissions = rolePermissions.Union(userPermissions).Distinct().ToList();

            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id!),
            new Claim(ClaimTypes.Email, user.Email ?? ""),
            new Claim(ClaimTypes.Name, user.UserName ?? ""),
            new Claim(ClaimTypes.GivenName, user.FristName ?? "")
        };

            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            foreach (var permission in allPermissions)
            {
                if (!string.IsNullOrEmpty(permission))
                    claims.Add(new Claim(Permissions.Type, permission));
            }

            return claims;
        }
    }

}
