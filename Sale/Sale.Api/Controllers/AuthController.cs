using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Linq;
using Sale.Database.Entities;
using Microsoft.Extensions.Configuration;
using Sale.Api.ApiModel.User;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace Sale.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController(UserManager<ApplicationUser> userManager, IConfiguration configuration) : ControllerBase
    {
        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequestModel userLoginRequestModel) {
            try {
                var identityUser = await userManager.FindByNameAsync(userLoginRequestModel.UserName);
                if (identityUser == null)
                    return BadRequest(new { message = "Login failed" });

                if (identityUser.PasswordHash == null)
                    return BadRequest(new { message = "Invalid user attempted to login" });

                var result = userManager.PasswordHasher.VerifyHashedPassword(identityUser, identityUser.PasswordHash, userLoginRequestModel.Password);
                if (result == PasswordVerificationResult.Failed)
                    return BadRequest(new { message = "Login failed" });

                var userClaims = await userManager.GetClaimsAsync(identityUser);

                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, identityUser.Id),
                    new(ClaimTypes.Name, userLoginRequestModel.UserName),
                };
                claims.AddRange(userClaims.Select(claim => new Claim(claim.Type, claim.Value)));

                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"] ?? ""));
                var token = new JwtSecurityToken(
                    issuer: configuration["JWT:ValidIssuer"],
                    audience: configuration["JWT:ValidAudience"],
                    expires: DateTime.UtcNow.AddMinutes(1),
                    claims: claims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );


                var generatedToken = new JwtSecurityTokenHandler().WriteToken(token);

                var cookieOptions = new CookieOptions {
                    HttpOnly = false,
                    Secure = true,
                    SameSite = SameSiteMode.None
                };

                Response.Cookies.Append("token", generatedToken, cookieOptions);

                return Ok(new {
                    claims = JsonSerializer.Serialize(claims.Select(e => new { type = e.Type, value = e.Value })),
                    userId = identityUser.Id,
                    userName = userLoginRequestModel.UserName
                });
            } catch (Exception ex) {
                return BadRequest(new { message = ex.GetBaseException().Message });
            }
        }


        [HttpPost]
        [Authorize]
        [Route("Logout")]
        public IActionResult Logout() {
            try {
                var cookieOptions = new CookieOptions {
                    HttpOnly = false,
                    Secure = true,
                    SameSite = SameSiteMode.None
                };

                Response.Cookies.Delete("token", cookieOptions);
                return Unauthorized();
            } catch (Exception ex) {
                return BadRequest(new { message = ex.GetBaseException().Message });
            }
        }
    }
}
