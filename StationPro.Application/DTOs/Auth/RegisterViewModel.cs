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
        //[Required(ErrorMessage = "Store name is required")]
        //[StringLength(100, ErrorMessage = "Store name cannot exceed 100 characters")]
        public string StoreName { get; set; } = string.Empty;

        //[Required(ErrorMessage = "Email is required")]
        //[EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = string.Empty;

        //[Required(ErrorMessage = "Phone number is required")]
        //[Phone(ErrorMessage = "Invalid phone number")]
        //[StringLength(11, MinimumLength = 11, ErrorMessage = "Phone number must be 11 digits")]
        public string PhoneNumber { get; set; } = string.Empty;

        //[Required(ErrorMessage = "Password is required")]
        //[StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        //[DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        //[Required(ErrorMessage = "Please confirm your password")]
        //[DataType(DataType.Password)]
        //[Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;

        //[Required(ErrorMessage = "You must accept the terms and conditions")]
        public bool AcceptTerms { get; set; }
    }
}
