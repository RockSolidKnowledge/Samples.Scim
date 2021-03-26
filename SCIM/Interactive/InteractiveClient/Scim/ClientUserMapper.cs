using InteractiveClient.Models;
using Rsk.AspNetCore.Scim.Interfaces;
using Rsk.AspNetCore.Scim.Models;

namespace InteractiveClient
{
    public class ClientUserMapper : IResourceMapper<ClientUser, User>
    {
        public User ToScimResource(ClientUser resource)
        {
            return new User
            {
                Id = resource.EmployeeId,
                ExternalId = resource.EmployeeId,
                Name = new Name
                {
                    GivenName = resource.Name
                },
                UserName = resource.UserName
            };
        }

        public ClientUser FromScimResource(User scimResource)
        {
            return new ClientUser
            {
                Name = scimResource.Name.GivenName,
                UserName = scimResource.UserName,
                EmployeeId = scimResource.ExternalId
            };
        }
    }
}