using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MyApiSecurity.Helper;
using MyApiSecurity.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MyApiSecurity.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly JWT _jwt;

        public AuthService(UserManager<ApplicationUser> userManager, IOptions<JWT> jwt, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _jwt = jwt.Value;
            _roleManager = roleManager;
        }

        public async Task<UsersModel> GetAllUsers()
        {
            var usermodel = new UsersModel();
            var users =  await _userManager.Users.ToListAsync();
            foreach (var user in users)
            {         
                usermodel.FirstName = user.FirstName;
                usermodel.LastName = user.LastName;
                usermodel.Email = user.Email;
                usermodel.UserName = user.UserName;
            }

            return usermodel;
            
        }

        public async Task<string> AddRoleAsync(AddRoleModel model)
        {
            var user = await _userManager.FindByIdAsync(model.userId);
            if (user is null || !await _roleManager.RoleExistsAsync(model.roleName))
                return "user id or Role Name Invalid";

            if (await _userManager.IsInRoleAsync(user, model.roleName))
                return "User Alredy Assigned in This Role";

            var reslut = await _userManager.AddToRoleAsync(user, model.roleName);

            return reslut.Succeeded ? String.Empty : "SomeThing Wrong";
            
        }

        public async Task<AuthModel> RegisterAsync(RegisterModel model)
        {
            if (await _userManager.FindByEmailAsync(model.Email) != null)
                return new AuthModel { Message = "Email is Alredy in Use" };

            if (await _userManager.FindByNameAsync(model.UserName) != null)
                return new AuthModel { Message = "User Name is Alredy in User" };

            var user = new ApplicationUser
            {
                UserName = model.UserName,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
            };

            var result = await _userManager.CreateAsync(user,model.Password);

            if(!result.Succeeded)
            {
                var errors = string.Empty;
                foreach (var error in result.Errors)
                {
                    errors += $"{error.Description} , ";
                }
                return new AuthModel { Message = errors };
            }

            await _userManager.AddToRoleAsync(user, "User");

            var jwtSecurityToken = await CreateJwtToken(user);
            return new AuthModel
            {
                Email = user.Email,
                UserName = user.UserName,
                ExpiresOn = jwtSecurityToken.ValidTo,
                isAuthentcated = true,
                Roles = new List<string> { "User" },
                Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
            };
        }

        public async Task<AuthModel> GetTokenAsync(TokenRequistModel model)
        {
            var authemodel = new AuthModel();
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
            {
                authemodel.Message = "Email Or Password Invalid Please Try Again";
                return authemodel;
            }

            var jwtSecurityToken = await CreateJwtToken(user);
            var roles = await _userManager.GetRolesAsync(user);

            authemodel.isAuthentcated = true;
            authemodel.Email = user.Email;
            authemodel.UserName = user.UserName;
            authemodel.Roles = roles.ToList();
            authemodel.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            authemodel.ExpiresOn = jwtSecurityToken.ValidTo;
            
            return authemodel;
        }

        private async Task<JwtSecurityToken> CreateJwtToken(ApplicationUser user)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);
            var roleClamis = new List<Claim>();

            foreach (var role in roles)
            {
                roleClamis.Add(new Claim("roles", role));
            }

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("uid", user.Id)
            }
            .Union(roleClamis)
            .Union(userClaims);

            var symmterSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.key));
            var signingCredentials = new SigningCredentials(symmterSecurityKey, SecurityAlgorithms.HmacSha256);

            var jwtSecurityToken = new JwtSecurityToken
                (
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(_jwt.DurationInMinute),
                signingCredentials: signingCredentials
                );
            return jwtSecurityToken;
        }
    }
}
