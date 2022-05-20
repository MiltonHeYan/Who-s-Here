using Microsoft.AspNetCore.Identity;

namespace WhosHereServer.Data
{
    public class AppUser : IdentityUser
    {
        public byte[]? AvatarImageData { get; set; }
    }
}
