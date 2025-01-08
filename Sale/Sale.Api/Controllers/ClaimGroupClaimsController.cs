using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Sale.Model.Base;
using Microsoft.EntityFrameworkCore;

namespace Sale.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class ClaimGroupClaimsController(RoleManager<IdentityRole> roleManager) : ControllerBase
    {
        [HttpGet]
        [Route("GetByClaimGroupName/{claimGroupName}")]
        public async Task<IActionResult> GetByUserId(string claimGroupName) {
            try {
                var role = await roleManager.Roles.FirstOrDefaultAsync(e => e.Name == claimGroupName);
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
        public async Task<IActionResult> Post(string claimGroupName, List<ClaimModel> requestModel) {
            try {
                if (string.IsNullOrWhiteSpace(claimGroupName))
                    return BadRequest("Invalid claim group name submitted");

                var role = await roleManager.FindByNameAsync(claimGroupName);
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
        public async Task<IActionResult> Put(string claimGroupName, List<ClaimModel> requestModel) {
            try {
                if (string.IsNullOrWhiteSpace(claimGroupName))
                    return BadRequest("Invalid claim group name submitted");

                var errors = new StringBuilder();

                var role = await roleManager.FindByNameAsync(claimGroupName);
                if (role == null)
                    return BadRequest();

                var existingClaims = await roleManager.GetClaimsAsync(role);
                foreach (var existClaim in existingClaims) {
                    var result = await roleManager.RemoveClaimAsync(role, existClaim);
                    if (!result.Succeeded) {
                        errors.Append(string.Join(Environment.NewLine, result.Errors.Select(e => e.Description)));
                    }
                }

                if (errors.Length > 0)
                    return BadRequest(new { message = errors.ToString() });


                var newClaims = requestModel.Select(e => new Claim(e.ClaimType, e.ClaimValue));
                foreach (var claim in newClaims) {
                    var result = await roleManager.AddClaimAsync(role, claim);
                    if (!result.Succeeded) {
                        errors.Append(string.Join(Environment.NewLine, result.Errors.Select(e => e.Description)));
                    }
                }

                if (errors.Length > 0)
                    return BadRequest(new { message = errors.ToString() });

                return Ok();
            } catch (Exception ex) {
                return BadRequest(new { message = ex.GetBaseException().Message });
            }
        }

        [HttpDelete]
        [Route("{claimGroupName}")]
        public async Task<IActionResult> Delete(string claimGroupName) {
            try {
                var role = await roleManager.FindByNameAsync(claimGroupName);
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
