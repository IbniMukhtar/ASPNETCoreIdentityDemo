using ASPNETCoreIdentityDemo.Models;
using ASPNETCoreIdentityDemo.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
namespace ASPNETCoreIdentityDemo.Controllers
{
    public class DashBoardController:Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        //Both UserManager and SignInManager services are injected into the AccountController using constructor injection
        public DashBoardController(UserManager<ApplicationUser> userManager,
                                 SignInManager<ApplicationUser> signInManager,
                                 RoleManager<ApplicationRole> roleManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            _roleManager = roleManager;
        }
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var userEmaill = HttpContext.Session.GetString("UserEmail");
            ViewBag.userId= HttpContext.Session.GetString("UserId");
            var userEmail = TempData["data"] as string;
            if (userEmaill == null)
            {
                ViewBag.UsersCount = await this.userManager.Users.CountAsync();
                ViewBag.RolesCount = await _roleManager.Roles.CountAsync();
                return View(new DashboardViewModel());
            }

            var user = await this.userManager.FindByEmailAsync(userEmaill);
            if (user == null)
            {
                return NotFound();
            }
                ViewBag.UserEmail = user.Email;
                ViewBag.FirstName = user.FirstName;
                ViewBag.LastName = user.LastName;
                ViewBag.UsersCount = await this.userManager.Users.CountAsync();
                ViewBag.RolesCount = await _roleManager.Roles.CountAsync();
                return View();
        }       
    }
}
