using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InteractiveClient.Models;
using InteractiveClient.Stores;
using Microsoft.Extensions.Logging;
using Rsk.AspNetCore.Scim.Clients;
using Rsk.AspNetCore.Scim.Exceptions;
using Rsk.AspNetCore.Scim.Models;

namespace InteractiveClient
{
    public interface IUserService
    {
        Task AddUser(ClientUser user);
        Task<IList<ClientUser>> GetAllUsers();
    }

    public class UserService : IUserService
    {
        private readonly IScimClient<ClientUser, User> scimClient;
        private readonly IClientUserStore userStore;
        private readonly ILogger<UserService> logger;

        public UserService(IScimClient<ClientUser, User> scimClient, IClientUserStore userStore, ILogger<UserService> logger)
        {
            this.scimClient = scimClient ?? throw new ArgumentNullException(nameof(scimClient));
            this.userStore = userStore ?? throw new ArgumentNullException(nameof(userStore));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task AddUser(ClientUser user)
        {
            var scimResult = await scimClient.Create(user, default);

            if (scimResult.IsSuccess)
            {
                userStore.Add(user);
            }
            else
            {
                var errors = scimResult.InnerResults.Select(ir => ir.ErrorMessage);
                var joined = string.Join(',', errors);
                logger.LogError(joined);
                throw new ScimClientException(joined);
            }
        }

        public Task<IList<ClientUser>> GetAllUsers()
        {
            return Task.FromResult(userStore.GetAll());
        }
    }
}