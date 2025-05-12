using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace TH_Deeplearning.Models
{
    public class JobViewModel
    {
        [Display(Name = "Chọn công việc")]
        public string SelectedJob { get; set; }

        public List<string> AvailableJobs { get; set; }

        [Display(Name = "Từ khóa tìm kiếm")]
        public string SearchKeyword { get; set; } // Thêm thuộc tính để binding dữ liệu từ khung tìm kiếm

        public JobViewModel()
        {
            AvailableJobs = new List<string>();
            SearchKeyword = ""; // Giá trị mặc định là chuỗi rỗng
        }
    }
}