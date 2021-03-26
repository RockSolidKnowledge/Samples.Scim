using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Rsk.AspNetCore.Scim.Enums;
using Rsk.AspNetCore.Scim.Results;
using Rsk.AspNetCore.Scim.Stores;
using Rsk.AspNetCore.Scim.Validators;
using ScimUser = Rsk.AspNetCore.Scim.Models.User;

namespace CustomStoreAndValidation.Validators
{
    public class ScimValidator : IScimValidator<ScimUser>
    {
        private readonly IScimStore<ScimUser> userStore;

        public ScimValidator(IScimStore<ScimUser> userStore)
        {
            this.userStore = userStore; 
        }

        public async Task<IScimResult<ScimUser>> ValidateUpdate(string resourceAsString, string schema)
        {
            var user = JsonConvert.DeserializeObject<ScimUser>(resourceAsString);

            var schemaResult = HasExpectedSchemas(user, schema);

            if (schemaResult.Status == ScimResultStatus.Failure)
            {
                return schemaResult;
            }

            if (string.IsNullOrWhiteSpace(user.Id))
            {
                return ScimResult<ScimUser>.Error(ScimStatusCode.Status400BadRequest,
                    "Id is required on an update");
            }

            var existingUser = await userStore.GetById(user.Id);

            if (existingUser == null)
            {
                return ScimResult<ScimUser>.Error(ScimStatusCode.Status400BadRequest, "Cannot update a user that does not exist");
            }

            return ScimResult<ScimUser>.Success(user);
        }

        public Task<IScimResult<ScimUser>> ValidateAdd(string resourceAsString, string schema)
        {
            var user = JsonConvert.DeserializeObject<ScimUser>(resourceAsString);

            var schemaResult = HasExpectedSchemas(user, schema);

            if (schemaResult.Status == ScimResultStatus.Failure)
            {
                return Task.FromResult(schemaResult);
            }

            if (string.IsNullOrWhiteSpace(user.UserName))
            {
                var error = ScimResult<ScimUser>.Error(ScimStatusCode.Status400BadRequest,
                    "Username is required on User");

                return Task.FromResult(error as IScimResult<ScimUser>);
            }

            var success = ScimResult<ScimUser>.Success(user);

            return Task.FromResult(success as IScimResult<ScimUser>);
        }

        private IScimResult<ScimUser> HasExpectedSchemas(ScimUser user, string schema)
        {
            if (user.Schemas == null || !user.Schemas.Any())
            {
                return ScimResult<ScimUser>.Error(ScimStatusCode.Status400BadRequest,
                    "Resource doesn't contain any schemas");
            }

            var hasExpectedSchema = user.Schemas.Contains(schema);

            if (!hasExpectedSchema)
            {
                return ScimResult<ScimUser>.Error(ScimStatusCode.Status400BadRequest,
                    "Resource doesn't contain expected schema");
            }

            return ScimResult<ScimUser>.Success(user);
        }
    }
}
