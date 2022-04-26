using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NewsFeedNet.Data;
using NewsFeedNet.Models;
using NewsFeedNet.Services;
using Newtonsoft.Json;
using WebPush;

namespace NewsFeedNet.Controllers
{
    public class HomeController : Controller
    {
        private readonly NewsFeedDbContext _newsFeedDbContext;
        private readonly IConfiguration _configuration;
        private readonly INewsApi _newsApi;
        public HomeController(INewsApi newsApi, IConfiguration configuration, NewsFeedDbContext newsFeedDbContext)
        {
            _newsApi = newsApi;
            _configuration = configuration;
            _newsFeedDbContext = newsFeedDbContext;
        }

        public IActionResult Index()
        {
            if (!HttpContext.Request.Cookies.ContainsKey("categories") || String.IsNullOrEmpty(HttpContext.Request.Cookies["categories"]))
            {
                IndexViewModel view = new IndexViewModel();
                TempData["preferences"] = false;
                HttpContext.Response.Cookies.Append("categories", "");
                return View(view);
            }
            else
            {
                return RedirectToAction("Feed");
            }

        }

        public IActionResult Feed()
        {
            ViewBag.PublicKey = _configuration["VAPID:publicKey"];
            ViewBag.LastArticle = "";
            FeedViewModel view = new FeedViewModel();
            view.refreshTime =  Convert.ToInt32(HttpContext.Request.Cookies["refresh"]);
            return View(view);
        }

        public async Task<List<Source>> Sources()
        {
            string[] categories = HttpContext.Request.Cookies["categories"].Split();
            return await _newsApi.GetSources(categories[0]);
        }

        public async Task<JsonResult> SetCategories(string[] categories)
        {
            //if (HttpContext.Request.Cookies.ContainsKey("categories")) 
            //{
            HttpContext.Response.Cookies.Append("categories", String.Join(",", categories));
            var sourcesList = await Sources();
            //}
            //int status = 1;
            return Json(JsonConvert.SerializeObject((from s in sourcesList select s).Take(10)));
        }

        [HttpPost]
        public IActionResult SaveSettings(IndexViewModel model)
        {
            HttpContext.Response.Cookies.Append("sources", String.Join(",", model.sources));
            HttpContext.Response.Cookies.Append("refresh", model.refreshFreq.ToString());

            return RedirectToAction("Feed");
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public async Task<PartialViewResult> PartialFeed(string startDate, string endDate, bool subscribed, string uid)
        {
            bool newArticle = false;
            List<Article> articles = new List<Article>();
            string sources = HttpContext.Request.Cookies["sources"];
            if (String.IsNullOrEmpty(startDate) && String.IsNullOrEmpty(endDate))
            {
                //articles = await _newsApi.GetArticles(sources);
                if (ViewBag.LastArticle = articles[0].url) {
                    newArticle = true;
                    ViewBag.LastArticle = articles[0].url;
                }
            }
            else
            {
                //articles = await _newsApi.GetArticlesByDate(sources, startDate, endDate);
            }

            if(subscribed && newArticle)
            {
                var payload = "New articles avaible!";
                var device = await _newsFeedDbContext.PushInfos.SingleOrDefaultAsync(m => m.Name == uid);

                var subject = _configuration["VAPID:subject"];
                var publicKey = _configuration["VAPID:publicKey"];
                var privateKey = _configuration["VAPID:privateKey"];

                var pushSubscription = new PushSubscription(device.EndPoint, device.P256dh, device.Auth);
                var vapidDetails = new VapidDetails(subject, publicKey, privateKey);

                var webPushClient = new WebPushClient();
                webPushClient.SendNotification(pushSubscription, payload, vapidDetails);
                newArticle = false;
            }

            //PushSubscription pushSubscription = new PushSubscription();
            //string ep = HttpContext.Request.Cookies["ep"];

            //if(!String.IsNullOrEmpty(ep))             
            //{
            //    PushInfo pushInfo = _newsFeedDbContext.PushInfos.Where(p => p.EndPoint == ep).FirstOrDefault<PushInfo>();
            //    pushSubscription.Endpoint = pushInfo.EndPoint;
            //    pushSubscription.P256DH = pushInfo.P256dh;
            //    pushSubscription.Auth = pushInfo.Auth;

            //    var message = "New articles avaible!";
            //    var subject = _configuration["VAPID:subject"];
            //    var publicKey = _configuration["VAPID:publicKey"];
            //    var privateKey = _configuration["VAPID:privateKey"];

            //    var vapidDetials = new VapidDetails(subject, publicKey, privateKey);
            //    var webPushClient = new WebPushClient();

            //    try
            //    {
            //        await webPushClient.SendNotificationAsync(pushSubscription, message, vapidDetials);
            //    }
            //    catch (WebPushException e)
            //    {
            //        Console.WriteLine("Http STATUS code" + e.StatusCode);
            //    }

            //}

            return PartialView("_PartialFeed", articles);
        }

        public async Task<JsonResult> SavePushSub(string uid, string endPoint, string p256dh, string auth)
        {

            
            PushInfo pushInfo = new PushInfo();
            pushInfo.Name = uid; 
            pushInfo.EndPoint = endPoint;
            pushInfo.P256dh = p256dh;
            pushInfo.Auth = auth;

            //if (String.IsNullOrEmpty(pushInfo.Name)) 
            //{
            //    pushInfo.Name = Guid.NewGuid().ToString();  
            //}

            try
            {
                pushInfo = _newsFeedDbContext.PushInfos.Where(p => p.Name == pushInfo.Name).FirstOrDefault();
                if (pushInfo == null)
                {
                    pushInfo = new PushInfo();
                    pushInfo.Name = Guid.NewGuid().ToString();
                    pushInfo.EndPoint = endPoint;
                    pushInfo.P256dh = p256dh;
                    pushInfo.Auth = auth;
                    _newsFeedDbContext.Add(pushInfo);
                    await _newsFeedDbContext.SaveChangesAsync();
                    HttpContext.Response.Cookies.Append("ep", pushInfo.EndPoint);
                    return Json(pushInfo.Name.ToString());
                }
                else
                {
                    pushInfo.Auth = auth;
                    pushInfo.P256dh = p256dh;
                    _newsFeedDbContext.PushInfos.Update(pushInfo);
                    await _newsFeedDbContext.SaveChangesAsync();
                    HttpContext.Response.Cookies.Append("ep", pushInfo.EndPoint);
                    return Json(pushInfo.Name.ToString());
                }                
            }
            catch (DbUpdateException ex) {
                return Json("ERROR" + ex.GetBaseException().ToString());
            }

            
        }
    }
}
