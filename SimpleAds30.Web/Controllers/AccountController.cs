using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using SimpleAds30.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SimpleAds30.Web.Controllers
{
    public class AccountController : Controller
    {
        private string _connString = @"Data Source=.\sqlexpress;Initial Catalog=SimpleAds;Integrated Security=true;";

        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SignUp(User user, string password)
        {
            var adsRepo = new AdsRepository(_connString);
            adsRepo.AddUser(user, password);
            return RedirectToAction("LogIn");
        }

        public IActionResult LogIn()
        {
            return View();
        }

        [HttpPost]
        public IActionResult LogIn(string emailAddress, string password)
        {
            var adsRepo = new AdsRepository(_connString);
            User user = adsRepo.LogIn(emailAddress, password);
            if (user == null)
            {
                //ViewBag.Message = "Invalid Login Information";
                return Redirect("/account/login");
            }

            //ViewBag.Message = null;
            var claims = new List<Claim>
            {
                new Claim("user", emailAddress)
            };
            HttpContext.SignInAsync(new ClaimsPrincipal(
                new ClaimsIdentity(claims, "Cookies", "user", "role"))).Wait();
            return Redirect("/home/newad");
        }

        public IActionResult LogOut()
        {
            HttpContext.SignOutAsync().Wait();
            return Redirect("/");
        }
    }
}
