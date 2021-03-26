using InteractiveClient.Models;
using System.Collections.Generic;

namespace InteractiveClient.Stores
{
    public interface IClientUserStore
    {
        ClientUser Get(string employeeId);
        IList<ClientUser> GetAll();
        void Add(ClientUser user);
        void Delete(ClientUser user);
        void Update(ClientUser user);
    }
}