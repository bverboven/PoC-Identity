using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Identity.Library.Entities
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        [PersonalData]
        [MaxLength(256)]
        public string GivenName { get; set; }
        [PersonalData]
        [MaxLength(256)]
        public string FamilyName { get; set; }
    }
}
