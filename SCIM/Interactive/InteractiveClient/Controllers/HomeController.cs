using System;
using System.Threading.Tasks;
using InteractiveClient.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace InteractiveClient.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUserService userService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IUserService userService, ILogger<HomeController> logger)
        {
            this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var users = await userService.GetAllUsers();
            return View(users);
        }

        [HttpGet]
        public IActionResult AddUser()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUser(ClientUser user)
        {
            try
            {
                await userService.AddUser(user);
                return View("Success");
            }
            catch (Exception ex)
            {
                return View("Error", ex.Message);
            }
        }
    }
}