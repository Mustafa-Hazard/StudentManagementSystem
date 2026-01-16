using System.ComponentModel.DataAnnotations;

namespace SMS.Models.Entities
{
    public class Attendance
    {
        public int Id { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Required]
        public bool IsPresent { get; set; }

        // Foreign Key for Student
        [Required]
        public int StudentId { get; set; }
        public Student Student { get; set; }

        // Foreign Key for Course (Attendance is usually per-course session)
        [Required]
        public int CourseId { get; set; }
        public Course Course { get; set; }
    }
}