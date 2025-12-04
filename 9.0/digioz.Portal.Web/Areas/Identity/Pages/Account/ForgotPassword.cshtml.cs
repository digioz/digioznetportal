// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace digioz.Portal.Web.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailNotificationService _emailNotificationService;
        private readonly ILogger<ForgotPasswordModel> _logger;

        public ForgotPasswordModel(
            UserManager<IdentityUser> userManager,
            IEmailNotificationService emailNotificationService,
            ILogger<ForgotPasswordModel> logger)
        {
            _userManager = userManager;
            _emailNotificationService = emailNotificationService;
            _logger = logger;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);
                
                // Always show success message for security (don't reveal if email exists)
                if (user == null)
                {
                    _logger.LogWarning("Password reset requested for non-existent email: {Email}", Input.Email);
                    return RedirectToPage("./ForgotPasswordConfirmation");
                }

                try
                {
                    // Generate password reset token
                    var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    
                    // Build callback URL for password reset
                    var callbackUrl = Url.Page(
                        "/Account/ResetPassword",
                        pageHandler: null,
                        values: new { area = "Identity", code },
                        protocol: Request.Scheme);

                    // Get user name (use email if no username)
                    var userName = user.UserName ?? Input.Email;

                    // Send password reset email using EmailNotificationService
                    var emailSent = await _emailNotificationService.SendPasswordResetEmailAsync(
                        Input.Email,
                        userName,
                        callbackUrl);

                    if (emailSent)
                    {
                        _logger.LogInformation("Password reset email sent successfully to {Email}", Input.Email);
                    }
                    else
                    {
                        _logger.LogError("Failed to send password reset email to {Email}", Input.Email);
                        
                        // Still redirect to confirmation for security, but log the failure
                        // In production, you might want to show a generic error or retry
                    }
                }
                catch (System.Exception ex)
                {
                    _logger.LogError(ex, "Error processing password reset for {Email}", Input.Email);
                    
                    // Still redirect to confirmation for security
                }

                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            return Page();
        }
    }
}
