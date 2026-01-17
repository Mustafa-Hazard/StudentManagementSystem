using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering; // 👈 Zaruri: Dropdown ke liye
using SMS.Data;
using SMS.Models.Entities;

namespace SMS.Controllers
{
    [Authorize(Roles = "Admin,Teacher")]
    public class CoursesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CoursesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Courses
        public async Task<IActionResult> Index()
        {
            // 👈 Change: Teacher ka data bhi sath layein taake list mein teacher ka naam nazar aaye
            var courses = await _context.Courses
                .Include(c => c.Teacher)
                .ThenInclude(t => t.User)
                .ToListAsync();
            return View(courses);
        }

        // GET: Courses/Create
        public async Task<IActionResult> Create()
        {
            // 👈 Change: Dropdown bharne ke liye teachers ki list bhejna
            var teachers = await _context.Teachers.Include(t => t.User).ToListAsync();
            ViewBag.TeacherId = new SelectList(teachers, "Id", "User.FullName");

            return View(new Course());
        }

        // POST: Courses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Course course)
        {
            // 👈 Change: Navigation properties ko validation se nikalna taake "Required" error na aaye
            ModelState.Remove("Enrollments");
            ModelState.Remove("Teacher");

            if (ModelState.IsValid)
            {
                _context.Add(course);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Agar validation fail ho jaye toh dropdown dobara bharein
            var teachers = await _context.Teachers.Include(t => t.User).ToListAsync();
            ViewBag.TeacherId = new SelectList(teachers, "Id", "User.FullName", course.TeacherId);
            return View(course);
        }

        // GET: Courses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var course = await _context.Courses.FindAsync(id);
            if (course == null) return NotFound();

            // 👈 Change: Edit page par bhi teacher select karne ka dropdown hona chahiye
            var teachers = await _context.Teachers.Include(t => t.User).ToListAsync();
            ViewBag.TeacherId = new SelectList(teachers, "Id", "User.FullName", course.TeacherId);

            return View(course);
        }

        // POST: Courses/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Course course)
        {
            if (id != course.Id) return NotFound();

            ModelState.Remove("Enrollments");
            ModelState.Remove("Teacher");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(course);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CourseExists(course.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            var teachers = await _context.Teachers.Include(t => t.User).ToListAsync();
            ViewBag.TeacherId = new SelectList(teachers, "Id", "User.FullName", course.TeacherId);
            return View(course);
        }

        // POST: Courses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course != null)
            {
                _context.Courses.Remove(course);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CourseExists(int id)
        {
            return _context.Courses.Any(e => e.Id == id);
        }
    }
}