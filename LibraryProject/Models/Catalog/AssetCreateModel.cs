using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.Models.Catalog
{
    public class AssetCreateModel
    {

        public string Title { get; set; }
        public string AuthorOrDirector { get; set; }
        public int Year { get; set; }
        public string ISBN { get; set; }
        public string DeweyCallNumber { get; set; }
        public decimal Cost { get; set; }
        public int NumberOfCopies { get; set; }
        public int CurrentLocationId { get; set; }
        public string ImageUrl { get; set; }
    }
}
