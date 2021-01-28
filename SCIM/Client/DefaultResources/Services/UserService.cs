using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DefaultResources.Models;
using DefaultResources.Stores;
using Microsoft.Extensions.Logging;
using Rsk.AspNetCore.Scim.Clients;
using Rsk.AspNetCore.Scim.Clients.Models;
using Rsk.AspNetCore.Scim.Interfaces;
using Rsk.AspNetCore.Scim.Models;
using Rsk.AspNetCore.Scim.Results;

namespace DefaultResources.Services
{
    public interface IUserService
    {
        Task Create(ClientUser user);
        Task Delete(string id);
        Task<ClientUser> Read(string id);
        Task Update(ClientUser user);
    }

    public class UserService : IUserService
    {
        private IScimClient<ClientUser, User> scimClient;
        private readonly IResourceMapper<ClientUser, User> mapper;
        private readonly IStore store;
        private readonly ILogger<UserService> logger;

        public UserService(IScimClient<ClientUser, User> scimClient, IResourceMapper<ClientUser, User> mapper,
            IStore store, ILogger<UserService> logger)
        {
            this.scimClient = scimClient ?? throw new ArgumentNullException(nameof(scimClient));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            this.store = store ?? throw new ArgumentNullException(nameof(store));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Create(ClientUser user)
        {
            var scimResult = await scimClient.Create(user, default);

            if (scimResult.IsSuccess)
            {
                var serviceProviderResults = scimResult.InnerResults.ToDictionary(ir => ir.ServiceProviderName, ir => ir.ResourceId);

                user.IdToSpNameMap = serviceProviderResults;

                store.Create(user);
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
            var foundUser = store.Get(id);

            if (foundUser == null) return;

            var tasks = new List<Task<IScimClientResult>>();

            foreach (var serviceProviderResource in foundUser.IdToSpNameMap)
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

        public async Task<ClientUser> Read(string id)
        {
            var foundUser = store.Get(id);

            if (foundUser == null) return null;

            var resource = new ServiceProviderResource
            {
                Id = foundUser.ScimId,
                ServiceProviderName = "ServiceProviderName"
            };

            var scimResult = await scimClient.Read(resource, CancellationToken.None);

            if (scimResult.IsSuccess) return mapper.FromScimResource(scimResult.Resource);

            return null;
        }

        public async Task Update(ClientUser user)
        {
            var scimResult = await scimClient.Update(user, default);

            if (scimResult.IsSuccess)
            {
                store.Update(user);
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