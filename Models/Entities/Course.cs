using System.ComponentModel.DataAnnotations;

namespace SMS.Models.Entities
{
    public class Course
    {
        public int Id { get; set; }
        [Required]
        public string CourseCode { get; set; } // e.g., CS101
        [Required]
        public string Title { get; set; }
        public int Credits { get; set; }

        public ICollection<Enrollment> Enrollments { get; set; }
    }
}
