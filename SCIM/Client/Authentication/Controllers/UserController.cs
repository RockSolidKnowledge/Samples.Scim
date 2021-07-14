using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Rsk.AspNetCore.Scim.Models;
using Shared.Models;
using Shared.Services;

namespace Authentication.Controllers
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

        [HttpGet("{id}")]
        public async Task<ClientUser> GetUser([FromRoute]string id)
        {
            return await userService.Read(id, "ServiceProviderName");
        }

        [HttpPost]
        public async Task AddUser(ClientUser clientUser)
        {
            await userService.Create(clientUser);
        }
    }
}
