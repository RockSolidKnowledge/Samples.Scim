using Rsk.AspNetCore.Scim.EntityFramework.Entities;

namespace CustomStoreAndValidation.Mappers
{
    public class UserMapper
    {
        public User ToEntity(Rsk.AspNetCore.Scim.Models.User user)
        {
            return new User
            {
                Id = user.Id,
                UserName = user.UserName,
                Schemas = string.Join(",", user.Schemas)
            };
        }

        public Rsk.AspNetCore.Scim.Models.User ToDomain(User user)
        {
            return new Rsk.AspNetCore.Scim.Models.User
            {
                Id = user.Id,
                UserName = user.UserName,
                Schemas = user.Schemas.Split(",")
            };
        }
    }
}
