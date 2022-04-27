using Microsoft.Extensions.Configuration;
using NewsFeedNet.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace NewsFeedNet.Services
{
    public class NewsApi: INewsApi
    {
        public static string _token;
        public static string _baseUrl;

        public NewsApi()
        {
            var configurationBuilder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();

            _token = configurationBuilder.GetSection("NewsApiSettings:token").Value;
            _baseUrl = configurationBuilder.GetSection("NewsApiSettings:baseUrl").Value;
        }

        public async Task<ApiArticles> GetArticles(string sources, string page)
        {
            ApiArticles apiArticles = new ApiArticles();
            apiArticles.articles = new List<Article>();
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(_baseUrl);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("X-Api-Key",_token);
            
            //var response = await httpClient.GetAsync($"/v2/everything?sources={sources}");
            var response = await httpClient.GetAsync($"/v2/top-headlines?pageSize=20&page={page}&sources={sources}");
            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                apiArticles = JsonConvert.DeserializeObject<ApiArticles>(jsonResponse);
                apiArticles.totalResults = apiArticles.totalResults > 100 ? apiArticles.totalResults = 100 : apiArticles.totalResults;

            }

            return apiArticles;

        }

        public async Task<ApiArticles> GetArticlesByDate(string sources, string startDate, string endDate, string page)
        {
            ApiArticles apiArticles = new ApiArticles();
            apiArticles.articles = new List<Article>();
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(_baseUrl);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("X-Api-Key", _token);

            var response = await httpClient.GetAsync($"/v2/everything?pageSize=20&page={page}&sources={sources}&from={startDate}&to={endDate}");

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                apiArticles = JsonConvert.DeserializeObject<ApiArticles>(jsonResponse);
                apiArticles.totalResults = apiArticles.totalResults > 100 ? apiArticles.totalResults = 100 : apiArticles.totalResults;
            }

            return apiArticles;
        }

        public async Task<List<Source>> GetSources(string[] categories)
        {
            List<Source> sources = new List<Source>();
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(_baseUrl);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("X-Api-Key", _token);
            
            foreach(string c in categories)
            {
                var response = await httpClient.GetAsync($"/v2/top-headlines/sources?category={c}&language=en");
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    sources.AddRange(JsonConvert.DeserializeObject<ApiSources>(jsonResponse).sources);
                }
            }
            //var response = await httpClient.GetAsync($"/v2/top-headlines/sources?category={category}");
            //var response = await httpClient.GetAsync($"/v2/top-headlines/sources?language=en");

            return sources;
        }
    }
}
