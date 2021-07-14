using Microsoft.AspNetCore.Authentication;
using Rsk.AspNetCore.Scim.Factories;
using Rsk.AspNetCore.Scim.Models;
using Rsk.AspNetCore.Scim.Stores;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InteractiveServiceProvider.Stores
{
    public class CustomScimStore<T> : InMemoryScimStore<T> where T : Resource
    {
        private readonly ICollection<T> resources;

        public CustomScimStore(
            ICollection<T> resources,
            ISystemClock systemClock,
            IExpressionFactory expressionFactory,
            IStoreScimExtensions extensionStore) : base(resources, systemClock, expressionFactory, extensionStore)
        {
            this.resources = resources;
        }

        public Task<IList<T>> GetAll()
        {
            lock (resources)
            {
                var allResources = resources.ToList();
                return Task.FromResult<IList<T>>(allResources);
            }
        }
    }
}