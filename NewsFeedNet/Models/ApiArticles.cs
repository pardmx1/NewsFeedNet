using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NewsFeedNet.Models
{
    public class ApiArticles
    {
        public string status { get; set; }
        public int totalResuls { get; set; }
        public List<Article> articles { get; set; }
    }
}
