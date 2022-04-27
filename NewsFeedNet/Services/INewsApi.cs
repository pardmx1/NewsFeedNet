using NewsFeedNet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NewsFeedNet.Services
{
    public interface INewsApi
    {
        Task<ApiArticles> GetArticles(string sources, string page);
        Task<List<Source>> GetSources(string[] categories);

        Task<ApiArticles> GetArticlesByDate(string sources, string startDate, string endDate, string page);
    }
}
