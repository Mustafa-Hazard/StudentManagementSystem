using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SMS.Data;
using SMS.Models.Entities;

namespace SMS.Controllers
{
    public class AttendancesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AttendancesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Attendances
        public async Task<IActionResult> Index()
        {
            // Student aur Course ki details ke sath list dikhana
            var attendances = await _context.Attendances
                .Include(a => a.Student)
                .ThenInclude(s => s.User)
                .Include(a => a.Course)
                .ToListAsync();
            return View(attendances);
        }

        // GET: Attendances/Create
        public IActionResult Create()
        {
            // Dropdowns mein Student ka FullName aur Course ka Title load karna
            ViewBag.StudentId = new SelectList(_context.Students.Include(s => s.User), "Id", "User.FullName");
            ViewBag.CourseId = new SelectList(_context.Courses, "Id", "Title");
            return View();
        }

        // POST: Attendances/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Attendance attendance)
        {
            // CHANGES HERE: Navigation properties ko remove karna taake ModelState.IsValid TRUE ho
            ModelState.Remove("Student");
            ModelState.Remove("Course");

            if (ModelState.IsValid)
            {
                _context.Add(attendance);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Agar validation fail ho jaye toh dropdowns dobara bharna
            ViewBag.StudentId = new SelectList(_context.Students.Include(s => s.User), "Id", "User.FullName", attendance.StudentId);
            ViewBag.CourseId = new SelectList(_context.Courses, "Id", "Title", attendance.CourseId);
            return View(attendance);
        }

        // GET: Attendances/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var attendance = await _context.Attendances.FindAsync(id);
            if (attendance == null) return NotFound();

            ViewBag.StudentId = new SelectList(_context.Students.Include(s => s.User), "Id", "User.FullName", attendance.StudentId);
            ViewBag.CourseId = new SelectList(_context.Courses, "Id", "Title", attendance.CourseId);
            return View(attendance);
        }

        // POST: Attendances/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Attendance attendance)
        {
            if (id != attendance.Id) return NotFound();

            ModelState.Remove("Student");
            ModelState.Remove("Course");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(attendance);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AttendanceExists(attendance.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.StudentId = new SelectList(_context.Students.Include(s => s.User), "Id", "User.FullName", attendance.StudentId);
            ViewBag.CourseId = new SelectList(_context.Courses, "Id", "Title", attendance.CourseId);
            return View(attendance);
        }

        // GET: Attendances/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var attendance = await _context.Attendances
                .Include(a => a.Student)
                .ThenInclude(s => s.User)
                .Include(a => a.Course)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (attendance == null) return NotFound();

            return View(attendance);
        }

        // POST: Attendances/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var attendance = await _context.Attendances.FindAsync(id);
            if (attendance != null)
            {
                _context.Attendances.Remove(attendance);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AttendanceExists(int id)
        {
            return _context.Attendances.Any(e => e.Id == id);
        }
    }
}