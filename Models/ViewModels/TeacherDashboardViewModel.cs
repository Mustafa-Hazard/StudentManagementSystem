using SMS.Models.Entities;

namespace SMS.Models.ViewModels
{
    public class TeacherDashboardViewModel
    {
        public string TeacherName { get; set; }
        public int AssignedCoursesCount { get; set; }
        public int TotalStudentsCount { get; set; }
        public List<Course> MyCourses { get; set; }
        public List<Attendance> RecentAttendances { get; set; }
    }
}