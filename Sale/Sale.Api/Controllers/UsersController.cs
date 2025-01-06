using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;
using System;
using System.Linq;
using Sale.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Sale.Api.ApiModel.User;
using Sale.Model.Base;
using System.Collections.Generic;
using CoffeeCCode.Models.DataTables;
using Microsoft.AspNetCore.Authorization;
using System.Text;
using Sale.Security;

namespace Sale.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController(UserManager<ApplicationUser> userManager) : ControllerBase
    {
        [HttpPost("GetUsers")]
        [Authorize(Policy = Policies.ManageUsersPolicy)]
        public async Task<IActionResult> GetUsers(UserGetRequestModel requestModel)
        {
            try
            {
                var dataTableParamResult = new DataTableParamResult(requestModel.DataTableParam);

                var query = userManager.Users.AsNoTracking();

                var totalRecord = query.Count();

                if (!string.IsNullOrWhiteSpace(dataTableParamResult.SearchValue))
                {
                    query = query.Where(e =>
                        e.UserName.Contains(dataTableParamResult.SearchValue) ||
                        e.Email.Contains(dataTableParamResult.SearchValue)
                    );
                }
                var filteredRecord = query.Count();
                switch (dataTableParamResult.SortColumn)
                {
                    case 1:
                        query = dataTableParamResult.SortDirection == "asc" ? query.OrderBy(e => e.UserName) : query.OrderByDescending(e => e.UserName);
                        break;
                    case 2:
                        query = dataTableParamResult.SortDirection == "asc" ? query.OrderBy(e => e.Email) : query.OrderByDescending(e => e.Email);
                        break;
                    default:
                        query = query.OrderBy(e => e.UserName);
                        break;
                }

                var users = await query.Skip(dataTableParamResult.Skip).Take(dataTableParamResult.PageSize)
                    .Select(e => new ApplicationUserModel
                    {
                        Id = e.Id,
                        UserName = e.UserName,
                        Email = e.Email,
                        RoleName = "Admin"
                    }).ToListAsync();

                return Ok(new DataTableResult<List<ApplicationUserModel>>
                {
                    data = users,
                    recordsFiltered = filteredRecord,
                    recordsTotal = totalRecord
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.GetBaseException().Message });
            }
        }

        [HttpGet("GetById/{id}")]
        [Authorize(Policy = Policies.ManageUsersPolicy)]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                var user = await userManager.FindByIdAsync(id);
                if (user == null)
                    return BadRequest(new { message = $"Invalid user Id {id}" });

                return Ok(new ApplicationUserModel()
                {
                    Id = user.Id,
                    Email = user.Email,
                    UserName = user.UserName
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.GetBaseException().Message });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("Register")]
        public async Task<IActionResult> Register([FromForm] UserRegisterRequestModel userRegisterRequestModel)
        {
            try
            {
                if (userRegisterRequestModel == null)
                    return BadRequest(new { message = "Invalid Registration" });

                var identityUser = new ApplicationUser
                {
                    UserName = userRegisterRequestModel.UserName,
                    Email = userRegisterRequestModel.Email
                };

                var result = await userManager.CreateAsync(identityUser, userRegisterRequestModel.Password);
                if (result.Succeeded)
                {
                    return Ok(new { message = "User Registration Successful" });
                }

                var errorMessage = new StringBuilder();
                foreach (IdentityError error in result.Errors)
                {
                    errorMessage.Append(error.Description);
                    errorMessage.Append("\n");
                }

                return BadRequest(new { message = errorMessage });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.GetBaseException().Message });
            }
        }


        [HttpPut]
        [Route("UpdateUser")]
        [Authorize(Policy = Policies.ManageUsersPolicy)]
        public async Task<IActionResult> Put([FromForm] UserModifyRequestModel userModifyRequestModel)
        {
            try
            {
                if (userModifyRequestModel == null)
                    return BadRequest(new { message = "Invalid Registration" });

                var user = await userManager.FindByIdAsync(userModifyRequestModel.Id);
                if (user == null)
                    return BadRequest(new { message = "Invalid Registration" });

                user.UserName = userModifyRequestModel.UserName;
                user.Email = userModifyRequestModel.Email;

                var result = await userManager.UpdateAsync(user);
                if (!result.Errors.IsNullOrEmpty())
                {
                    return BadRequest(new { message = string.Join(Environment.NewLine, result.Errors.Select(e => e.Description)) });
                }

                return Ok(new ApplicationUserModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    UserName = user.UserName
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.GetBaseException().Message });
            }
        }

        [HttpPost]
        [Route("ResetPassword")]
        [Authorize(Policy = Policies.ResetUserPasswordPolicy)]
        public async Task<IActionResult> ChangeUserStatus(UserResetPasswordRequestModel requestModel)
        {
            try
            {
                var user = await userManager.FindByIdAsync(requestModel.ApplicationUserId);
                if (user == null)
                    return BadRequest(new { message = "Can't find a user, please Contact System Administrator" });

                var token = await userManager.GeneratePasswordResetTokenAsync(user);
                var result = await userManager.ResetPasswordAsync(user, token, requestModel.NewPassword);
                if (!result.Succeeded)
                    return BadRequest(result.Errors.FirstOrDefault()?.Description);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.GetBaseException().Message });
            }
        }

    }
}
