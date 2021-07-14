using InteractiveServiceProvider.Stores;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Rsk.AspNetCore.Scim.Models;

namespace InteractiveServiceProvider.Controllers
{
    public class HomeController : Controller
    {
        private readonly MyInMemoryScimStore<User> scimStore;

        public HomeController(MyInMemoryScimStore<User> scimStore)
        {
            this.scimStore = scimStore ?? throw new ArgumentNullException(nameof(scimStore));
        }

        public async Task<IActionResult> Index()
        {
            var users = await scimStore.GetAll();
            return View(users);
        }
    }
}