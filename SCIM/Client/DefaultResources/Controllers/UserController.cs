using System;
using System.Threading.Tasks;
using DefaultResources.Models;
using DefaultResources.Services;
using Microsoft.AspNetCore.Mvc;

namespace DefaultResources.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService userService;

        public UserController(IUserService userService)
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
