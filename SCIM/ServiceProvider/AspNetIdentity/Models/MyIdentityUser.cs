using Microsoft.AspNetCore.Identity;

namespace AspNetIdentity.Models
{
    public class MyIdentityUser : IdentityUser
    {
        public string Schemas { get; set; }
    }
}
