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
using Sale.Security;

namespace Sale.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class UserClaimsController(UserManager<ApplicationUser> userManager) : ControllerBase
    {
        [HttpGet]
        [Route("GetByUserId/{userId}")]
        [Authorize(Policy = Policies.ManageUsersPolicy)]
        public async Task<IActionResult> GetByUserId(string userId) {
            try {
                var user = await userManager.FindByIdAsync(userId);
                if (user == null)
                    return BadRequest();

                var userClaims = (await userManager.GetClaimsAsync(user)).Select(e => new ClaimModel(e.Type, e.Value));
                return Ok(userClaims);
            } catch (Exception ex) {
                return BadRequest(new { message = ex.GetBaseException().Message });
            }
        }

        [HttpPost]
        [Authorize(Policy = Policies.ManageUsersPolicy)]
        public async Task<IActionResult> Post(List<UserClaimCreateRequestModel> requestModel) {
            try {
                var userId = requestModel.FirstOrDefault()?.UserId;
                if (string.IsNullOrWhiteSpace(userId))
                    return BadRequest("Invalid User Id submitted");

                var user = await userManager.FindByIdAsync(userId);
                if (user == null)
                    return BadRequest();

                var claims = requestModel.Select(e => new Claim(e.ClaimType, e.ClaimValue));

                var userClaims = await userManager.AddClaimsAsync(user, claims);
                return Ok(userClaims);
            } catch (Exception ex) {
                return BadRequest(new { message = ex.GetBaseException().Message });
            }
        }

        [HttpPut]
        [Authorize(Policy = Policies.ManageUsersPolicy)]
        public async Task<IActionResult> Put(List<UserClaimModifyRequestModel> requestModel) {
            try {
                var userId = requestModel.FirstOrDefault()?.UserId;
                if (string.IsNullOrWhiteSpace(userId))
                    return BadRequest("Invalid User Id submitted");

                var user = await userManager.FindByIdAsync(userId);
                if (user == null)
                    return BadRequest();

                var existingClaims = await userManager.GetClaimsAsync(user);
                await userManager.RemoveClaimsAsync(user, existingClaims);

                var newClaims = requestModel.Select(e => new Claim(e.ClaimType, e.ClaimValue));
                var result = await userManager.AddClaimsAsync(user, newClaims);
                if (result.Succeeded)
                    return Ok();

                return BadRequest(new { message = string.Join(Environment.NewLine, result.Errors.Select(e => e.Description)) });
            } catch (Exception ex) {
                return BadRequest(new { message = ex.GetBaseException().Message });
            }
        }

        [HttpDelete]
        [Route("{userId}")]
        [Authorize(Policy = Policies.ManageUsersPolicy)]
        public async Task<IActionResult> Delete(string userId) {
            try {
                var user = await userManager.FindByIdAsync(userId);
                if (user == null)
                    return BadRequest();

                var existingClaims = await userManager.GetClaimsAsync(user);
                await userManager.RemoveClaimsAsync(user, existingClaims);

                return Ok();
            } catch (Exception ex) {
                return BadRequest(new { message = ex.GetBaseException().Message });
            }
        }
    }
}
