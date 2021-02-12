using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Rsk.AspNetCore.Scim.Models;
using Shared.Models;
using Shared.Services;

namespace DefaultResources.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IClientService<ClientUser, User> userService;

        public UserController(IClientService<ClientUser, User> userService)
        {
            this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        [HttpPost]
        public async Task AddUser(ClientUser clientUser)
        {
            await userService.Create(clientUser);
        }

        [HttpGet("{id}")]
        public async Task<ClientUser> GetUser([FromRoute]string id)
        {
            return await userService.Read(id, "ServiceProviderName");
        }

        [HttpPut]
        public async Task UpdateUser(ClientUser clientUser)
        {
            await userService.Update(clientUser);
        }

        [HttpDelete("{id}")]
        public async Task DeleteUser([FromRoute] string id)
        {
            await userService.Delete(id);
        }
    }
}
