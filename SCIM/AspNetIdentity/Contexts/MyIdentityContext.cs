using AspNetIdentity.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AspNetIdentity.Contexts
{
        public class MyIdentityContext : IdentityDbContext<MyIdentityUser>
        {
            public MyIdentityContext(DbContextOptions<MyIdentityContext> optionsBuilderOptions)
            : base(optionsBuilderOptions)
            {
            }
        }
}
