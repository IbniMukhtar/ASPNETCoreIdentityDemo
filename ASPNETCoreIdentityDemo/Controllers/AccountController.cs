using ASPNETCoreIdentityDemo.Models.ViewModels;
using ASPNETCoreIdentityDemo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ASPNETCoreIdentityDemo.Services;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Components.Sections;


namespace ASPNETCoreIdentityDemo.Controllers
{

    public class AccountController : Controller
    {
        //userManager will hold the UserManager instance and SignInManager instance 
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        //ISenderEmail will hold the EmailSender instance
        private readonly ISenderEmail _emailSender;
        private readonly ISMSSender _SMSSender;

        //Both UserManager and SignInManager services are injected into the AccountController using constructor injection
        public AccountController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<ApplicationRole> roleManager,
            ISenderEmail emailSender,
            ISMSSender SMSSender)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            _roleManager = roleManager;
            _emailSender = emailSender;
            _SMSSender = SMSSender;
        }

        [HttpGet]
        public async Task<IActionResult> Register()
        {
            RegisterViewModel model = new()
            {
                ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Copy data from RegisterViewModel to ApplicationUser
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    DOB = model.DOB,
                    PhoneNumber = model.PhoneNumber
                };
                if (!string.IsNullOrEmpty(model.PhoneNumber))
                {

                }
                // Store user data in AspNetUsers database table
                var result = await userManager.CreateAsync(user, model.Password!);

                // If user is successfully created, sign-in the user using
                // SignInManager and redirect to index action of HomeController
                if (result.Succeeded)
                {
                    await SendConfirmationEmail(model.Email, user);
                    ViewBag.Message = "Registration under Process! Please check your email to verify your account.";
                    TempData["SuccessMessage"] = "Before you can Login, please confirm your email, by clicking on the confirmation link we have emailed you..";

                    // Check if the user is signed in and has Admin or SuperAdmin role
                    if (signInManager.IsSignedIn(User) && (User.IsInRole("Admin") || User.IsInRole("SuperAdmin")))
                    {
                        /*return RedirectToAction("ListUsers", "Administration");*/
                        return RedirectToAction("Login", "Account");
                    }
                    TempData["SuccessMessage"] = "Registration under Process! Please check your email to verify your account..";
                    // If not an Admin or SuperAdmin, sign in the user and redirect to Login
                    /* await signInManager.SignInAsync(user, isPersistent: false);*/
                    return RedirectToAction("Login", "Account");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        private async Task SendConfirmationEmail(string? email, ApplicationUser? user)
        {
            //Generate the Token
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user!);
            //Build the Email Confirmation Link which must include the Callback URL
            var ConfirmationLink = Url.Action("ConfirmEmail", "Account",
            new { UserId = user!.Id, Token = token }, protocol: HttpContext.Request.Scheme);
            //Send the Confirmation Email to the User Email Id
            await _emailSender.SendEmailAsync(email!, "Confirm Your Email", $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(ConfirmationLink!)}'>clicking here</a>.", true);
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string UserId, string Token)
        {
            if (UserId == null || Token == null)
            {
                ViewBag.Message = "The link is Invalid or Expired";
            }

            //Find the User By Id
            var user = await userManager.FindByIdAsync(UserId!);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"The User ID {UserId} is Invalid";
                return View("NotFound");
            }

            //Call the ConfirmEmailAsync Method which will mark the Email as Confirmed
            var result = await userManager.ConfirmEmailAsync(user, Token!);
            if (result.Succeeded)
            {
                ViewBag.Message = "Thank you for confirming your email";
                TempData["SuccessMessage"] = "Thank you for confirming your email";
                return RedirectToAction("Index", "DashBoard");
            }

            ViewBag.Message = "Email cannot be confirmed";
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> Login(string? ReturnUrl = null)
        {
            ViewData["ReturnUrl"] = ReturnUrl;

            LoginViewModel model = new LoginViewModel
            {
                ReturnUrl = ReturnUrl,
                ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
            };
            ViewBag.EL = model.ExternalLogins;
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string? ReturnUrl)
        {
            if (ModelState.IsValid)
            {
                var result = await signInManager.PasswordSignInAsync(model.Email!, model.Password!, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    var user = await this.userManager.FindByEmailAsync(model.Email!);
                    if (user != null)
                    {
                        HttpContext.Session.SetString("UserEmail", user.Email!);
                        HttpContext.Session.SetString("UserId", user.Id);
                        ViewBag.UserEmail = user.Email;
                        ViewBag.FirstName = user.FirstName;
                        ViewBag.LastName = user.LastName;
                        TempData["data"] = user.Email;
                        ViewBag.UsersCount = await this.userManager.Users.CountAsync();
                        ViewBag.RolesCount = await _roleManager.Roles.CountAsync();
                        return RedirectToAction("Index", "Dashboard");
                    }
                    else
                    {
                        // Handle failure
                        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                        return View(model);
                    }

                }
            }
            LoginViewModel model1 = new LoginViewModel
            {
                ReturnUrl = ReturnUrl,
                ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
            };
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model1);
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult ExternalLogin(string provider, string returnUrl)
        {
            //This call will generate a URL that directs to the ExternalLoginCallback action method in the Account controller
            //with a route parameter of ReturnUrl set to the value of returnUrl.
            var redirectUrl = Url.Action(action: "ExternalLoginCallback", controller: "Account", values: new { ReturnUrl = returnUrl });

            // Configure the redirect URL, provider and other properties
            var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

            //This will redirect the user to the external provider's login page
            return new ChallengeResult(provider, properties);
        }

        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string? returnUrl, string? remoteError)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            // Check if the user is already signed in
            if (User.Identity!.IsAuthenticated)
            {
                // Redirect to the post-login action
                return RedirectToAction("Login", "Account");
            }

            var loginViewModel = new LoginViewModel
            {
                ReturnUrl = returnUrl,
                ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
            };

            if (remoteError != null)
            {
                ModelState.AddModelError(string.Empty, $"Error from external provider: {remoteError}");
                return View("Login", loginViewModel);
            }

            var info = await signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ModelState.AddModelError(string.Empty, "Error loading external login information.");
                return View("Login", loginViewModel);
            }

            var signInResult = await signInManager.ExternalLoginSignInAsync(info.LoginProvider,
                info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

            if (signInResult.Succeeded)
            {
                var email = User.Identity.Name;
                var user = await userManager.FindByEmailAsync(email!);
                // Determine the role of the user
                ViewBag.roles = await userManager.GetRolesAsync(user!);
                HttpContext.Session.SetString("UserEmail", user!.Email!);
                HttpContext.Session.SetString("UserId", user.Id);
                ViewBag.UserEmail = user.Email;
                ViewBag.FirstName = user.FirstName;
                ViewBag.LastName = user.LastName;
                TempData["data"] = user.Email;
                ViewBag.UsersCount = await this.userManager.Users.CountAsync();
                ViewBag.RolesCount = await _roleManager.Roles.CountAsync();
                return RedirectToAction("Index", "Dashboard");

            }

            // If there is no record in AspNetUserLogins table, the user may not have a local account
            else
            {
                // Get the email claim value
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);

                if (email != null)
                {
                    // Create a new user without password if we do not have a user already
                    var user = await userManager.FindByEmailAsync(email);

                    if (user == null)
                    {
                        user = new ApplicationUser
                        {
                            UserName = info.Principal.FindFirstValue(ClaimTypes.Email),
                            Email = info.Principal.FindFirstValue(ClaimTypes.Email),
                            FirstName = info.Principal.FindFirstValue(ClaimTypes.GivenName)!,
                            LastName = info.Principal.FindFirstValue(ClaimTypes.Surname)!,
                        };
                        if (user.LastName == null)
                        {
                            user.LastName = "";
                        }
                        //This will create a new user into the AspNetUsers table without password
                        await userManager.CreateAsync(user);
                    }

                    // Add a login (i.e., insert a row for the user in AspNetUserLogins table)
                    await userManager.AddLoginAsync(user, info);

                    //Then Signin the User
                    await signInManager.SignInAsync(user, isPersistent: false);

                    return LocalRedirect(returnUrl);
                }

                // If we cannot find the user email we cannot continue
                ViewBag.ErrorTitle = $"Email claim not received from: {info.LoginProvider}";
                ViewBag.ErrorMessage = "Please contact support on viqinfotec@gmail.com";

                return View("Error");
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [HttpGet]
        public async Task<IActionResult> IsEmailAvailable(string Email)
        {
            //Check If the Email Id is Already in the Database
            var user = await userManager.FindByEmailAsync(Email);

            if (user == null)
            {
                return Json(true);
            }
            else
            {
                return Json($"Email {Email} is already in use.");
            }
        }

        /*        [Authorize]
                [HttpGet]
                public IActionResult ChangePassword()
                {
                    return View();
                }*/

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> _PasswordUpdatePartial(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.GetUserAsync(User);
                if (user == null)
                {
                    // If User does not exist, return a JSON response indicating failure
                    return Json(new { success = false, errors = new[] { "User not found. Please log in again." } });
                }

                // Attempt to change the user's password
                var result = await userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);

                if (!result.Succeeded)
                {
                    // Password change failed, add errors to ModelState
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }

                    // Return JSON response with validation errors
                    return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
                }

                // Password change successful, refresh sign-in cookie
                await signInManager.RefreshSignInAsync(user);
                TempData["SuccessMessage"] = "Password successfully updated.";
                // Return JSON response indicating success
                return Json(new { success = true, message = "Password successfully updated." });
               
            }

            // If ModelState is not valid, return JSON response with validation errors
            return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        /* public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
         {
             if (ModelState.IsValid)
             {
                 //fetch the User Details
                 var user = await userManager.GetUserAsync(User);
                 if (user == null)
                 {
                     //If User does not exists, redirect to the Login Page
                     return RedirectToAction("Login", "Account");
                 }
                 // ChangePasswordAsync Method changes the user password
                 var result = await userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
                 // The new password did not meet the complexity rules or the current password is incorrect.
                 // Add these errors to the ModelState and rerender ChangePassword view
                 if (!result.Succeeded)
                 {
                     foreach (var error in result.Errors)
                     {
                         ModelState.AddModelError(string.Empty, error.Description);
                     }

                     return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
                 }
                 // Upon successfully changing the password refresh sign-in cookie
                 await signInManager.RefreshSignInAsync(user);
                 //Then redirect the user to the ChangePasswordConfirmation view
                 TempData["SuccessMessage"] = "Your password is successfully changed.";
                 return RedirectToAction("Settings", "Account");
             }
             return View(model);
         }*/

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> AddPassword()
        {
            //First Fetch the User Details
            var user = await userManager.GetUserAsync(User);
            //Then Check whether the User Already has a Password
            var userHasPassword = await userManager.HasPasswordAsync(user!);
            //If the user already has a password, redirect to the ChangePassword Action method
            if (userHasPassword)
            {
                return RedirectToAction("ChangePassword", "Account");
            }
            //If the user has no password, then display the Add Password view
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddPassword(AddPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.GetUserAsync(User);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Unable to Load User.");
                    return View();
                }
                //Call the AddPasswordAsync method to set the new password without old password
                var result = await userManager.AddPasswordAsync(user, model.NewPassword!);
                // Handle the failure scenario
                if (!result.Succeeded)
                {
                    //fetch all the error messages and display on the view
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View();
                }
                // Handle Success Scenario
                // refresh the authentication cookie to store the updated user information
                await signInManager.RefreshSignInAsync(user);
                TempData["SuccessMessage"] = "You have successfully set a local password.Now, you can use either your local user account or external account to Login";
                //redirecting to the AddPasswordConfirmation action method
                return RedirectToAction("Index", "DashBoard");
            }
            return View();
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> ConfirmPhoneNumber()
        {
            //If the User already provided the Mobile while registering, we need to show that mobile number,
            //else we need to display empty and allow the user to add or update the mobile number
            ConfirmPhoneNumberViewModel model1 = new ConfirmPhoneNumberViewModel();
            var user = await userManager.GetUserAsync(User);
            if (user!.PhoneNumberConfirmed)
            {
                TempData["SuccessMessage"] = "Phone Number Already Confirmed.You can change your number Now";
                // If not an Admin or SuperAdmin, sign in the user and redirect to Login
                /* await signInManager.SignInAsync(user, isPersistent: false);*/
                return RedirectToAction("ChangePhoneNumber", "Account");
            }
            else
            {
                ConfirmPhoneNumberViewModel model = new ConfirmPhoneNumberViewModel()
                {
                    PhoneNumber = user!.PhoneNumber
                };
                if (user!.PhoneNumber == null)
                {
                    return View(model);
                }
                if (user == null)
                {
                    return NotFound($"Unable to load user with ID '{userManager.GetUserId(User)}'.");
                }

                return View(model);
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> SendPhoneVerificationCode(ConfirmPhoneNumberViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.GetUserAsync(User);
                if (user == null)
                {
                    return NotFound($"Unable to load user with ID '{userManager.GetUserId(User)}'.");
                }

                //Generate the Token
                var token = await userManager.GenerateChangePhoneNumberTokenAsync(user, model.PhoneNumber!);

                // Code to send the token via SMS 
                var result = await _SMSSender.SendSmsAsync(model.PhoneNumber!, token);

                if (result)
                {
                    // Save or pass the phone number for later verification
                    TempData["PhoneNumber"] = model.PhoneNumber;

                    // Redirect to verification view
                    return RedirectToAction("VerifyPhoneNumber", "Account");
                }
                else
                {
                    ViewBag.ErrorTitle = "Unable to send SMS";
                    ViewBag.ErrorMessage = "Please try after some time";
                    return RedirectToAction("Error");
                }
            }

            return View(model);
        }

        [Authorize]
        [HttpGet]
        public IActionResult VerifyPhoneNumber()
        {
            TempData["PhoneNumber"] = TempData["PhoneNumber"] as string;
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> VerifyPhoneNumber(string Token)
        {
            var PhoneNumber = TempData["PhoneNumber"] as string;
            var user = await userManager.GetUserAsync(User);
            var result = await userManager.VerifyChangePhoneNumberTokenAsync(user, Token, PhoneNumber!);
            if (result)
            {
                // Update user's PhoneNumber and PhoneNumberConfirmed
                user!.PhoneNumber = PhoneNumber;
                user.PhoneNumberConfirmed = true;
                await userManager.UpdateAsync(user);
                // Redirect to success page or show success message
                TempData["SuccessMessage"] = "Your Phone Number Verified Successfully";
                return RedirectToAction("Index", "DashBoard");
            }
            else
            {
                // Handle verification failure
                ViewBag.ErrorTitle = "Verification Failed";
                ViewBag.ErrorMessage = "Either the Token Expired or you entered an invalid token";
                return RedirectToAction("Error");
            }
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPhoneNumber(ConfirmPhoneNumberViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Get the current user
                var user = await userManager.GetUserAsync(User);
                if (user == null)
                {
                    return NotFound($"Unable to load user with ID '{userManager.GetUserId(User)}'.");
                }

                // Update the user's phone number
                user.PhoneNumber = model.PhoneNumber;

                // Update the user in the database
                var result = await userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    // Redirect to a confirmation page or take appropriate action
                    return RedirectToAction("ConfirmPhoneNumber");
                }
                else
                {
                    // If update fails, add model errors
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }

            // If ModelState is invalid, return back to the ConfirmPhoneNumber view with errors
            return View("ConfirmPhoneNumber", model);
        }

        [HttpGet]
        [Authorize]
        public IActionResult ChangePhoneNumber()
        {
            ChangePhoneNumberViewModel model = new ChangePhoneNumberViewModel();
            return View(model);
        }


        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePhoneNumber(ChangePhoneNumberViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.GetUserAsync(User);
                if (user == null)
                {
                    return NotFound($"Unable to load user with ID '{userManager.GetUserId(User)}'.");
                }

                user.PhoneNumber = model.NewPhoneNumber;
                user.PhoneNumberConfirmed = false;
                var result = await userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    // Redirect to a success page or take appropriate action
                    TempData["SuccessMessage"] = "Your Phone Number Added.You need to verify it";
                    return RedirectToAction("ConfirmPhoneNumber", "Account");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If ModelState is invalid, return back to the ChangePhoneNumber view with errors
            return View(model);
        }

        [HttpGet]
        [Authorize]
        public IActionResult UploadProfilePicture()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UploadProfilePicture(ProfilePictureViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.GetUserAsync(User);
                if (user == null)
                {
                    return NotFound();
                }

                if (model.Picture != null)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await model.Picture.CopyToAsync(memoryStream);
                        user.ProfilePicture = memoryStream.ToArray();
                    }

                    var result = await userManager.UpdateAsync(user);
                    if (result.Succeeded)
                    {
                        // Profile picture updated successfully
                        return RedirectToAction("Settings", "Account");
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError("Picture", "Please select a file");
                }
            }

            // If we're here, something went wrong
            return View(model);
        }

        [HttpGet]
        public IActionResult ModelPopUp()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
        public IActionResult Settings()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUsername(string newUsername)
        {
            // Get the currently signed-in user
            var user = await userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Check if the new username (email) is already in use
            var userWithSameEmail = await userManager.FindByEmailAsync(newUsername);
            if (userWithSameEmail != null && userWithSameEmail.Id != user.Id)
            {
                ModelState.AddModelError(string.Empty, "The email is already in use by another user.");
                return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
            }

            // Update the username (email)
            user.UserName = newUsername;
            user.Email = newUsername;

            // Save the changes
            var result = await userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                // Return errors if updating username fails
                return Json(new { success = false, errors = result.Errors.Select(e => e.Description) });
            }

            // If email is already verified or verification is not needed, sign the user out and then back in
            await signInManager.SignOutAsync();
            await signInManager.SignInAsync(user, isPersistent: false);

            // Build the Email Confirmation Link
            await SendConfirmationEmail(user.Email, user);

            // Return the confirmation link
            return Json(new { success = true, message = "A verification email has been sent to your email address. Please verify it to continue using our services." });
        }



    }
}


