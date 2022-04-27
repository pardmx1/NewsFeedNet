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
            if (!HttpContext.Request.Cookies.ContainsKey("categories") || String.IsNullOrEmpty(HttpContext.Request.Cookies["categories"])
                || !HttpContext.Request.Cookies.ContainsKey("sources") || String.IsNullOrEmpty(HttpContext.Request.Cookies["sources"]))             
            {
                IndexViewModel view = new IndexViewModel();
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
            HttpContext.Response.Cookies.Append("lastArt", "");
            FeedViewModel view = new FeedViewModel();
            view.refreshTime =  Convert.ToInt32(HttpContext.Request.Cookies["refresh"]);
            return View(view);
        }

        public async Task<List<Source>> Sources(string[] categories)
        {
            //string[] categories = HttpContext.Request.Cookies["categories"].Split(",");
            return await _newsApi.GetSources(categories);
        }

        public async Task<JsonResult> SetCategories(string[] categories)
        {
            if (categories.Length > 1)
            {
                HttpContext.Response.Cookies.Append("categories", String.Join(",", categories));
            }
            else
            {
                HttpContext.Response.Cookies.Append("categories", categories[0]);
            }

            var sourcesList = await Sources(categories);

            int sc = 10/categories.Length;

            List<Source> sources = new List<Source>();
            Random random = new Random();
            foreach(string c in categories)
            {
                sources.AddRange(sourcesList.OrderBy(x => random.Next()).Where(x => x.category.Equals(c)).Take(sc));
            }

            if(sources.Count() < 10)
            {
                sources.Add(sourcesList.Last());
            }

            return Json(JsonConvert.SerializeObject(sources));
        }

        [HttpPost]
        public IActionResult SaveSettings(IndexViewModel model)
        {
            HttpContext.Response.Cookies.Append("sources", String.Join(",", model.sources));
            HttpContext.Response.Cookies.Append("refresh", model.refreshFreq.ToString());

            return RedirectToAction("Feed");
        }
        public IActionResult Settings()
        {
            Response.Cookies.Delete("sources");
            Response.Cookies.Delete("refresh");
            Response.Cookies.Delete("lastArt");
            Response.Cookies.Delete("categories");
            return RedirectToAction("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public async Task<PartialViewResult> PartialFeed(string startDate, string endDate, bool subscribed, string uid, string page)
        {
            bool newArticle = false;
            ApiArticles apiArticles = new ApiArticles();
            //List<Article> articles = new List<Article>();
            string sources = HttpContext.Request.Cookies["sources"];
            string lastArt = HttpContext.Request.Cookies["lastArt"];
            if (String.IsNullOrEmpty(startDate) && String.IsNullOrEmpty(endDate))
            {
                
                apiArticles = await _newsApi.GetArticles(sources, page);

                if (!lastArt.Equals(apiArticles.articles[0].url)) {
                    newArticle = true;
                    HttpContext.Response.Cookies.Append("lastArt", apiArticles.articles[0].url);
                }
            }
            else
            {
                apiArticles = await _newsApi.GetArticlesByDate(sources, startDate, endDate, page);
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
            HttpContext.Response.Cookies.Append("lastArt", apiArticles.articles[0].url);
            return PartialView("_PartialFeed", apiArticles);
        }

        public async Task<JsonResult> SavePushSub(string uid, string endPoint, string p256dh, string auth)
        {

            
            PushInfo pushInfo = new PushInfo();
            pushInfo.Name = uid; 
            pushInfo.EndPoint = endPoint;
            pushInfo.P256dh = p256dh;
            pushInfo.Auth = auth;

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
                    //HttpContext.Response.Cookies.Append("ep", pushInfo.EndPoint);
                    return Json(pushInfo.Name.ToString());
                }
                else
                {
                    pushInfo.Auth = auth;
                    pushInfo.P256dh = p256dh;
                    _newsFeedDbContext.PushInfos.Update(pushInfo);
                    await _newsFeedDbContext.SaveChangesAsync();
                    //HttpContext.Response.Cookies.Append("ep", pushInfo.EndPoint);
                    return Json(pushInfo.Name.ToString());
                }                
            }
            catch (DbUpdateException ex) {
                return Json("ERROR" + ex.GetBaseException().ToString());
            }

            
        }
    }
}
