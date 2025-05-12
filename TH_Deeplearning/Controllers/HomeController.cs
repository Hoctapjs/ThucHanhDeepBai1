using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TH_Deeplearning.Models;
using System.IO;
using System.Reflection;

namespace TH_Deeplearning.Controllers
{
    public class HomeController : Controller
    {

        public ActionResult Home()
        {
            return View();
        }

        private List<string> ReadJobsFromFile(string filePath)
        {
            List<string> availableJobs = new List<string>();

            if (System.IO.File.Exists(filePath))
            {
                string[] lines = System.IO.File.ReadAllLines(filePath);
                foreach (string line in lines)
                {
                    string title = line.Trim().Replace("\"", "");
                    if (!string.IsNullOrEmpty(title) && !availableJobs.Contains(title))
                    {
                        availableJobs.Add(title);
                    }
                }
            }
            else
            {
                ViewBag.ErrorMessage = "Không tìm thấy file JobsDataset.csv";
                availableJobs = new List<string> { "Công việc 1", "Công việc 2", "Công việc 3" }; // Fallback
            }

            return availableJobs;
        }

        [HttpGet]
        public ActionResult SuggestTech_Skills()
        {
            string filePath = Server.MapPath("~/App_Data/JobsDataset.csv");
            var availableJobs = ReadJobsFromFile(filePath);

            var model = new JobViewModel
            {
                AvailableJobs = availableJobs,
                SelectedJob = null,
                SearchKeyword = "" 
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult SuggestTech_Skills(JobViewModel model)
        {
            string filePath = Server.MapPath("~/App_Data/JobsDataset.csv");
            var availableJobs = ReadJobsFromFile(filePath);

            if (!string.IsNullOrEmpty(model.SearchKeyword))
            {
                ViewBag.SearchResult = model.SearchKeyword;
                model.SelectedJob = null;
            }
            else if (!string.IsNullOrEmpty(model.SelectedJob) && availableJobs.Contains(model.SelectedJob))
            {
                ViewBag.SearchResult = model.SelectedJob;
            }
            else
            {
                if (!string.IsNullOrEmpty(model.SelectedJob) && !availableJobs.Contains(model.SelectedJob))
                {
                    System.Diagnostics.Debug.WriteLine($"SelectedJob '{model.SelectedJob}' không có trong danh sách AvailableJobs!");
                    model.SelectedJob = null;
                }
                ViewBag.SearchResult = null;
            }

            model.AvailableJobs = availableJobs;

            if (string.IsNullOrEmpty(model.SelectedJob) && string.IsNullOrEmpty(model.SearchKeyword))
            {
                model.SelectedJob = null;
            }

            ViewBag.SelectedJob = model.SelectedJob;
            return View(model);
        }

        [HttpGet]
        public ActionResult SuggestCareer()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SuggestCareer(string SearchKeyword)
        {
            if (!string.IsNullOrEmpty(SearchKeyword))
            {
                ViewBag.SearchResult = SearchKeyword;
            }
            else
            {
                ViewBag.SearchResult = null;
            }
            return View();
        }
    }
}