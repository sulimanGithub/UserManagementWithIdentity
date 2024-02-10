using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Net.Mail;
using UserManagement_WithIdentity.Models;
using UserManagement_WithIdentity.ViewModels;

namespace UserManagement_WithIdentity.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailSender _emailSender;

        public UsersController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _emailSender = emailSender;
        }


        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.Select(user => new UserViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserName = user.UserName,
                Email = user.Email,
                Roles = _userManager.GetRolesAsync(user).Result
            }).ToListAsync();

            return View(users);
        }

        public async Task<IActionResult> ManageRoles(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return NotFound();

            var roles = await _roleManager.Roles.ToListAsync();

            var viewModel = new UserRolesViewModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                Roles = roles.Select(role => new RoleViewModel
                {
                    RoleId = role.Id,
                    RoleName = role.Name,
                    IsSelected = _userManager.IsInRoleAsync(user, role.Name).Result
                }).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageRoles(UserRolesViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);

            if (user == null)
                return NotFound();

            var userRoles = await _userManager.GetRolesAsync(user);

            foreach (var role in model.Roles)
            {
                if (userRoles.Any(r => r == role.RoleName) && !role.IsSelected)
                    await _userManager.RemoveFromRoleAsync(user, role.RoleName);

                if (!userRoles.Any(r => r == role.RoleName) && role.IsSelected)
                    await _userManager.AddToRoleAsync(user, role.RoleName);
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> AddUser()
        {

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddUser(AdminAddUserViewModel user)
        {
            if (ModelState.IsValid)
            {
                var newUser = new ApplicationUser
                {
                    UserName = new MailAddress(user.Email).User,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName
                };
                var result = await _userManager.CreateAsync(newUser, user.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(newUser, "User");

                    var userId = await _userManager.GetUserIdAsync(newUser);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);

                    return RedirectToAction(nameof(Index));
                }
                
               // return RedirectToAction(nameof(Index));

            }
            return RedirectToAction(nameof(Index));
        }
    }
}
