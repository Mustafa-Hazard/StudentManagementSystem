using SMS.Models.Entities;

namespace SMS.Models.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalStudents { get; set; }
        public int TotalCourses { get; set; }
        public int TotalEnrollments { get; set; }
        public double AverageMarks { get; set; }
        public List<Student> RecentStudents { get; set; }
        public List<Enrollment> RecentEnrollments { get; set; }
    }
}