using System.ComponentModel.DataAnnotations;

namespace SMS.Models.Entities
{
    public class Enrollment
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int CourseId { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public int? Marks { get; set; }
        public string? Grade { get; set; }

        public virtual Student? Student { get; set; } // Nullable rakhein
        public virtual Course? Course { get; set; }   // Nullable rakhein
    }
}
