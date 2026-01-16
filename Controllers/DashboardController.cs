using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SMS.Models;
using Microsoft.AspNetCore.Authorization;

namespace SMS.Controllers
{
    [Authorize(Roles = "Admin,Teacher")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Simple counts
            int totalStudents = await _context.Students.CountAsync();
            int totalCourses = await _context.Courses.CountAsync();
            int totalEnrollments = await _context.Enrollments.CountAsync();

            // Fixed Average Calculation
            var avg = await _context.Enrollments
                .Where(e => e.Marks != null)
                .AverageAsync(e => (double?)e.Marks) ?? 0.0;

            var viewModel = new DashboardViewModel
            {
                TotalStudents = totalStudents,
                TotalCourses = totalCourses,
                TotalEnrollments = totalEnrollments,
                AverageMarks = avg,
                // DashboardController.cs mein ye check karein
                RecentStudents = await _context.Students
    .Include(s => s.User)
    .OrderByDescending(s => s.Id)
    .Take(5)
    .ToListAsync(),
                RecentEnrollments = await _context.Enrollments
                    .Include(e => e.Student).ThenInclude(s => s.User)
                    .Include(e => e.Course)
                    .OrderByDescending(e => e.EnrollmentDate)
                    .Take(5)
                    .ToListAsync()
            };

            return View(viewModel);
        }
    }
}