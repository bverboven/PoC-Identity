using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;

namespace Identity.Library.Entities
{
    public class LoginEntry
    {
        public long Id { get; set; }
        [Required, MaxLength(450)]
        public string UserId { get; set; }
        [Required, MinLength(4), MaxLength(16)]
        public byte[] IPAddressBytes { get; set; }
        [NotMapped]
        public IPAddress IPAddress
        {
            get => new IPAddress(IPAddressBytes);
            set => IPAddressBytes = value.GetAddressBytes();
        }
        [Required, MaxLength(128)]
        public string Status { get; set; }
        [Required]
        public DateTime Created { get; set; } = DateTime.UtcNow;

        public ApplicationUser User { get; set; }
    }
}
