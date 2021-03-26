using Rsk.AspNetCore.Scim.Models;
using Rsk.AspNetCore.Scim.Results;
using Rsk.AspNetCore.Scim.Stores;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InteractiveServiceProvider.Stores
{
    public class CustomScimStore : IScimStore<User>
    {
        private readonly IList<User> users;

        public CustomScimStore()
        {
            users = new List<User>();
        }

        public async Task<IScimResult<User>> Add(User resource, IEnumerable<ScimExtensionValue> scimExtensions, string resourceSchema)
        {
            resource.Id = Guid.NewGuid().ToString();
            users.Add(resource);

            return ScimResult<User>.Success(resource);
        }

        public Task<IList<User>> GetAll()
        {
            return Task.FromResult(users);
        }

        public Task<IEnumerable<(bool Exists, string Id)>> Exists(IEnumerable<string> ids)
        {
            throw new NotImplementedException();
        }

        public Task<IScimResult> Delete(string id, string resourceSchema)
        {
            throw new NotImplementedException();
        }

        public Task<User> GetById(string id)
        {
            throw new NotImplementedException();
        }

        public Task<IScimResult<User>> Update(User resource, IEnumerable<ScimExtensionValue> scimExtensions, string resourceSchema)
        {
            throw new NotImplementedException();
        }

        public Task<IScimResult<IEnumerable<ScimExtensionValue>>> GetExtensionsForResource(string resourceId, string resourceSchema)
        {
            throw new NotImplementedException();
        }
    }
}