using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DefaultResources.Models;
using Microsoft.Extensions.Logging;
using Rsk.AspNetCore.Scim.Clients;
using Rsk.AspNetCore.Scim.Clients.Models;
using Rsk.AspNetCore.Scim.Interfaces;
using Rsk.AspNetCore.Scim.Models;
using Rsk.AspNetCore.Scim.Results;
using Shared.Models;
using Shared.Stores;

namespace DefaultResources.Services
{
    public interface IGroupService
    {
        Task Update(ClientGroup group);
        Task Create(ClientGroup group);
        Task<ClientGroup> Read(string id, string serviceProviderName);
        Task Delete(string id);
    }

    public class GroupService : IGroupService
    {
        private readonly IStore<ClientUser> userStore;
        private readonly IScimClient<ClientGroupDto, Group> scimClient;
        private readonly IStore<ClientGroup> groupStore;
        private readonly ILogger<GroupService> logger;
        private readonly IResourceMapper<ClientGroupDto, Group> mapper;

        public GroupService(IStore<ClientUser> userStore, IScimClient<ClientGroupDto, Group> scimClient, IStore<ClientGroup> groupStore,
            ILogger<GroupService> logger, IResourceMapper<ClientGroupDto, Group> mapper)
        {
            this.userStore = userStore ?? throw new ArgumentNullException(nameof(userStore));
            this.scimClient = scimClient ?? throw new ArgumentNullException(nameof(scimClient));
            this.groupStore = groupStore ?? throw new ArgumentNullException(nameof(groupStore));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task Update(ClientGroup group)
        {
            var foundGroup = groupStore.Get(group.Id);

            var allResults = new List<IScimClientResult<Group>>();

            foreach (var serviceProviderPair in foundGroup.SpNameToId)
            {
                var results = await UpdateGroup(group, serviceProviderPair);
                allResults.AddRange(results.InnerResults);
            }

            if (allResults.All(ar => ar.IsSuccess))
            {
                groupStore.Update(group);
            }
            else
            {
                var errors = allResults.Where(ar => !ar.IsSuccess)
                    .Select(ir => ir.ErrorMessage);

                var joined = string.Join(',', errors);
                logger.LogError(joined);
            }
        }

        public async Task Create(ClientGroup group)
        {
            var serviceProviders = GetAllServiceProviders();
            
            var allResults = new List<IScimClientResult<Group>>();

            foreach (var serviceProviderPair in serviceProviders)
            {
                var results = await CreateGroup(group, serviceProviderPair);
                allResults.AddRange(results.InnerResults);

                if(results.IsSuccess) group.SpNameToId.Add(serviceProviderPair.Key, results.InnerResults.First().ResourceId);
            }

            if (allResults.All(ar => ar.IsSuccess))
            {
                groupStore.Create(group);
            }
            else
            {
                var errors = allResults.Where(ar => !ar.IsSuccess)
                                                        .Select(ir => ir.ErrorMessage);

                var joined = string.Join(',', errors);
                logger.LogError(joined);
            }
        }

        private async Task<IAggregateScimResult<Group>> UpdateGroup(ClientGroup group, KeyValuePair<string, string> serviceProviderPair)
        {
            var spUrl = GetServiceProviderUri(serviceProviderPair.Key);

            var groupForRequest = new ClientGroupDto
            {
                DisplayName = group.DisplayName
            };

            var members = new List<ClientMemberDto>();

            foreach (var member in group.Members)
            {
                var storeMember = userStore.Get(member);

                if (!storeMember.SpNameToId.TryGetValue(serviceProviderPair.Key, out var id)) continue;

                members.Add(new ClientMemberDto
                {
                    Id = id,
                    Uri = $"{spUrl}/users/{serviceProviderPair.Value}"
                });
            }

            groupForRequest.Members = members;

            var spResource = new ServiceProviderResource
            {
                ServiceProviderName = serviceProviderPair.Key,
                Id = serviceProviderPair.Value
            };

            return await scimClient.Update(groupForRequest, new []{ spResource }, default);
        }


        private async Task<IAggregateScimResult<Group>> CreateGroup(ClientGroup group, KeyValuePair<string, string> serviceProviderPair)
        {
            var groupForRequest = new ClientGroupDto
            {
                DisplayName = @group.DisplayName
            };

            var members = new List<ClientMemberDto>();

            foreach (var member in @group.Members)
            {
                var storeMember = userStore.Get(member);

                if (!storeMember.SpNameToId.TryGetValue(serviceProviderPair.Key, out var id)) continue;

                members.Add(new ClientMemberDto
                {
                    Id = id,
                    Uri = $"{serviceProviderPair.Value}/users/{id}"
                });
            }

            groupForRequest.Members = members;

            return await scimClient.Create(groupForRequest, default);
        }

        public async Task<ClientGroup> Read(string id, string serviceProviderName)
        {
            var foundResource = groupStore.Get(id);

            if (foundResource == null) return null;

            var idForServiceProvider = foundResource.SpNameToId.FirstOrDefault(d => d.Key == serviceProviderName);

            if (string.IsNullOrWhiteSpace(idForServiceProvider.Value)) return null;

            var resource = new ServiceProviderResource
            {
                Id = idForServiceProvider.Value,
                ServiceProviderName = serviceProviderName
            };

            var scimResult = await scimClient.Read(resource, CancellationToken.None);

            if (scimResult.IsSuccess)
            {
                var mapped = mapper.FromScimResource(scimResult.Resource);

                var members = userStore.GetAll()
                    .Where(u => u.SpNameToId.ContainsKey(serviceProviderName))
                    .Select(u => u.Id);

                return new ClientGroup
                {
                    DisplayName = mapped.DisplayName,
                    Id = id,
                    Members = members
                };
            }

            return null;
        }

        public async Task Delete(string id)
        {
            var foundResource = groupStore.Get(id);

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

        public Dictionary<string, string> GetAllServiceProviders()
        {
            return new Dictionary<string, string>
            {
                ["ServiceProviderName"] = "https://localhost:5000/SCIM/"
            };
        }

        public string GetServiceProviderUri(string name)
        {
            if (name == "ServiceProviderName") return "https://localhost:5000/SCIM/";

            return "";
        }
    }
}
