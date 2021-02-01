using DefaultResources.Models;
using Rsk.AspNetCore.Scim.Interfaces;
using Rsk.AspNetCore.Scim.Models;

namespace DefaultResources.Mappers
{
    public class ClientUserMapper : IResourceMapper<ClientUser, User>
    {
        public User ToScimResource(ClientUser resource)
        {
            return new User
            {
                UserName = resource.UserName,
                DisplayName = resource.DisplayName,
                NickName = resource.NickName,
                Name = new Rsk.AspNetCore.Scim.Models.Name
                {
                    GivenName = resource.Name.FirstName
                },
                Id = resource.Id
            };
        }

        public ClientUser FromScimResource(User scimResource)
        {
            return new ClientUser
            {
                UserName = scimResource.UserName,
                DisplayName = scimResource.DisplayName,
                NickName = scimResource.NickName,
                Name = new ClientName
                {
                    FirstName = scimResource.Name.GivenName
                },
                Id = scimResource.Id
            };
        }
    }
}
