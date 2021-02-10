using System.Collections.Generic;

namespace Shared.Models
{
    public class ClientGroup : ClientResource
    {
        public string DisplayName { get; set; }

        public IEnumerable<string> Members { get; set; }
    }
}
