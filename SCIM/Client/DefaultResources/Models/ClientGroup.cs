using System.Collections.Generic;
using Shared.Models;

namespace DefaultResources.Models
{
    public class ClientGroup : ClientResource
    {
        public string DisplayName { get; set; }
        public IEnumerable<string> Members { get; set; }
    }
}
