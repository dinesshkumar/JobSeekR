namespace Job_1.Models
{
    public class Notification_table
    {

        public int notification_id { get; set; }
        public string? notification_title { get; set; }
        public string? notification_description { get; set; }
        public int? user_id { get; set; }
        public DateTime? notification_time { get; set; }
        public string? notification_status { get; set; }
        public string? job_application_active_status { get; set; }


}
}
