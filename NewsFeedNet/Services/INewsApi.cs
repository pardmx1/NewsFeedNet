using NewsFeedNet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NewsFeedNet.Services
{
    public interface INewsApi
    {
        Task<List<Article>> GetArticles(string sources);
        Task<List<Source>> GetSources(string[] categories);

        Task<List<Article>> GetArticlesByDate(string sources, string startDate, string endDate);
    }
}
