using BestStoreMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BestStoreMVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;

        public AccountController(UserManager<ApplicationUser> userManager,
                                   SignInManager<ApplicationUser> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }
        public IActionResult Register()
        {
            // if alreadyu logged in, redirect to main page
            if (signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

		[HttpPost]
        public async Task<IActionResult> Register(RegisterDto registerDto)
		{
            if (!ModelState.IsValid)
            {
                return View(registerDto);
            }

            var user = new ApplicationUser()
            {
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                UserName = registerDto.Email,
                Email = registerDto.Email,
                PhoneNumber = registerDto.PhoneNumber,
                Address = registerDto.Address,
                CreateAt = DateTime.Now,

            };

            var result = await userManager.CreateAsync(user, registerDto.Password);

            if (result.Succeeded)
            {
                //successful user registration, then sign in the new user
                await userManager.AddToRoleAsync(user, "client");

                await signInManager.SignInAsync(user, false);

                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

			return View(registerDto);
		}
		public async Task<IActionResult> Logout()
		{
			if (signInManager.IsSignedIn(User))
			{
				await signInManager.SignOutAsync();
			}

			return RedirectToAction("Index", "Home");
		}
		public IActionResult Login()
        {
            if (signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return View(loginDto);
            }

            var result = await signInManager.PasswordSignInAsync(loginDto.Email, loginDto.Password, loginDto.RememberMe, false);

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewBag.ErrorMessage = "Invalid Login Attemp";
            }

            return View(result);
        }

		[Authorize]
		public async Task<IActionResult> Profile()
		{
			var appUser = await userManager.GetUserAsync(User);
			if (appUser == null)
			{
				return RedirectToAction("Index", "Home");
			}

			var profileDto = new ProfileDto()
			{
				FirstName = appUser.FirstName,
				LastName = appUser.LastName,
				Email = appUser.Email ?? "",
				PhoneNumber = appUser.PhoneNumber,
				Address = appUser.Address,
			};

			return View(profileDto);
		}

		[Authorize]
		[HttpPost]
		public async Task<IActionResult> Profile(ProfileDto profileDto)
		{
			if (!ModelState.IsValid)
			{
				ViewBag.ErrorMessage = "Please fill all the required fields with valid values";
				return View(profileDto);
			}

			// Get the current user
			var appUser = await userManager.GetUserAsync(User);
			if (appUser == null)
			{
				return RedirectToAction("Index", "Home");
			}

			// Update the user profile
			appUser.FirstName = profileDto.FirstName;
			appUser.LastName = profileDto.LastName;
			appUser.UserName = profileDto.Email;
			appUser.Email = profileDto.Email;
			appUser.PhoneNumber = profileDto.PhoneNumber;
			appUser.Address = profileDto.Address;

			var result = await userManager.UpdateAsync(appUser);

			if (result.Succeeded)
			{
				ViewBag.SuccessMessage = "Profile updated successfully";
			}
			else
			{
				ViewBag.ErrorMessage = "Unable to update the profile: " + result.Errors.First().Description;
			}


			return View(profileDto);
		}

		public IActionResult AccessDenied()
		{
			return RedirectToAction("Index", "Home");
		}
	}
}
