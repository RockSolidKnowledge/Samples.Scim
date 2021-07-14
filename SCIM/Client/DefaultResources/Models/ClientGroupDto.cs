using System.Collections.Generic;

namespace DefaultResources.Models
{
    public class ClientGroupDto
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }

        public IEnumerable<ClientMemberDto> Members { get; set; }
    }
}