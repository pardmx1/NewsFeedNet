using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NewsFeedNet.Models
{
    public class ApiSources
    {
        public string status { get; set; }
        //public int totalResuls { get; set; }
        public List<Source> sources { get; set; }
    }
}
