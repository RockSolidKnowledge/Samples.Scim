using System.Linq;
using DefaultResources.Models;
using Rsk.AspNetCore.Scim.Interfaces;
using Rsk.AspNetCore.Scim.Models;

namespace DefaultResources.Mappers
{
    public class ClientGroupMapper : IResourceMapper<ClientGroupDto, Group>
    {
        public Group ToScimResource(ClientGroupDto resource)
        {
            return new Group
            {
                DisplayName = resource.DisplayName,
                Members = resource.Members.Select(m => new Member
                {
                    ScimRef = m.Uri,
                    Value = m.Id
                })
            };
        }

        public ClientGroupDto FromScimResource(Group scimResource)
        {
            return new ClientGroupDto
            {
                DisplayName = scimResource.DisplayName,
                Members = scimResource.Members.Select(m => new ClientMemberDto
                {
                    Id = m.Value,
                    Uri = m.ScimRef
                })
            };
        }
    }
}
