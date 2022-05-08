using Rsk.AspNetCore.Scim.Interfaces;
using Rsk.AspNetCore.Scim.Models;
using Shared.Models;
using System.Collections.Generic;
using System.Linq;
using Rsk.AspNetCore.Scim.Constants;

namespace Shared.Mappers
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
                Name = new Name
                {
                    GivenName = resource.Name.FirstName
                },
                Id = resource.Id,
                Extensions = new Dictionary<string, ResourceExtension>
                {
                    [ScimSchemas.EnterpriseUser] = new EnterpriseUser
                    {
                        Organization = resource.Organization,
                        Department = resource.Department
                    }
                }
            };
        }

        public ClientUser FromScimResource(User scimResource)
        {
            var enterpriseUser =
                scimResource?.Extensions.First(e => e.Key == ScimSchemas.EnterpriseUser)
                    .Value as EnterpriseUser;
             
            return new ClientUser
            {
                UserName = scimResource.UserName,
                DisplayName = scimResource.DisplayName,
                NickName = scimResource.NickName,
                Name = new ClientName
                {
                    FirstName = scimResource.Name.GivenName
                },
                Id = scimResource.Id,
                Organization = enterpriseUser.Organization,
                Department = enterpriseUser.Department
            };
        }
    }
}
