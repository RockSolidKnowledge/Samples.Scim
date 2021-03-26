using System;
using System.Threading.Tasks;
using DefaultResources.Models;
using DefaultResources.Services;
using Microsoft.AspNetCore.Mvc;

namespace DefaultResources.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GroupController : ControllerBase
    {
        private readonly IGroupService groupService;

        public GroupController(IGroupService groupService)
        {
            this.groupService = groupService ?? throw new ArgumentNullException(nameof(groupService));
        }

        [HttpPost]
        public async Task AddGroup(ClientGroup clientGroup)
        {
            await groupService.Create(clientGroup);
        }

        [HttpGet("{id}")]
        public async Task<ClientGroup> GetGroup([FromRoute]string id)
        {
            return await groupService.Read(id, "ServiceProviderName");
        }

        [HttpPut]
        public async Task UpdateGroup(ClientGroup clientGroup)
        {
            await groupService.Update(clientGroup);
        }

        [HttpDelete("{id}")]
        public async Task DeleteGroup([FromRoute] string id)
        {
            await groupService.Delete(id);
        }
    }
}
