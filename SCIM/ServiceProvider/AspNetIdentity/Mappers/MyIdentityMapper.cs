using AspNetIdentity.Models;
using Rsk.AspNetCore.Scim.Identity.Mappers;
using Rsk.AspNetCore.Scim.Models;
using System.Threading.Tasks;

namespace AspNetIdentity.Mappers
{
    public class MyIdentityMapper : IIdentityUserMappingService<User, MyIdentityUser>
    {
        public Task<User> ToResource(MyIdentityUser identityUser)
        {
            return Task.FromResult(new User
            {
                UserName = identityUser.UserName,
                Id = identityUser.Id,
                Schemas = identityUser.Schemas.Split(',')
            });
        }

        public UserMappingResult<MyIdentityUser> ToIdentityUser(User userResource)
        {
            var identityUser = new MyIdentityUser
            {
                UserName = userResource.UserName,
                Id = userResource.Id,
                Schemas = string.Join(',', userResource.Schemas)
            };

            return new UserMappingResult<MyIdentityUser>
            {
                MappedUser = identityUser
            };
        }
    }
}
