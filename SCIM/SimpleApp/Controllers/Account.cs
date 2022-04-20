using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleApp.Models;
using SimpleApp.Services;

namespace SimpleApp.Controllers;

public class AccountController : Controller
{
    private readonly AppDbContext ctx;
   

    public AccountController(AppDbContext ctx)
    {
        this.ctx = ctx;
     
    }
    // GET
    public async Task<IActionResult> Index()
    {

        var users = await ctx.Users.Select(u => new AppUserModel(u)).ToListAsync();
        var groups = await ctx.Roles.Include(r=>r.Members).Select(r => new AppRoleModel()
        {
            Id = r.Id,
            Name = r.Name,
            Users = r.Members.Select(m=>new AppUserModel(m)).ToList()
        }).ToListAsync();
        
        return View( new AccountsModel()
        {
            AllRoles = groups,
            AllUsers = users
        });
    }
}