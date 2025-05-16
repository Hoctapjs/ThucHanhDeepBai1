namespace TH_Deeplearning.Models
{
    public class JobSuggestion
    {
        public string Response { get; set; }
        public Reference Reference { get; set; }
    }

    public class Reference
    {
        public string Title { get; set; }
        public string Description { get; set; }
    }
}