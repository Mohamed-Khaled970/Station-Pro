using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationPro.Application.DTOs.Auth
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Store name is required.")]
        [MaxLength(200)]
        public string StoreName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        [MaxLength(20)]
        [RegularExpression(@"^[0-9]{10,15}$", ErrorMessage = "Please enter a valid phone number (digits only).")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your password.")]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        // Checkbox: browser sends "true" when checked, nothing when unchecked.
        // MVC binds it correctly as bool. No server attribute needed —
        // the HTML `required` attribute + JS handles the UX.
        // We validate it manually in the controller.
        public bool AcceptTerms { get; set; }
    }
}
