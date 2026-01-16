using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SMS.Data;
using SMS.Models.Entities;
using SMS.Services; // Email service ke liye

namespace SMS.Controllers
{
    public class TeachersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;

        public TeachersController(ApplicationDbContext context,
                                  UserManager<ApplicationUser> userManager,
                                  IEmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
        }

        // 1. INDEX: Saare teachers ki list dikhana
        public async Task<IActionResult> Index()
        {
            var teachers = await _context.Teachers.Include(t => t.User).ToListAsync();
            return View(teachers);
        }

        // 2. DETAILS: Teacher ki profile dekhna
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var teacher = await _context.Teachers
                .Include(t => t.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (teacher == null) return NotFound();

            return View(teacher);
        }

        // 3. CREATE (GET)
        public IActionResult Create() => View();

        // 4. CREATE (POST): Account banana aur Teacher ko welcome email bhejna
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Teacher teacher, string Email, string Password, string FullName)
        {
            // Validation errors se bachne ke liye navigation properties hatana
            ModelState.Remove("User");
            ModelState.Remove("UserId");

            if (ModelState.IsValid)
            {
                // Identity User banana
                var user = new ApplicationUser
                {
                    UserName = Email,
                    Email = Email,
                    FullName = FullName // SQL NULL error fix
                };

                var result = await _userManager.CreateAsync(user, Password);

                if (result.Succeeded)
                {
                    // Teacher role assign karna
                    await _userManager.AddToRoleAsync(user, "Teacher");

                    // Teacher record ko User account se link karna
                    teacher.UserId = user.Id;
                    _context.Add(teacher);
                    await _context.SaveChangesAsync();

                    // Welcome Email logic
                    try
                    {
                        string subject = "Welcome to NMEIS Faculty - " + FullName;
                        string body = $"<h3>Hello {FullName},</h3>" +
                                      $"<p>Your faculty account has been created successfully in the <b>{teacher.Department}</b> department.</p>" +
                                      $"<p><b>Login Email:</b> {Email}</p>";

                        await _emailService.SendEmailAsync(Email, subject, body);
                    }
                    catch { /* Email fail ho toh project na ruke */ }

                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(teacher);
        }

        // 5. EDIT (GET)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var teacher = await _context.Teachers.Include(t => t.User).FirstOrDefaultAsync(t => t.Id == id);
            if (teacher == null) return NotFound();

            return View(teacher);
        }

        // 6. EDIT (POST): Teacher ka data aur User ka FullName update karna
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Teacher teacher, string FullName)
        {
            if (id != teacher.Id) return NotFound();

            ModelState.Remove("User");
            ModelState.Remove("UserId");

            if (ModelState.IsValid)
            {
                try
                {
                    var existingTeacher = await _context.Teachers.Include(t => t.User).FirstOrDefaultAsync(t => t.Id == id);

                    if (existingTeacher != null && existingTeacher.User != null)
                    {
                        existingTeacher.User.FullName = FullName; // Name update
                        existingTeacher.Department = teacher.Department; // Dept update

                        _context.Update(existingTeacher);
                        await _context.SaveChangesAsync();
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TeacherExists(teacher.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(teacher);
        }

        // 7. DELETE (GET)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var teacher = await _context.Teachers
                .Include(t => t.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (teacher == null) return NotFound();

            return View(teacher);
        }

        // 8. DELETE (POST): Teacher aur uska Identity Account dono delete karna
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var teacher = await _context.Teachers.Include(t => t.User).FirstOrDefaultAsync(t => t.Id == id);

            if (teacher != null)
            {
                // Cascade Delete logic: Pehle User account khatam karein
                if (teacher.User != null)
                {
                    await _userManager.DeleteAsync(teacher.User);
                }
                _context.Teachers.Remove(teacher);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool TeacherExists(int id) => _context.Teachers.Any(e => e.Id == id);
    }
}