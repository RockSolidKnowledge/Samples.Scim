using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CustomStoreAndValidation.Mappers;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Rsk.AspNetCore.Scim.EntityFramework;
using Rsk.AspNetCore.Scim.Enums;
using Rsk.AspNetCore.Scim.Models;
using Rsk.AspNetCore.Scim.Results;
using Rsk.AspNetCore.Scim.Stores;

namespace CustomStoreAndValidation.Stores
{
    public class ScimStore : IScimStore<User>
    {
        private readonly ScimDbContext dbContext;
        private readonly UserMapper userMapper = new UserMapper();

        public ScimStore(ScimDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<IEnumerable<(bool Exists, string Id)>> Exists(IEnumerable<string> ids)
        {
            var existingIds = await dbContext.Users.Where(s => ids.Contains(s.Id))
                                                   .Select(s => s.Id)
                                                   .ToListAsync();

            return ids.Select(id => (existingIds.Any(eid => eid == id), id));
        }

        public async Task<IScimResult> Delete(string id, string resourceSchema)
        {
            var user = await dbContext.Users.FindAsync(id);

            if (user != null)
            {
                dbContext.Users.Remove(user);
                await dbContext.SaveChangesAsync();
            }

            return new ScimResult(ScimResultStatus.Success);
        }

        public async Task<IScimResult<User>> Add(User resource, IEnumerable<ScimExtensionValue> scimExtensions, string resourceSchema)
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.UserName == resource.UserName);

            if (user != null)
            {
                return ScimResult<User>.Error(ScimStatusCode.Status409Conflict,
                    $"User with the username '{user.UserName}' already exists");
            }

            var mappedUser = userMapper.ToEntity(resource);

            mappedUser.Id = Guid.NewGuid().ToString();

            dbContext.Users.Add(mappedUser);

            await dbContext.SaveChangesAsync();

            return ScimResult<User>.Success(userMapper.ToDomain(mappedUser));
        }

        public async Task<User> GetById(string id)
        {
            var user = await dbContext.Users.FindAsync(id);

            if (user == null)
            {
                return null;
            }

            return userMapper.ToDomain(user);
        }

        public async Task<IScimResult<User>> Update(User resource, IEnumerable<ScimExtensionValue> scimExtensions, string resourceSchema)
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == resource.Id);

            if (user == null)
            {
                return ScimResult<User>.Error(ScimStatusCode.Status404NotFound,
                    $"User doesn't exist");
            }

            var mappedUser = userMapper.ToEntity(resource);

            //...
            user.UserName = mappedUser.UserName;
            //...

            dbContext.Users.Update(user);

            await dbContext.SaveChangesAsync();

            return ScimResult<User>.Success(userMapper.ToDomain(user));
        }

        public async Task<IScimResult<IEnumerable<ScimExtensionValue>>> GetExtensionsForResource(string resourceId, string resourceSchema)
        {
            var extensions =  await dbContext.ScimExtensions.Where(se =>
                se.ResourceSchema == resourceSchema && se.ResourceId == resourceId).ToListAsync();

            var mappedExtensions = extensions.Select(ex => new ScimExtensionValue
            {
                ExtensionSchema = ex.ExtensionSchema,
                Value = JsonConvert.DeserializeObject(ex.Value)
            });

            return ScimExtensionResult.Success(mappedExtensions);
        }
    }
}
