using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Text;
using System.Web.Mvc;
using TH_Deeplearning.Models;
using Newtonsoft.Json;

namespace TH_Deeplearning.Controllers
{
    public class HomeController : Controller
    {
        private readonly string apiUrl = ConfigurationManager.AppSettings["ApiUrl"];

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
            return View(new JobSuggestion());
        }

        [HttpPost]
        public ActionResult SuggestCareer(string SearchKeyword)
        {
            if (string.IsNullOrEmpty(SearchKeyword))
            {
                ViewBag.ErrorMessage = "Vui lòng nhập từ khóa tìm kiếm.";
                return View(new JobSuggestion());
            }

            using (var client = new HttpClient())
            {
                try
                {
                    // Chuẩn bị payload cho API Flask
                    var payload = new { prompt = SearchKeyword };
                    var content = new StringContent(
                        JsonConvert.SerializeObject(payload),
                        Encoding.UTF8,
                        "application/json"
                    );

                    // Gửi yêu cầu POST tới API Flask
                    HttpResponseMessage response = client.PostAsync(apiUrl, content).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        string jsonResponse = response.Content.ReadAsStringAsync().Result;
                        var jobSuggestion = JsonConvert.DeserializeObject<JobSuggestion>(jsonResponse);

                        // Truyền dữ liệu tới View
                        ViewBag.SearchResult = SearchKeyword;
                        return View(jobSuggestion);
                    }
                    else
                    {
                        ViewBag.ErrorMessage = $"Lỗi: {response.ReasonPhrase}";
                        return View(new JobSuggestion());
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.ErrorMessage = $"Lỗi: {ex.Message}";
                    return View(new JobSuggestion());
                }
            }
        }
    }
}