using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SimpleAds30.Web.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using SimpleAds30.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace SimpleAds30.Web.Controllers
{
    public class HomeController : Controller
    {
        private string _connString = @"Data Source=.\sqlexpress;Initial Catalog=SimpleAds;Integrated Security=true;";

        public IActionResult Index()
        {
            AdsRepository adsRepo = new(_connString);
            List<Ad> ads = adsRepo.GetAds();
            if (ads == null)
            {
                return View(new HomeViewModel 
                {
                    Ads = new List<AdViewModel>() 
                });
            }
                        
            return View(new HomeViewModel
            {
                Ads = ads.Select(ad => new AdViewModel
                {
                    Ad = ad,
                    CanDelete = ad.UserId == adsRepo.GetUserIdByEmail(User.Identity.IsAuthenticated ? User.Identity.Name : "")
                }).ToList()
            });
        }

        [Authorize]
        public IActionResult NewAd()
        {
            return View();
        }

        [HttpPost]
        public IActionResult NewAd(NewAd newAd)
        {
            AdsRepository adsRepo = new(_connString);
            newAd.UserId = adsRepo.GetUserIdByEmail(User.Identity.Name);
            int adId = adsRepo.AddAd(newAd);
            return Redirect("/home");
        }

        [HttpPost]
        public IActionResult DeleteAd(int id, bool myAccount)
        {
            AdsRepository adsRepo = new(_connString);
            adsRepo.DeleteAd(id);
            if (myAccount)
            {
                return Redirect("/home/myaccount");
            }
            return Redirect("/");
        }

        [Authorize]
        public IActionResult MyAccount()
        {
            AdsRepository adsRepo = new(_connString);
            List<Ad> ads = adsRepo.GetAds(adsRepo.GetUserIdByEmail(User.Identity.Name));
            if (ads == null)
            {
                return View(new HomeViewModel
                {
                    Ads = new List<AdViewModel>()
                });
            }

            return View(new HomeViewModel
            {
                Ads = ads.Select(ad => new AdViewModel
                {
                    Ad = ad,
                    CanDelete = ad.UserId == adsRepo.GetUserIdByEmail(User.Identity.IsAuthenticated ? User.Identity.Name : "")
                }).ToList()
            });
        }
    }


    public static class SessionExtensions
    {
        public static void Set<T>(this ISession session, string key, T value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }

        public static T Get<T>(this ISession session, string key)
        {
            string value = session.GetString(key);

            return value == null ? default(T) :
                JsonConvert.DeserializeObject<T>(value);
        }
    }
}