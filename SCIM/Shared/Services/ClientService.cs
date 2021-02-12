using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Rsk.AspNetCore.Scim.Clients;
using Rsk.AspNetCore.Scim.Clients.Models;
using Rsk.AspNetCore.Scim.Interfaces;
using Rsk.AspNetCore.Scim.Models;
using Rsk.AspNetCore.Scim.Results;
using Shared.Models;
using Shared.Stores;

namespace Shared.Services
{
    public interface IClientService<TClientResource, TScimResource>
        where TClientResource : ClientResource
        where TScimResource : Resource
    {
        Task Create(TClientResource user);
        Task Delete(string id);
        Task<TClientResource> Read(string id, string serviceProviderName);
        Task Update(TClientResource user);
    }

    public class ClientService<TClientResource, TScimResource> : IClientService<TClientResource, TScimResource>
        where TClientResource : ClientResource
        where TScimResource : Resource
    {
        private IScimClient<TClientResource, TScimResource> scimClient;
        private readonly IResourceMapper<TClientResource, TScimResource> mapper;
        private readonly IStore<TClientResource> store;
        private readonly ILogger<ClientService<TClientResource, TScimResource>> logger;

        public ClientService(IScimClient<TClientResource, TScimResource> scimClient, IResourceMapper<TClientResource, TScimResource> mapper,
            IStore<TClientResource> store, ILogger<ClientService<TClientResource, TScimResource>> logger)
        {
            this.scimClient = scimClient ?? throw new ArgumentNullException(nameof(scimClient));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            this.store = store ?? throw new ArgumentNullException(nameof(store));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Create(TClientResource resource)
        {
            var scimResult = await scimClient.Create(resource, default);

            if (scimResult.IsSuccess)
            {
                var serviceProviderResults = scimResult.InnerResults.ToDictionary(ir => ir.ServiceProviderName, ir => ir.ResourceId);

                resource.SpNameToId = serviceProviderResults;

                store.Create(resource);
            }
            else
            {
                var errors = scimResult.InnerResults.Select(ir => ir.ErrorMessage);
                var joined = string.Join(',', errors);
                logger.LogError(joined);
            }
        }

        public async Task Delete(string id) 
        {
            var foundResource = store.Get(id);

            if (foundResource == null) return;

            var tasks = new List<Task<IScimClientResult>>();

            foreach (var serviceProviderResource in foundResource.SpNameToId)
            {
                var resource = new ServiceProviderResource
                {
                    Id = serviceProviderResource.Value,
                    ServiceProviderName = serviceProviderResource.Key
                };

                tasks.Add(scimClient.Delete(resource, CancellationToken.None));
            }

            await Task.WhenAll(tasks);

            var isSuccess = tasks.Select(t => t.Result.IsSuccess)
                .All(success => success);

            if (!isSuccess)
            {
                var errors = tasks.Select(t => t.Result.ErrorMessage);
                var joined = string.Join(',', errors);
                logger.LogError(joined);
            }
        }

        public async Task<TClientResource> Read(string id, string serviceProviderName)
        {
            var foundResource = store.Get(id);

            if (foundResource == null) return null;

            var idForSp = foundResource.SpNameToId.FirstOrDefault(d => d.Key == serviceProviderName);

            if (string.IsNullOrWhiteSpace(idForSp.Value)) return null;

            var resource = new ServiceProviderResource
            {
                Id = idForSp.Value,
                ServiceProviderName = serviceProviderName
            };

            var scimResult = await scimClient.Read(resource, CancellationToken.None);

            if (scimResult.IsSuccess) return mapper.FromScimResource(scimResult.Resource);

            return null;
        }

        public async Task Update(TClientResource resource)
        {
            var foundResource = store.Get(resource.Id);

            if (foundResource == null) return;

            var serviceProviders = foundResource.SpNameToId.Select(map => new ServiceProviderResource
            {
                Id = map.Value,
                ServiceProviderName = map.Key
            });

            var scimResult = await scimClient.Update(resource, serviceProviders, default);

            if (scimResult.IsSuccess)
            {
                store.Update(resource);
            }
            else
            {
                var errors = scimResult.InnerResults.Select(ir => ir.ErrorMessage);
                var joined = string.Join(',', errors);
                logger.LogError(joined);
            }
        }
    }
}