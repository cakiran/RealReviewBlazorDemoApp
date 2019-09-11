using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RealReviewBlazorDemo.Server.Helpers;
using RealReviewBlazorDemo.Shared;

namespace RealReviewBlazorDemo.Server.Controllers
{
    [Route("api/businesses")]
    [ApiController]
    public class YelpController : ControllerBase
    {
        private readonly IWebHostEnvironment _hostingEnvironment;

        public YelpController(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }
        [HttpGet]
        [Route("{searchstring}/{location}")]
        public async Task<IEnumerable<Business>> GetBusinesses(string location, string searchstring)
        {
            if (string.IsNullOrEmpty(location) || string.IsNullOrEmpty(searchstring))
                return new List<Business>();
            List<Business> lstBusiness = new List<Business>();
            var client = new Yelp.Api.Client("<apikey>");
            Yelp.Api.Models.SearchRequest searchRequest = new Yelp.Api.Models.SearchRequest();
            searchRequest.Location = location;
            searchRequest.Term = searchstring;
            var results = await client.SearchBusinessesAllAsync(searchRequest);
            Yelp.Api.Models.SearchResponse searchResponse = results;
            var businesses = searchResponse.Businesses;
            foreach (var b in businesses)
            {
                var business = new Business();
                business.Name = b.Name.Trim();
                business.Address = b.Location.Address1.Trim();
                business.City = b.Location.City.Trim();
                business.ZipCode = b.Location.ZipCode.Trim();
                business.State = b.Location.State.Trim();
                business.Phone = b.Phone.Trim();
                var reviewresult = await client.GetReviewsAsync(b.Id);
                var revs = reviewresult.Reviews;
                business.Reviews = revs.Select(x => Regex.Replace(x.Text.Trim(), @"\r\n?|\n", "")).ToArray();
                lstBusiness.Add(business);
            }
            return lstBusiness;
        }

        [HttpPost]
        [Route("realreviewscore")]
        public RealReviewScoreData GetRealReviewScore(Business business)
        {
            string webRootPath = _hostingEnvironment.ContentRootPath + @"\Data\yelp_labelled.txt";
            RealReviewScoreData realReviewScoreData = new RealReviewScoreData();
            BinaryClassifierPipeline _binaryClassifierPipeline = new BinaryClassifierPipeline(webRootPath);
            float allProbability = 0;
            realReviewScoreData.Accuracy = _binaryClassifierPipeline.Accuracy;
            realReviewScoreData.AreaUnderROCCurve = _binaryClassifierPipeline.AreaUnderROCCurve;
            realReviewScoreData.F1Score = _binaryClassifierPipeline.F1Score;
            foreach (var reviewTxt in business.Reviews)
            {
                allProbability += _binaryClassifierPipeline.GetProbabilityByUsingModelWithSingleItem(reviewTxt) * 100;
            }
            realReviewScoreData.RealReviewScore = Convert.ToInt32(allProbability / 3);
            return realReviewScoreData;
        }
    }
}