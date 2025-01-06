using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoffeeCCode.Models.DataTables;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sale.Api.ApiModel.Role;
using Sale.Api.ApiModel.User;
using Sale.Model.Base;

namespace Sale.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class RolesController(RoleManager<IdentityRole> roleManager) : ControllerBase
    {

        [HttpGet]
        public async Task<IActionResult> GetAll() {
            try {
                var roles = await roleManager.Roles.ToListAsync();
                return Ok(roles);
            } catch (Exception ex) {
                return BadRequest(new { message = ex.GetBaseException().Message });
            }
        }

        [HttpPost("GetRoles")]
        //[Authorize(Policy = Policies.ManageUsersPolicy)]
        public async Task<IActionResult> GetUsers(UserGetRequestModel requestModel) {
            try {
                var dataTableParamResult = new DataTableParamResult(requestModel.DataTableParam);

                var query = roleManager.Roles.AsNoTracking();

                var totalRecord = query.Count();

                if (!string.IsNullOrWhiteSpace(dataTableParamResult.SearchValue)) {
                    query = query.Where(e =>
                        e.Name.Contains(dataTableParamResult.SearchValue)
                    );
                }
                var filteredRecord = query.Count();
                switch (dataTableParamResult.SortColumn) {
                    case 0:
                        query = dataTableParamResult.SortDirection == "asc" ? query.OrderBy(e => e.Name) : query.OrderByDescending(e => e.Name);
                        break;
                    default:
                        query = query.OrderBy(e => e.Name);
                        break;
                }

                var roles = await query.Skip(dataTableParamResult.Skip).Take(dataTableParamResult.PageSize).ToListAsync();

                return Ok(new DataTableResult<List<IdentityRole>> {
                    data = roles,
                    recordsFiltered = filteredRecord,
                    recordsTotal = totalRecord
                });
            } catch (Exception ex) {
                return BadRequest(new { message = ex.GetBaseException().Message });
            }
        }

        [HttpGet]
        [Route("Get/{roleName}")]
        public async Task<IActionResult> Get(string roleName) {
            try {
                var roles = await roleManager.Roles.FirstOrDefaultAsync(e => e.Name == roleName);
                return Ok(roles);
            } catch (Exception ex) {
                return BadRequest(new { message = ex.GetBaseException().Message });
            }
        }

        [HttpPost("{roleName}")]
        public async Task<IActionResult> Post(string roleName) {
            try {
                var existRole = await roleManager.FindByNameAsync(roleName);
                if (existRole != null)
                    return NotFound(new { message = "Role already exist." });

                var role = new IdentityRole(roleName);
                var result = await roleManager.CreateAsync(role);
                if (result.Succeeded)
                    return Ok(role);

                return BadRequest(string.Join(Environment.NewLine, result.Errors.Select(e => e.Description)));
            } catch (Exception ex) {
                return BadRequest(new { message = ex.GetBaseException().Message });
            }
        }

        [HttpPut]
        public async Task<IActionResult> Put(RoleModifyRequestModel roleModifyRequestModel) {
            try {
                var role = await roleManager.FindByNameAsync(roleModifyRequestModel.RoleName);
                if (role == null)
                    return NotFound(new { message = "Role not found." });

                role.Name = roleModifyRequestModel.UpdatedRoleName;
                var result = await roleManager.UpdateAsync(role);
                if (result.Succeeded)
                    return Ok(role);

                return BadRequest(string.Join(Environment.NewLine, result.Errors.Select(e => e.Description)));
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
                    return NotFound(new { message = "Role not found." });

                var result = await roleManager.DeleteAsync(role);
                if (result.Succeeded)
                    return Ok();

                return BadRequest(string.Join(Environment.NewLine, result.Errors.Select(e => e.Description)));
            } catch (Exception ex) {
                return BadRequest(new { message = ex.GetBaseException().Message });
            }
        }
    }
}
