using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using IdentityModel;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace TokenService.Controllers.Users
{
    public sealed class EditUserModel
    {
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string? Password { get; set; }
        public string? PasswordVerification { get; set; }

        public string? CurrentPassword { get; set; }

        public EditUserModel()
        {
        }

        public EditUserModel(IEnumerable<Claim> source) : this()
        {
            Email = source.ClaimByName(JwtClaimTypes.Email);
            FullName = source.ClaimByName(JwtClaimTypes.Name);
        }
    }

    public static class ClaimListExtensions
    {
        public static string ClaimByName(this IEnumerable<Claim> source, string type)=>
            source.FirstOrDefault(i => i.Type.Equals(type, StringComparison.Ordinal))?.Value??"";

    }
}