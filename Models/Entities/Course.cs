using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SMS.Models.Entities
{
    public class Course
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Course Code likhna lazmi hai.")]
        [StringLength(10, ErrorMessage = "Code 10 characters se zyada nahi ho sakta.")]
        [Display(Name = "Course Code")]
        public string CourseCode { get; set; } // e.g., CS-101

        [Required(ErrorMessage = "Course ka Title dena zaroori hai.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Title 3 se 100 characters ke darmiyan hona chahiye.")]
        [Display(Name = "Course Title")]
        public string Title { get; set; }

        [Range(1, 4, ErrorMessage = "Credits 1 se 4 ke darmiyan hone chahiye.")]
        [Display(Name = "Credit Hours")]
        public int Credits { get; set; }

        [Display(Name = "Description")]
        [DataType(DataType.MultilineText)]
        public string? Description { get; set; }

        // --- Relationships ---

        [Required(ErrorMessage = "Teacher assign karna lazmi hai.")]
        [Display(Name = "Assigned Teacher")]
        public int? TeacherId { get; set; } // Foreign Key

        [ForeignKey("TeacherId")]
        public virtual Teacher? Teacher { get; set; } // Navigation Property

        // Enrollments ko nullable (?) rakha hai validation fix ke liye
        public virtual ICollection<Enrollment>? Enrollments { get; set; }
    }
}