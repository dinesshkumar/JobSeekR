using Job_1.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Runtime.Intrinsics.Arm;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Collections.Generic;
using System.Runtime.CompilerServices;


namespace Job_1.Controllers
{
    static class Global
    {
        public static string global_user_id;
        public static string global_user_name;
        public static string JobTitleQualificationDomainSkillsKeywordorCompany;
        public static string location;

    }


    public class HomeController : Controller
    {


        string global_connectionString = "Data Source=(localdb)\\dinessh_local;Initial Catalog=jobSearchDatabase;Integrated Security=True";

        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult Register()
        {
            return View("~/Views/Register.cshtml");
        }
        public string GenerateHash(string userPassword) {
            //Generating Password
            SHA512 sha512 = SHA512.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(userPassword);
            byte[] hash = sha512.ComputeHash(bytes);
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                result.Append(hash[i].ToString("X2"));
            }
            var hashedPassword = result.ToString();
            hashedPassword = hashedPassword.Substring(0, 45);
            return hashedPassword;
        }

        public IActionResult AccountRegister(string firstName, string lastname, string userName, string email, string password, string confirmPassword, string contactNumber, string userType)

        {
            string emailID = null;
            string connectionString = "Data Source=(localdb)\\dinessh_local;Initial Catalog=jobSearchDatabase;Integrated Security=True";
            string user_email_sql = "SELECT user_email FROM user_table where user_email='" + email + "'";
            using (SqlConnection conn = new(connectionString))
            {
                SqlCommand cmd = new SqlCommand(user_email_sql, conn);
                conn.Open();
                SqlDataReader rdr = cmd.ExecuteReader();
                if (rdr.Read())
                {
                    emailID = rdr[0].ToString();
                }
                conn.Close();
            }
            if (emailID != null)
            {
                return View("~/Views/Register_Fails.cshtml");
            }
            else
            {

                string created_at = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff");
                password = GenerateHash(password);

                //Inserting User Table
                string user_id = null;
                string insertUserTable_sql = "INSERT INTO user_table (user_name, user_password, user_first_name, user_last_name, user_email, user_contact, user_type, last_active, created_at, user_status, user_active_status)\r\nVALUES ('" + userName + "','" + password + "','" + firstName + "','" + lastname + "','" + email + "','" + contactNumber + "','" + userType + "','" + created_at + "','" + created_at + "',1,1)";
                string getUserID_sql = "select user_id from user_table where user_email='" + email + "'";
                using (SqlConnection conn = new(connectionString))
                {
                    SqlCommand cmd = new SqlCommand(insertUserTable_sql, conn);
                    SqlCommand getUserID = new SqlCommand(getUserID_sql, conn);

                    conn.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    conn.Close();
                    conn.Open();
                    rdr = getUserID.ExecuteReader();
                    if (rdr.Read())
                    {
                        user_id = rdr[0].ToString();

                    }
                    string insertProfileTable_sql = null; 
                    if (userType == "employee")
                    {
                        insertProfileTable_sql = "INSERT INTO job_seeker_profile (user_id) VALUES ('" + user_id + "')";
                    }
                    if (userType == "employer")
                    {
                        insertProfileTable_sql = "INSERT INTO job_recruiter_profile (user_id) VALUES ('" + user_id + "')";
                    }
                    SqlCommand cmd_profile = new SqlCommand(insertProfileTable_sql, conn);
                    conn.Close();
                    conn.Open();
                    rdr = cmd_profile.ExecuteReader();
                    conn.Close();
                }

                return View("~/Views/Register_Success.cshtml");
            }


        }
        public IActionResult EmployeeHomePage()
        {
            ViewBag.user_name = Global.global_user_name;
            return View("~/Views/Employee/Pages/EmployeeHomePage.cshtml");
        }
        public IActionResult EmployeeJobSearchPage()
        {
            return View("~/Views/Employee/Pages/EmployeeJobSearchPage.cshtml");
        }

        public IActionResult EmployeeJobSearchResultsPage(string JobTitleQualificationDomainSkillsKeywordorCompany, string location)
        {
            Global.JobTitleQualificationDomainSkillsKeywordorCompany = JobTitleQualificationDomainSkillsKeywordorCompany;
            Global.location = location;
            string[] keywords = JobTitleQualificationDomainSkillsKeywordorCompany.Split(' ');
            string[] column_names = ["job_title", "job_description", "job_requirements", "job_company", "job_category"];
            string start_query = "select * from job_table where job_active_status=1 and (";
            string middle_query = "";
            string end_query = " order by job_posted_at desc";
            middle_query += start_query;
            int i = 0;
            foreach (var columns in column_names) {
                if (i == 0) { 
                    i++;
            }
                else
                    middle_query += " or ";


                foreach (var word in keywords)
                {
                    middle_query += columns +" like '%"+ word+"%'";
                }
               
            }

            string full_query = middle_query + ") and job_location like '%" + location + "%'" + end_query;


            using (SqlConnection conn = new(global_connectionString))
            {
                SqlCommand cmd = new SqlCommand(full_query, conn);
                conn.Open();
                SqlDataReader rdr = cmd.ExecuteReader();
                var jobs = new List<JobTable>();

                while (rdr.Read())
                {
                    var jobTable = new JobTable
                    {
                        job_id = (int)(rdr["job_id"]),
                        job_title = (string)(rdr["job_title"]),
                        job_description = (string)(rdr["job_description"]),
                        job_requirements = (string)rdr["job_requirements"],
                        job_company = (string)(rdr["job_company"]),
                        job_category = (string)(rdr["job_category"]),
                        job_location = (string)(rdr["job_location"]),
                        job_posted_at = (DateTime)(rdr["job_posted_at"])

                    };

                    jobs.Add(jobTable);
                }
                ViewBag.jobs = jobs;
                conn.Close();
            }
            return View("~/Views/Employee/Pages/EmployeeJobSearchResultsPage.cshtml");
        }
        public IActionResult EmployeeJobApplyButton( string job_id)
        {
            string applied_at = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff");

            string get_job_title = "select job_title,job_recruiter_id from job_table where job_id='" + job_id+"'";


            using (SqlConnection conn = new(global_connectionString))
            {
                SqlCommand cmd_title = new SqlCommand(get_job_title, conn);
                string job_title = null;
                string job_recruiter_id = null;
                conn.Open();
                SqlDataReader rdr_title = cmd_title.ExecuteReader();
                if (rdr_title.Read())
                {
                    job_title = rdr_title[0].ToString();
                    job_recruiter_id = rdr_title[1].ToString();

                }
                conn.Close();

                string insertJobApplication_sql = "INSERT INTO job_application_table(job_id, job_seeker_id, job_title, job_application_status, job_application_active_status, applied_at) VALUES ('" + job_id + "','" + job_recruiter_id + "','" + job_title + "','Applied',1,'" + applied_at + "')";
                string insertNotification_sql = "INSERT INTO notification_table(notification_title, notification_description, user_id, notification_time, notification_status, job_application_active_status) VALUES ('New Application Received','" + job_title + "','" + job_recruiter_id + "','" + applied_at + "','New',1)";
                SqlCommand cmd = new SqlCommand(insertJobApplication_sql, conn);
                SqlCommand cmd_n = new SqlCommand(insertNotification_sql, conn);
                conn.Open();
                SqlDataReader rdr = cmd.ExecuteReader();
                conn.Close();
                conn.Open();
                SqlDataReader rdr_n = cmd_n.ExecuteReader();
                conn.Close();
            }

            return EmployeeJobSearchResultsPage(Global.JobTitleQualificationDomainSkillsKeywordorCompany, Global.location);
        }
        public IActionResult EmployeeManageJobsPage()
        {
            return View("~/Views/Employee/Pages/EmployeeManageJobsPage.cshtml");
        }
        public IActionResult EmployeeProfilePage()
        {
            string getProfile_sql = "select * from job_seeker_profile where user_id='"+ Global.global_user_id + "'";

            using (SqlConnection conn = new(global_connectionString))
            {
                SqlCommand cmd = new SqlCommand(getProfile_sql, conn);
                conn.Open();
                SqlDataReader rdr = cmd.ExecuteReader();

                if (rdr.Read())
                {
                    try
                    {

                        var jobSeekersProfile = new JobSeekerProfile
                        {
                            job_seeker_name = (string)(rdr["job_seeker_name"]),

                            job_seeker_profile_summary = (string)(rdr["job_seeker_profile_summary"]),
                            job_seeker_skills = (string)rdr["job_seeker_skills"],
                            job_seeker_education = (string)(rdr["job_seeker_education"]),
                            job_seeker_experience = (string)(rdr["job_seeker_experience"]),
                            job_seeker_location = (string)(rdr["job_seeker_location"]),
                            job_seeker_resume_file_path = (string)(rdr["job_seeker_resume_file_path"]),
                            job_seeker_cover_file_path = (string)(rdr["job_seeker_cover_file_path"]),
                            job_seeker_achievement = (string)(rdr["job_seeker_achievement"])
                        };

                        ViewBag.jobSeekerProfile = jobSeekersProfile;
                    }
                    catch (Exception e)
                    {
                        var jobSeekersProfile = new JobSeekerProfile
                        {
                            job_seeker_name = "",
                            job_seeker_profile_summary = "",
                            job_seeker_skills = "",
                            job_seeker_education = "",
                            job_seeker_experience = "",
                            job_seeker_location = "",
                            job_seeker_resume_file_path = "",
                            job_seeker_cover_file_path = "",
                            job_seeker_achievement = ""
                        };
                        ViewBag.jobSeekerProfile = jobSeekersProfile;
                    }

                }
                conn.Close();
            }
            ViewBag.user_id = Global.global_user_id;

            return View("~/Views/Employee/Pages/EmployeeProfilePage.cshtml");

        }

        public IActionResult EmployeeProfileSave(string fullName, string profileSummary, string skills, string education, string experience, string achievement, string uploadResume, string uploadCoverLetter)
        {
           
            string insertProfileTable_sql = "UPDATE job_seeker_profile SET job_seeker_name = '" + fullName + "', job_seeker_profile_summary = '" + profileSummary + "',job_seeker_skills = '" + skills + "', job_seeker_education = '" + education + "', job_seeker_experience = '" + experience + "', job_seeker_achievement = '" + achievement + "', job_seeker_active_status = 1, job_seeker_resume_file_path = '" + uploadResume + "' , job_seeker_cover_file_path = '" + uploadCoverLetter + "' WHERE user_id='" + Global.global_user_id + "'";
            using (SqlConnection conn = new(global_connectionString))
                {
                    SqlCommand cmd = new SqlCommand(insertProfileTable_sql, conn);
                    conn.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    conn.Close();
                }
                return EmployeeProfilePage();
        }
            public IActionResult EmployeeProfileDownload()
            {
                return EmployeeProfilePage();
            }
            public IActionResult EmployeeAccountPage()
            {
            string getProfile_sql = "select * from user_table where user_id='" + Global.global_user_id + "'";

            using (SqlConnection conn = new(global_connectionString))
            {
                SqlCommand cmd = new SqlCommand(getProfile_sql, conn);
                conn.Open();
                SqlDataReader rdr = cmd.ExecuteReader();
                if (rdr.Read())
                {
                    var userTable = new UserTable
                    {
                        user_first_name = (string)(rdr["user_first_name"]),
                        user_last_name = (string)(rdr["user_last_name"]),
                        user_email = (string)rdr["user_email"],
                        user_name = (string)(rdr["user_name"]),
                        user_contact = (string)(rdr["user_contact"])

                    };

                    ViewBag.userTable = userTable;

                }
                conn.Close();
            }

;

            return View("~/Views/Employee/Pages/EmployeeAccountPage.cshtml");
            }
            public IActionResult EmployeeNotificationsPage()
            {
                return View("~/Views/Employee/Pages/EmployeeNotificationsPage.cshtml");
            }
            public IActionResult EmployeeLogout()
            {
                return View("~/Views/Employee/EmployeeLoginPage.cshtml");
            }
            public IActionResult EmployerPreLogin()
            {
                return View("~/Views/Employer/EmployerLoginPage.cshtml");
            }
            public IActionResult EmployeePreLogin()
            {
                return View("~/Views/Employee/EmployeeLoginPage.cshtml");
            }
            public IActionResult EmployeeLogin(string userEmail, string userPassword)
            {
                
                string user_id = null, user_first_name = null;
                string hashedPassword = GenerateHash(userPassword);


                string connectionString = "Data Source=(localdb)\\dinessh_local;Initial Catalog=jobSearchDatabase;Integrated Security=True";
                string sql = "SELECT user_id,user_first_name FROM user_table WHERE user_email ='" + userEmail + "' and user_password='" + hashedPassword + "' and user_type='employee'";

            //var model = new List<AccountsModel>();
            using (SqlConnection conn = new(connectionString))
                {
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    conn.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();


                    if (rdr.Read())
                    {
                        user_id = rdr[0].ToString();
                    ViewBag.user_id = user_id;
                    Global.global_user_id = user_id;
                        user_first_name = rdr[1].ToString();
                        ViewBag.userName = user_first_name;
                    Global.global_user_name = user_first_name;
    }
                    conn.Close();

                }

                if (user_id == null)
                {
                    return View("~/Views/Employee/EmployeeLoginFail.cshtml");
                }

                return EmployeeHomePage();
            }
            public IActionResult EmployerLogin(string userEmail, string userPassword)
            {
                ViewBag.UserEmail = userEmail;
                string user_id = null, user_first_name = null;
                string hashedPassword = GenerateHash(userPassword);


               string sql = "SELECT user_id,user_first_name FROM user_table WHERE user_email ='" + userEmail + "' and user_password='" + hashedPassword + "' and user_type='employer'";

                //var model = new List<AccountsModel>();
                using (SqlConnection conn = new(global_connectionString))
                {
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    conn.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();


                    if (rdr.Read())
                    {
                    user_id = rdr[0].ToString();
                    ViewBag.user_id = user_id;
                    Global.global_user_id = user_id;
                    user_first_name = rdr[1].ToString();
                    ViewBag.user_Name = user_first_name;
                    Global.global_user_name = user_first_name;
                }
                    conn.Close();
                }

                if (user_id == null)
                {
                    return View("~/Views/Employer/EmployerLoginFail.cshtml");
                }

                return View("~/Views/Employer/Pages/EmployerHomePage.cshtml");
            }

            public IActionResult EmployerHomePage()
            {
            ViewBag.user_name = Global.global_user_name;

            return View("~/Views/Employer/Pages/EmployerHomePage.cshtml");
            }
            public IActionResult EmployerCandidateSearchPage()
            {
                return View("~/Views/Employer/Pages/EmployerCandidateSearchPage.cshtml");
            }

            public IActionResult EmployerCandidateSearchResultsPage(string JobTitleQualificationDomainSkillsKeywordorCompany, string location)
            {
            Global.JobTitleQualificationDomainSkillsKeywordorCompany = JobTitleQualificationDomainSkillsKeywordorCompany;
            Global.location = location;
            string[] keywords = JobTitleQualificationDomainSkillsKeywordorCompany.Split(' ');
            string[] column_names = ["job_seeker_profile_summary", "job_seeker_skills", "job_seeker_education", "job_seeker_experience", "job_seeker_achievement"];
            string start_query = "select * from job_seeker_profile where job_seeker_active_status=1 and (";
            string middle_query = "";
            string end_query = " order by job_seeker_experience desc";
            middle_query += start_query;
            int i = 0;
            foreach (var columns in column_names)
            {
                if (i == 0)
                {
                    i++;
                }
                else
                    middle_query += " or ";


                foreach (var word in keywords)
                {
                    middle_query += columns + " like '%" + word + "%'";
                }

            }

            string full_query = middle_query + ") and job_seeker_location like '%" + location + "%'" + end_query;


          
            using (SqlConnection conn = new(global_connectionString))
            {
                SqlCommand cmd = new SqlCommand(full_query, conn);
                conn.Open();
                SqlDataReader rdr = cmd.ExecuteReader();
                var jobSeekerProfiles = new List<JobSeekerProfile>();

                while (rdr.Read())
                {
                        var jobSeekersProfile = new JobSeekerProfile
                        {
                            job_seeker_name = (string)(rdr["job_seeker_name"]),
                            job_seeker_profile_summary = (string)(rdr["job_seeker_profile_summary"]),
                            job_seeker_skills = (string)rdr["job_seeker_skills"],
                            job_seeker_education = (string)(rdr["job_seeker_education"]),
                            job_seeker_experience = (string)(rdr["job_seeker_experience"]),
                            job_seeker_location = (string)(rdr["job_seeker_location"]),
                            job_seeker_resume_file_path = (string)(rdr["job_seeker_resume_file_path"]),
                            job_seeker_cover_file_path = (string)(rdr["job_seeker_cover_file_path"]),
                            job_seeker_achievement = (string)(rdr["job_seeker_achievement"])

                        };

                        jobSeekerProfiles.Add(jobSeekersProfile);
                }
                ViewBag.jobSeekerProfiles = jobSeekerProfiles;
                conn.Close();
            }


            return View("~/Views/Employer/Pages/EmployerCandidateSearchResultsPage.cshtml");
            }

            public IActionResult EmployerPostJobsPage()
            {


                return View("~/Views/Employer/Pages/EmployerPostJobsPage.cshtml");
            }
        public IActionResult EmployerPostJobs(string jobTitle, string jobDecription, string requirments, string company, string category, string location)
        {
            string job_posted_at = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff");

            string insertJobTable_sql = "INSERT INTO job_table (job_title, job_description, job_requirements, job_company, job_category, job_location, job_recruiter_id, job_active_status, job_posted_at) VALUES ('" + jobTitle + "','" + jobDecription + "','" + requirments + "','" + company + "','" + category + "','" + location + "','" + Global.global_user_id +"',1,'"+job_posted_at+"')";

            //var model = new List<AccountsModel>();
            using (SqlConnection conn = new(global_connectionString))
            {
                SqlCommand cmd = new SqlCommand(insertJobTable_sql, conn);
                conn.Open();
                SqlDataReader rdr = cmd.ExecuteReader();
                conn.Close();
            }

            return View("~/Views/Employer/Pages/EmployerPostJobsPage.cshtml");
        }
        public IActionResult EmployerJobApplyButton()
            {
                return EmployerCandidateSearchResultsPage(Global.JobTitleQualificationDomainSkillsKeywordorCompany, Global.location) ;
            }
            public IActionResult EmployerManageJobsPage()
            {
            string get_job_sql = "select * from job_table where job_recruiter_id='" + Global.global_user_id + "' and job_active_status=1";
            var listJobTables = new List<JobTable>();
            using (SqlConnection conn = new(global_connectionString))
            {
                SqlCommand cmd = new SqlCommand(get_job_sql, conn);
                conn.Open();
                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    var jobTable = new JobTable
                    {
                        job_title = (string)(rdr["job_title"]),
                        job_description = (string)(rdr["job_description"]),
                        job_requirements = (string)rdr["job_requirements"],
                        job_company = (string)(rdr["job_company"]),
                        job_category = (string)(rdr["job_category"]),
                        job_location = (string)(rdr["job_location"]),
                        job_posted_at = (DateTime)(rdr["job_posted_at"])

                    };

                    
                    listJobTables.Add(jobTable);
                }
                ViewBag.jobTable = listJobTables;
                conn.Close();
            }
            ViewBag.user_id = Global.global_user_id;


            return View("~/Views/Employer/Pages/EmployerManageJobsPage.cshtml");
            }
            public IActionResult EmployerManageApplications()
            {
                return View("~/Views/Employer/Pages/EmployerManageApplications.cshtml");
            }
            public IActionResult EmployerProfilePage()
            {
                return View("~/Views/Employer/Pages/EmployerProfilePage.cshtml");

            }
            public IActionResult EmployerAccountPage()
            {
            string getProfile_sql = "select * from user_table where user_id='" + Global.global_user_id + "'";

            using (SqlConnection conn = new(global_connectionString))
            {
                SqlCommand cmd = new SqlCommand(getProfile_sql, conn);
                conn.Open();
                SqlDataReader rdr = cmd.ExecuteReader();
                if (rdr.Read())
                {
                    var userTable = new UserTable
                    {
                        user_first_name = (string)(rdr["user_first_name"]),
                        user_last_name = (string)(rdr["user_last_name"]),
                        user_email = (string)rdr["user_email"],
                        user_name = (string)(rdr["user_name"]),
                        user_contact = (string)(rdr["user_contact"])

                    };

                    ViewBag.userTable = userTable;

                }
                conn.Close();
            }
            return View("~/Views/Employer/Pages/EmployerAccountPage.cshtml");
            }
            public IActionResult EmployerNotificationsPage()
            {
            string get_job_sql = "select * from notification_table where user_id='" + Global.global_user_id + "'";
            var listnotification_table = new List<Notification_table>();
            using (SqlConnection conn = new(global_connectionString))
            {
                SqlCommand cmd = new SqlCommand(get_job_sql, conn);
                conn.Open();
                SqlDataReader rdr = cmd.ExecuteReader();
                
                while (rdr.Read())
                {
                    var notification_table = new Notification_table
                    {
                        notification_title = (string)(rdr["notification_title"]),
                        notification_description = (string)(rdr["notification_description"]),
                        notification_time = (DateTime)rdr["notification_time"]

                    };


                    listnotification_table.Add(notification_table);
                }
                ViewBag.notification_table = listnotification_table;
                conn.Close();
            }
            ViewBag.user_id = Global.global_user_id;

            return View("~/Views/Employer/Pages/EmployerNotificationsPage.cshtml");
            }

            public IActionResult EmployerLogout()
            {
                return View("~/Views/Employer/EmployerLoginPage.cshtml");
            }




            [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
            public IActionResult Error()
            {
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }

        }
}
