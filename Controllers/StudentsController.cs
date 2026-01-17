using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using SMS.Data;
using SMS.Models.Entities;

namespace SMS.Controllers
{
    [Authorize(Roles = "Admin,Teacher")] // 👈 RBAC: Sirf Authorized staff hi access kar sakta hai
    public class StudentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public StudentsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Students
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            // 1. Agar Admin hai toh saari list dikhao
            if (User.IsInRole("Admin"))
            {
                return View(await _context.Students.Include(s => s.User).ToListAsync());
            }

            // 2. Agar Teacher hai toh sirf apne courses ke enrolled students dikhao
            if (User.IsInRole("Teacher"))
            {
                var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == userId);
                if (teacher == null) return NotFound("Teacher Profile Not Found");

                var myStudents = await _context.Enrollments
                    .Where(e => e.Course.TeacherId == teacher.Id)
                    .Select(e => e.Student)
                    .Distinct()
                    .Include(s => s.User)
                    .ToListAsync();

                return View(myStudents);
            }

            return View(new List<Student>());
        }

        // GET: Students/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var student = await _context.Students
                .Include(s => s.User)
                .Include(s => s.Enrollments).ThenInclude(e => e.Course)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (student == null) return NotFound();

            return View(student);
        }

        // GET: Students/Create
        [Authorize(Roles = "Admin")] // 👈 Security: Registration sirf Admin kar sakta hai
        public IActionResult Create()
        {
            return View();
        }

        // POST: Students/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Student student, string Email, string Password, string FullName)
        {
            if (ModelState.IsValid)
            {
                // 1. Create Identity User
                var user = new ApplicationUser { UserName = Email, Email = Email, FullName = FullName };
                var result = await _userManager.CreateAsync(user, Password);

                if (result.Succeeded)
                {
                    // 2. Assign Student Role
                    await _userManager.AddToRoleAsync(user, "Student");

                    // 3. Link Student record with Identity ID
                    student.UserId = user.Id;
                    _context.Add(student);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(student);
        }

        // GET: Students/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var student = await _context.Students.Include(s => s.User).FirstOrDefaultAsync(s => s.Id == id);
            if (student == null) return NotFound();

            return View(student);
        }

        // POST: Students/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Student student)
        {
            if (id != student.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(student);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StudentExists(student.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(student);
        }

        // POST: Students/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")] // 👈 Security: Sirf Admin delete kar sakta hai
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student != null)
            {
                _context.Students.Remove(student);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool StudentExists(int id)
        {
            return _context.Students.Any(e => e.Id == id);
        }
    }
}