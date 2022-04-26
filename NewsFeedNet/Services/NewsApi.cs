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

        public async Task<List<Article>> GetArticles(string sources)
        {
            List<Article> articles = new List<Article>();
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(_baseUrl);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("X-Api-Key",_token);
            
            //var response = await httpClient.GetAsync($"/v2/everything?sources={sources}");
            var response = await httpClient.GetAsync($"/v2/top-headlines?sources={sources}");
            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                articles = JsonConvert.DeserializeObject<ApiArticles>(jsonResponse).articles;

            }

            return articles;

        }

        public async Task<List<Article>> GetArticlesByDate(string sources, string startDate, string endDate)
        {
            List<Article> articles = new List<Article>();
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(_baseUrl);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("X-Api-Key", _token);

            var response = await httpClient.GetAsync($"/v2/everything?sources={sources}&from={startDate}&to={endDate}");

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                articles = JsonConvert.DeserializeObject<ApiArticles>(jsonResponse).articles;

            }

            return articles;
        }

        public async Task<List<Source>> GetSources(string category)
        {
            List<Source> sources = new List<Source>();
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(_baseUrl);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("X-Api-Key", _token);
            

            //var response = await httpClient.GetAsync($"/v2/top-headlines/sources?category={category}");
            var response = await httpClient.GetAsync($"/v2/top-headlines/sources?language=en");

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                sources = JsonConvert.DeserializeObject<ApiSources>(jsonResponse).sources;               
            }

            return sources;
        }
    }
}
