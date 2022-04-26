using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace NewsFeedNet.Models
{
    public class FeedViewModel
    {
        public int refreshTime { get; set; }
        public DateTime? startDate { get; set; } = null;
        public DateTime? endDate { get; set; } = null;
        public List<Article> articles { get; set; } = new List<Article>();
    }
}
