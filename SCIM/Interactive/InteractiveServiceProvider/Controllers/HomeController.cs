using InteractiveServiceProvider.Stores;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace InteractiveServiceProvider.Controllers
{
    public class HomeController : Controller
    {
        private readonly CustomScimStore scimStore;
        private readonly ILogger<HomeController> _logger;

        public HomeController(CustomScimStore scimStore, ILogger<HomeController> logger)
        {
            this.scimStore = scimStore ?? throw new ArgumentNullException(nameof(scimStore));
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var users = await scimStore.GetAll();
            return View(users);
        }
    }
}