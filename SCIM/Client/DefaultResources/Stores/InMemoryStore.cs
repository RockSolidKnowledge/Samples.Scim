using System.Collections.Generic;
using System.Linq;
using DefaultResources.Models;

namespace DefaultResources.Stores
{
    public interface IStore
    {
        ClientUser Get(string id);
        void Delete(ClientUser user);
        void Create(ClientUser user);
        void Update(ClientUser user);
    }

    public class InMemoryStore : IStore
    {
        List<ClientUser> users = new List<ClientUser>();

        public ClientUser Get(string id)
        {
            return users.FirstOrDefault(u => u.Id == id);
        }

        public void Delete(ClientUser user)
        {
            var foundUser = Get(user.Id);

            if (foundUser != null) users.Remove(foundUser);
        }

        public void Create(ClientUser user)
        {
            var foundUser = Get(user.Id);

            if (foundUser == null) users.Add(user);
        }

        public void Update(ClientUser user)
        {
            var foundUser = Get(user.Id);

            if (foundUser != null)
            {
                user.IdToSpNameMap = foundUser.IdToSpNameMap;

                users.Remove(foundUser);
                users.Add(user);
            }
        }
    }
}
