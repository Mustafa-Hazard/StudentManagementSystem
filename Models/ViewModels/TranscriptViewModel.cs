using SMS.Models.Entities;

namespace SMS.Models.ViewModels
{
    public class TranscriptViewModel
    {
        public Student Student { get; set; }
        public List<Enrollment> Enrollments { get; set; }
        public double TotalGPA { get; set; }
        public string PrintedBy { get; set; } = "Mustafa Muhammad Iqbal"; //
    }
}