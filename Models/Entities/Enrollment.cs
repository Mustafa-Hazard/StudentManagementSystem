using System.ComponentModel.DataAnnotations;

namespace SMS.Models.Entities
{
    public class Enrollment
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Student select karna lazmi hai!")]
        public int StudentId { get; set; }
        public virtual Student? Student { get; set; }

        [Required(ErrorMessage = "Course select karna lazmi hai!")]
        public int CourseId { get; set; }
        public virtual Course? Course { get; set; }

        [Range(0, 100)]
        public int? Marks { get; set; }
        public DateTime EnrollmentDate { get; set; }


        public string? Grade { get; set; }
    }
}