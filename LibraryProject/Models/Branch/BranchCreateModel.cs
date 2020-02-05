using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.Models.Branch
{
    public class BranchCreateModel
    {
        public string Address { get; set; }
        public string BranchName { get; set; }
        public string Telephone { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
    }
}
