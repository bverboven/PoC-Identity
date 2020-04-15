using System;
using System.ComponentModel.DataAnnotations;

namespace Identity.Library.Entities
{
    public class RefreshToken
    {
        [Key]
        public string Token { get; set; }
        [Required, MaxLength(450)]
        public string UserId { get; set; }
        public DateTime? Expires { get; set; }

        public ApplicationUser User { get; set; }
    }
}