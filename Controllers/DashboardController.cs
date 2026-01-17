using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SMS.Data;
using SMS.Models.Entities;
using SMS.Models.ViewModels;

namespace SMS.Controllers
{
    [Authorize] // 👈 Change: Ab sab logged-in users enter ho sakte hain
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // 👈 MAIN CHANGE: Ye action ab "Traffic Police" ka kaam karega
        public async Task<IActionResult> Index()
        {
            var viewModel = new DashboardViewModel
            {
                TotalStudents = await _context.Students.CountAsync(),
                TotalCourses = await _context.Courses.CountAsync(),
                TotalEnrollments = await _context.Enrollments.CountAsync(),

                // 👈 YE LINE CHECK KAREIN: Kya ye mojood hai?
                RecentStudents = await _context.Students.Include(s => s.User).Take(5).ToListAsync(),

                // 👈 YE LINE ADD KAREIN: Taake enrollments null na hon
                RecentEnrollments = await _context.Enrollments
                    .Include(e => e.Student).ThenInclude(s => s.User)
                    .Include(e => e.Course)
                    .OrderByDescending(e => e.Id)
                    .Take(5)
                    .ToListAsync()
            };

            return View(viewModel);
        }

        [Authorize(Roles = "Student")] // 👈 Change: Sirf Students ke liye locked
        public async Task<IActionResult> StudentIndex()
        {
            var userId = _userManager.GetUserId(User);
            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (student == null) return NotFound("Student Profile Not Found");

            var enrollments = await _context.Enrollments
                .Include(e => e.Course)
                .Where(e => e.StudentId == student.Id)
                .ToListAsync();

            var viewModel = new StudentDashboardViewModel
            {
                StudentName = student.User?.FullName ?? "Mustafa Muhammad Iqbal",
                RegId = student.StudentRegId,
                MyCourses = enrollments
            };

            return View(viewModel); // Views/Dashboard/StudentIndex.cshtml dikhao
        }
    }
}