using SMS.Models.Entities;

namespace SMS.Models.ViewModels
{
    public class StudentDashboardViewModel
    {
        public string StudentName { get; set; }
        public string RegId { get; set; }
        public List<Enrollment> MyCourses { get; set; }
        public double MyGPA { get; set; }
    }
}