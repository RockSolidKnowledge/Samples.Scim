using System.Collections.Generic;
using System.Linq;
using Shared.Models;

namespace Shared.Stores
{
    public interface IStore<T> where T : ClientResource
    {
        T Get(string id);
        IEnumerable<T> GetAll();
        void Delete(T resource);
        void Create(T resource);
        void Update(T resource);
    }

    public class InMemoryStore<TClientResource> : IStore<TClientResource>
        where TClientResource : ClientResource
    {
        readonly List<TClientResource> resources = new List<TClientResource>();

        public TClientResource Get(string id)
        {
            return resources.FirstOrDefault(u => u.Id == id);
        }

        public IEnumerable<TClientResource> GetAll()
        {
            return resources.ToArray();
        }

        public void Delete(TClientResource resource)
        {
            var foundResource = Get(resource.Id);

            if (foundResource != null) resources.Remove(foundResource);
        }

        public void Create(TClientResource resource)
        {
            var foundResource = Get(resource.Id);

            if (foundResource == null) resources.Add(resource);
        }

        public void Update(TClientResource resource)
        {
            var foundResource = Get(resource.Id);

            if (foundResource != null)
            {
                resource.SpNameToId = foundResource.SpNameToId;

                resources.Remove(foundResource);
                resources.Add(resource);
            }
        }
    }
}
