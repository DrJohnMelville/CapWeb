﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using IdentityModel;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using TokenService.Data.UserPriviliges;

namespace TokenService.Controllers.Users
{
    public sealed class EditUserModel
    {
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string? Password { get; set; }
        [Compare(nameof(Password))]
        public string? PasswordVerification { get; set; }
        public string? CurrentPassword { get; set; }
        public IEnumerable<WebsiteMembership> Privileges { get; set; } = Array.Empty<WebsiteMembership>();

        public EditUserModel()
        {
        }

        public EditUserModel(IEnumerable<Claim> source) : this()
        {
            Email = source.ClaimByName(JwtClaimTypes.Email);
            FullName = source.ClaimByName(JwtClaimTypes.Name);
        }
    }

    public class WebsiteMembership
    {
        public string Site { get; }
        public SitePrivilege Privilege { get; }
        public string Url { get; }

        public WebsiteMembership(string site, SitePrivilege privilege, string url)
        {
            Site = site;
            Privilege = privilege;
            Url = url;
        }
    }
    

    public static class ClaimListExtensions
    {
        public static string ClaimByName(this IEnumerable<Claim> source, string type)=>
            source.FirstOrDefault(i => i.Type.Equals(type, StringComparison.Ordinal))?.Value??"";

    }
}