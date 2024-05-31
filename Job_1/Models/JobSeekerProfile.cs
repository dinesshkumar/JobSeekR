namespace Job_1.Models
{
    public class JobSeekerProfile
    {
      public int job_seeker_id { get; set; }
        public int user_id { get; set; }
        public string? job_seeker_name { get; set; }
      public string? job_seeker_profile_summary { get; set; }
      public string? job_seeker_skills { get; set; }
      public string? job_seeker_education { get; set; }
      public string? job_seeker_experience { get; set; }
      public string? job_seeker_achievement { get; set; }
        public string? job_seeker_location { get; set; }
        public bool job_seeker_active_status { get; set; }
      public string? job_seeker_resume_file_path { get; set; }
      public string? job_seeker_cover_file_path { get; set; }
    }
}