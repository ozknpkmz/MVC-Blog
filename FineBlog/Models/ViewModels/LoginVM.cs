﻿using Microsoft.Build.Framework;

namespace FineBlog.Models.ViewModels
{
    public class LoginVM
    {
        [Required]
        public string? Email { get; set; }
        [Required]
        public string? Username { get; set; }
        [Required]
        public string? Password { get; set; }
        public bool RememberMe { get; set; }
    }
}
