namespace SMS.Models
{
    public class DashboardViewModel
    {
        public int TotalStudents { get; set; }
        public int TotalCourses { get; set; }
        public int TotalEnrollments { get; set; }
        public double AverageMarks { get; set; }

        // Recent activity ke liye lists
        public List<SMS.Models.Entities.Student> RecentStudents { get; set; }
        public List<SMS.Models.Entities.Enrollment> RecentEnrollments { get; set; }
    }
}