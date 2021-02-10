using System.Linq;
using Rsk.AspNetCore.Scim.Interfaces;
using Rsk.AspNetCore.Scim.Models;
using Shared.Models;

namespace Shared.Mappers
{
    public class ClientGroupMapper : IResourceMapper<ClientGroup, Group>
    {
        public Group ToScimResource(ClientGroup resource)
        {
            return new Group
            {
                DisplayName = resource.DisplayName,
                Members = resource.Members.Select(m => new Member
                {
                    ScimRef = "User",   
                    Value = m
                })
            };
        }   

        public ClientGroup FromScimResource(Group scimResource)
        {
            return new ClientGroup
            {
                DisplayName = scimResource.DisplayName,
                Members = scimResource.Members.Select(m => m.Value)
            };
        }
    }
}
