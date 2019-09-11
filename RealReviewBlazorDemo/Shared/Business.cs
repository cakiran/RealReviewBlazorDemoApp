using System;
using System.Collections.Generic;
using System.Text;

namespace RealReviewBlazorDemo.Shared
{
    public class Business
    {
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string City { get; set; }
        public string[] Reviews { get; set; }
    }
}
