// Author: Ryan Cobb (@cobbr_io)
// Project: EasyPeasy (https://github.com/cobbr/EasyPeasy)
// License: GNU GPLv3

using System;
using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Identity;

namespace EasyPeasy.Models.EasyPeasy
{
    public class EasyPeasyUser : IdentityUser
    {
        public EasyPeasyUser() : base()
        {
            this.Email = "";
            this.NormalizedEmail = "";
            this.PhoneNumber = "";
            this.LockoutEnd = DateTime.UnixEpoch;
            this.ThemeId = 1;
        }

        public int ThemeId { get; set; }
        public Theme Theme { get; set; }
    }

    public class EasyPeasyUserLogin
    {
        public string Id { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
    }

    public class EasyPeasyUserRegister : EasyPeasyUserLogin
    {
        [Required]
        public string ConfirmPassword { get; set; }
    }

    public class EasyPeasyUserLoginResult
    {
        public bool Success { get; set; } = true;
        public string EasyPeasyToken { get; set; } = default;
    }
}
