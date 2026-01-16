using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SMS.Data;
using SMS.Models.Entities;
using SMS.Services; // Email service ke liye

namespace SMS.Controllers
{
    public class StudentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;

        public StudentsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IEmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService; // Service inject ho rahi hai
        }

        // 1. INDEX: Saare students ki list dikhana
        public async Task<IActionResult> Index()
        {
            // .Include(s => s.User) taake GUID ki jagah FullName dikhe
            var students = await _context.Students.Include(s => s.User).ToListAsync();
            return View(students);
        }

        // 2. DETAILS: Student ki mukammal profile
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (student == null) return NotFound();

            return View(student);
        }

        // 3. CREATE (GET)
        public IActionResult Create() => View();

        // 4. CREATE (POST): Account banana aur Email bhejna
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Student student, string Email, string Password, string FullName)
        {
            // Navigation properties ko validation se hatana lazmi hai
            ModelState.Remove("User");
            ModelState.Remove("UserId");
            ModelState.Remove("Enrollments");

            if (ModelState.IsValid)
            {
                // Step A: Identity User banana
                var user = new ApplicationUser
                {
                    UserName = Email,
                    Email = Email,
                    FullName = FullName // SQL NULL error fix
                };

                var result = await _userManager.CreateAsync(user, Password);

                if (result.Succeeded)
                {
                    // Step B: Role assign karna
                    await _userManager.AddToRoleAsync(user, "Student");

                    // Step C: Student record link karna
                    student.UserId = user.Id;
                    _context.Add(student);
                    await _context.SaveChangesAsync();

                    // Step D: Welcome Email bhejna
                    try
                    {
                        string subject = "NMEIS Portal - Registration Successful";
                        string body = $"<h3>Welcome {FullName}!</h3>" +
                                      $"<p>Your account has been created successfully.</p>" +
                                      $"<p><b>Registration ID:</b> {student.StudentRegId}</p>" +
                                      $"<p><b>Login Email:</b> {Email}</p>";

                        await _emailService.SendEmailAsync(Email, subject, body);
                    }
                    catch { /* Email fail hone par project crash na ho */ }

                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(student);
        }

        // 5. EDIT (GET)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var student = await _context.Students.Include(s => s.User).FirstOrDefaultAsync(s => s.Id == id);
            if (student == null) return NotFound();

            return View(student);
        }

        // 6. EDIT (POST): Update details
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Student student, string FullName)
        {
            if (id != student.Id) return NotFound();

            ModelState.Remove("User");
            ModelState.Remove("UserId");
            ModelState.Remove("Enrollments");

            if (ModelState.IsValid)
            {
                try
                {
                    var existingStudent = await _context.Students.Include(s => s.User).FirstOrDefaultAsync(s => s.Id == id);

                    if (existingStudent != null && existingStudent.User != null)
                    {
                        // Dono tables (Student & AspNetUsers) update ho rahi hain
                        existingStudent.User.FullName = FullName;
                        existingStudent.StudentRegId = student.StudentRegId;
                        existingStudent.Department = student.Department;

                        _context.Update(existingStudent);
                        await _context.SaveChangesAsync();
                    }
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

        // 7. DELETE (GET)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (student == null) return NotFound();

            return View(student);
        }

        // 8. DELETE (POST): Account aur Record dono khatam karna
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var student = await _context.Students.Include(s => s.User).FirstOrDefaultAsync(s => s.Id == id);

            if (student != null)
            {
                // Cascade Delete logic: Pehle User account delete karein
                if (student.User != null)
                {
                    await _userManager.DeleteAsync(student.User);
                }
                _context.Students.Remove(student);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool StudentExists(int id) => _context.Students.Any(e => e.Id == id);
    }
}