using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NewsAPI;
using NewsAPI.Constants;
using NewsAPI.Models;
using Outbreak.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Outbreak.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            List<string> virusList = new List<string>();

            string line;

            // Read the file and display it line by line.  
            System.IO.StreamReader file =
                new System.IO.StreamReader("Controllers/viruses.txt");
            while ((line = file.ReadLine()) != null)
            {
                virusList.Add(line);
            }

            file.Close();

            return View(virusList);
        }

        [HttpPost]
        public IActionResult Results(string searchTerm)
        {
            var newsApiClient = new NewsApiClient("0fd2bcfa9ab24fdb97eb9f05847f01a9");
            List<string> articleList = new List<string>();
            int relevantCount = 0;
            int thisMonthCount = 0;

            var articlesResponse = newsApiClient.GetEverything(new EverythingRequest
            {
                Q = searchTerm,
                SortBy = SortBys.Relevancy,
                Language = Languages.EN,
            });
            if (articlesResponse.Status == Statuses.Ok)
            {
                if (articlesResponse.TotalResults > 0)
                {
                    foreach (var article in articlesResponse.Articles)
                    {

                        articleList.Add(article.Title + " " + article.PublishedAt.Value.ToShortDateString() + " @! " + article.Url);


                        if (article.Title.Contains(searchTerm.Split(" ")[0]))
                        {
                            relevantCount++;
                        }

                        if (article.PublishedAt.Value.Month == DateTime.Today.Month)
                        {
                            thisMonthCount++;
                        }
                    }
                }
                else
                {
                    articleList.Add("No results found for this virus.");
                }
            }

            ViewData["searchResultsCount"] = articlesResponse.TotalResults;
            ViewData["searchTerm"] = searchTerm;
            ViewData["relevantCount"] = relevantCount;
            ViewData["thisMonthCount"] = thisMonthCount;

            return View(articleList);
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
