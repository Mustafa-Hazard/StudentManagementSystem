using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using SMS.Data;
using SMS.Models.Entities;

namespace SMS.Controllers
{
    [Authorize(Roles = "Admin,Teacher")] //
    public class CoursesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CoursesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // 🟢 INDEX: Role-based filtering
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            if (User.IsInRole("Admin"))
            {
                // Admin saare courses dekh sakta hai
                return View(await _context.Courses.Include(c => c.Teacher).ThenInclude(t => t.User).ToListAsync());
            }

            // Teacher sirf apne assigned courses dekhega
            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == userId);
            if (teacher == null) return NotFound();

            var myCourses = await _context.Courses
                .Include(c => c.Teacher).ThenInclude(t => t.User)
                .Where(c => c.TeacherId == teacher.Id)
                .ToListAsync();

            return View(myCourses);
        }

        // 🔵 DETAILS:
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var course = await _context.Courses
                .Include(c => c.Teacher).ThenInclude(t => t.User)
                .Include(c => c.Enrollments).ThenInclude(e => e.Student).ThenInclude(s => s.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (course == null) return NotFound();

            return View(course);
        }

        // 🟡 CREATE (GET): Sirf Admin ke liye
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            var teachers = await _context.Teachers.Include(t => t.User).ToListAsync();
            ViewBag.TeacherId = new SelectList(teachers, "Id", "User.FullName");
            return View(new Course());
        }

        // 🟡 CREATE (POST): Server-side Validation
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Course course)
        {
            // Navigation properties ko validation se hatana
            ModelState.Remove("Teacher");
            ModelState.Remove("Enrollments");

            if (ModelState.IsValid)
            {
                _context.Add(course);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            var teachers = await _context.Teachers.Include(t => t.User).ToListAsync();
            ViewBag.TeacherId = new SelectList(teachers, "Id", "User.FullName", course.TeacherId);
            return View(course);
        }

        // 🟠 EDIT (GET): Admin ya wahi Teacher jis ka course hai
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var course = await _context.Courses.FindAsync(id);
            if (course == null) return NotFound();

            var teachers = await _context.Teachers.Include(t => t.User).ToListAsync();
            ViewBag.TeacherId = new SelectList(teachers, "Id", "User.FullName", course.TeacherId);
            return View(course);
        }

        // 🟠 EDIT (POST):
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Course course)
        {
            if (id != course.Id) return NotFound();

            ModelState.Remove("Teacher");
            ModelState.Remove("Enrollments");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(course);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Courses.Any(e => e.Id == course.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(course);
        }
    }
}