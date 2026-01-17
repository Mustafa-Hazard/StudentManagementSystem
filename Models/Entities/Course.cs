using System.ComponentModel.DataAnnotations;

namespace SMS.Models.Entities
{
    public class Course
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Yar, Course Code lazmi hai!")]
        public string CourseCode { get; set; } // e.g., CS101

        [Required(ErrorMessage = "Title bhool gaye aap?")]
        public string Title { get; set; }

        public int Credits { get; set; }

        // 👈 Change: Nullable (?) banaya hai taake validation fail na ho
        public virtual ICollection<Enrollment>? Enrollments { get; set; }
    }
}