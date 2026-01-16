using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace SMS.Models.Entities
{
    public class Student
    {
        public int Id { get; set; }

        [Required]
        public string StudentRegId { get; set; }

        public string? Department { get; set; }

        public string? UserId { get; set; }

        [ValidateNever] // Isse enrollment ka error khatam ho jayega
        public ApplicationUser? User { get; set; }

        public DateTime DateOfBirth { get; set; }

        // CHANGES HERE: Isay nullable list banayein
        [ValidateNever]
        public ICollection<Enrollment>? Enrollments { get; set; }
    }
}