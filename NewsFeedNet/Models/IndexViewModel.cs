using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NewsFeedNet.Models
{
    public class IndexViewModel
    {
        public enum Category
        {
            business,
            entertaiment,
            general,
            sports,
            technology
        }

        public Category CategoryType { get; set; }
        [Required]
        public int refreshFreq { get; set; } = 10;
        [Required]
        public List<string> sources { get; set; } = new List<string>();
    }
}
