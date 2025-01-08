using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Sale.Database.Entities
{
    public class ApplicationUser : IdentityUser
    {

        [MaxLength(128)]
        public string PermissionDescription { get; set; }

    }
}
