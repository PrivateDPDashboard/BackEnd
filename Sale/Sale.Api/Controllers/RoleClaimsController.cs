using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Sale.Database.Entities;
using Sale.Api.ApiModel.User.UserClaim;
using Sale.Model.Base;
using Microsoft.EntityFrameworkCore;
using Sale.Api.ApiModel.Role.RoleClaim;

namespace Sale.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class RoleClaimsController(RoleManager<IdentityRole> roleManager) : ControllerBase
    {
        [HttpGet]
        [Route("GetByRoleName/{roleName}")]
        public async Task<IActionResult> GetByUserId(string roleName) {
            try {
                var role = await roleManager.Roles.FirstOrDefaultAsync(e => e.Name == roleName);
                if (role == null)
                    return BadRequest();

                var claims = (await roleManager.GetClaimsAsync(role)).
                    Select(e => new ClaimModel(e.Type, e.Value));
                return Ok(claims);
            } catch (Exception ex) {
                return BadRequest(new { message = ex.GetBaseException().Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post(List<RoleClaimCreateRequestModel> requestModel) {
            try {
                var roleName = requestModel.FirstOrDefault()?.RoleName;
                if (string.IsNullOrWhiteSpace(roleName))
                    return BadRequest("Invalid RoleName submitted");

                var role = await roleManager.FindByNameAsync(roleName);
                if (role == null)
                    return BadRequest();

                var claims = requestModel.Select(e => new Claim(e.ClaimType, e.ClaimValue));

                foreach (var claim in claims) {
                    await roleManager.AddClaimAsync(role, claim);
                }
                return Ok(claims);
            } catch (Exception ex) {
                return BadRequest(new { message = ex.GetBaseException().Message });
            }
        }

        [HttpPut]
        public async Task<IActionResult> Put(List<RoleClaimCreateRequestModel> requestModel) {
            try {
                var roleName = requestModel.FirstOrDefault()?.RoleName;
                if (string.IsNullOrWhiteSpace(roleName))
                    return BadRequest("Invalid RoleName submitted");

                var role = await roleManager.FindByNameAsync(roleName);
                if (role == null)
                    return BadRequest();

                var existingClaims = await roleManager.GetClaimsAsync(role);
                foreach (var existClaim in existingClaims) {
                    await roleManager.RemoveClaimAsync(role, existClaim);
                }

                var newClaims = requestModel.Select(e => new Claim(e.ClaimType, e.ClaimValue));
                foreach (var claim in newClaims) {
                    var result = await roleManager.AddClaimAsync(role, claim);
                }
                return Ok();

                //return BadRequest(new { message = string.Join(Environment.NewLine, result.Errors.Select(e => e.Description)) });
            } catch (Exception ex) {
                return BadRequest(new { message = ex.GetBaseException().Message });
            }
        }

        [HttpDelete]
        [Route("{roleName}")]
        public async Task<IActionResult> Delete(string roleName) {
            try {
                var role = await roleManager.FindByNameAsync(roleName);
                if (role == null)
                    return BadRequest();

                var existingClaims = await roleManager.GetClaimsAsync(role);
                foreach (var existingClaim in existingClaims) {
                    await roleManager.RemoveClaimAsync(role, existingClaim);
                }

                return Ok();
            } catch (Exception ex) {
                return BadRequest(new { message = ex.GetBaseException().Message });
            }
        }
    }
}
