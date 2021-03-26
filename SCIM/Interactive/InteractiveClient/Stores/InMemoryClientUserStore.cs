using System.Collections.Generic;
using System.Linq;
using InteractiveClient.Models;

namespace InteractiveClient.Stores
{
    public class InMemoryClientUserStore : IClientUserStore
    {
        private readonly IList<ClientUser> users;

        public InMemoryClientUserStore()
        {
            users = new List<ClientUser>();
        }

        public ClientUser Get(string Id)
        {
            return users.FirstOrDefault(u => u.EmployeeId == Id);
        }

        public IList<ClientUser> GetAll()
        {
            return users;
        }

        public void Add(ClientUser user)
        {
            var existingUser = Get(user.EmployeeId);
            if (existingUser == null) users.Add(user);
        }

        public void Delete(ClientUser user)
        {
            users.Remove(user);
        }

        public void Update(ClientUser user)
        {
            var existingUser = Get(user.EmployeeId);

            if (existingUser != null)
            {
                users.Remove(existingUser);
                users.Add(user);
            }
        }
    }
}