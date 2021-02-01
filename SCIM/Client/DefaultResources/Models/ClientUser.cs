using System.Collections.Generic;

namespace DefaultResources.Models
{
    public class ClientResource
    {
        public Dictionary<string, string> IdToSpNameMap = new Dictionary<string, string>();
    }

    public class ClientUser : ClientResource
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public ClientName Name { get; set; }
        public string DisplayName { get; set; }
        public string NickName { get; set; }
    }

    public class ClientName
    {
        public string FirstName { get; set; }
    }
}
