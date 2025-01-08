using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sale.Model;

namespace Sale.Security
{
    public class Claims
    {
        internal static ApplicationClaimModel ManageUsersPolicyClaim = new("Users", "Manage", "User Settings", "Manage Users", "Allow to manage users");
        internal static ApplicationClaimModel ResetUserPasswordPolicyClaim = new("Users", "ResetPassword", "User Settings", "Reset user password", "Allow to reset user password");

        internal static ApplicationClaimModel ManageClaimGroupsPolicyClaim = new("ClaimGroup", "Manage", "Claim groups Settings", "Manage claim groups", "Allow to manage claim groups");

        public static List<ApplicationClaimModel> GetAll() {
            var claims = typeof(Claims)
                    .GetFields(BindingFlags.NonPublic | BindingFlags.Static)
                    .Where(f => f.FieldType == typeof(ApplicationClaimModel))
                    .Select(f => (ApplicationClaimModel)f.GetValue(null))
                    .ToList();

            return claims;
        }
    }
}
