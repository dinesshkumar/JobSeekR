namespace Job_1.Models
{
    public class JobTable
    {
        public int job_id { get; set; }
        public string? job_title { get; set; }
        public string? job_description { get; set; }
        public string? job_requirements { get; set; }
        public string? job_company { get; set; }
        public string? job_category { get; set; }
        public string? job_location { get; set; }
        public int job_recruiter_id { get; set; }
        public bool? job_active_status { get; set; }
        public DateTime? job_posted_at { get; set; }

    }
}
