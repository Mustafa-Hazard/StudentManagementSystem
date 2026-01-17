using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;
using SMS.Data;
using SMS.Models.Entities;
using SMS.Models.ViewModels;

namespace SMS.Controllers
{
    // 👈 Controller level par sab ko allow karein, actions par restriction lagayenge
    [Authorize(Roles = "Admin,Teacher,Student")]
    public class StudentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public StudentsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // 🟢 INDEX: List of Students (Staff Only)
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            if (User.IsInRole("Admin"))
            {
                return View(await _context.Students.Include(s => s.User).ToListAsync());
            }

            // Teacher filtering logic
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

        // 🔵 DETAILS: Profile View
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

        // 🟡 CREATE: Add New Student (Admin Only)
        [Authorize(Roles = "Admin")]
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Student student, string Email, string Password, string FullName)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = Email, Email = Email, FullName = FullName };
                var result = await _userManager.CreateAsync(user, Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Student");
                    student.UserId = user.Id;
                    _context.Add(student);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(student);
        }

        // 🟠 EDIT: Profile Update
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var student = await _context.Students.Include(s => s.User).FirstOrDefaultAsync(s => s.Id == id);
            return student == null ? NotFound() : View(student);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> Edit(int id, Student student)
        {
            if (id != student.Id) return NotFound();
            if (ModelState.IsValid)
            {
                _context.Update(student);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(student);
        }

        // 🔴 DELETE: Remove Student (Admin Only)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student != null) _context.Students.Remove(student);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // 📄 PDF TRANSCRIPT: Download logic (Fixed 404 & Access Denied)
        public async Task<IActionResult> DownloadTranscript(int? id)
        {
            var userId = _userManager.GetUserId(User);
            int targetId;

            // Auto-detect ID if user is a Student
            if (id == null && User.IsInRole("Student"))
            {
                var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
                if (student == null) return NotFound();
                targetId = student.Id;
            }
            else if (id != null)
            {
                targetId = id.Value;
            }
            else
            {
                return BadRequest("Student ID required.");
            }

            var studentData = await _context.Students
                .Include(s => s.User)
                .Include(s => s.Enrollments).ThenInclude(e => e.Course)
                .FirstOrDefaultAsync(s => s.Id == targetId);

            if (studentData == null) return NotFound();

            double totalMarks = studentData.Enrollments.Where(e => e.Marks != null).Sum(e => (double)e.Marks);
            double avgMarks = studentData.Enrollments.Any() ? totalMarks / studentData.Enrollments.Count : 0;

            var viewModel = new TranscriptViewModel
            {
                Student = studentData,
                Enrollments = studentData.Enrollments.ToList(),
                TotalGPA = avgMarks
            };

            return new ViewAsPdf("TranscriptPDF", viewModel)
            {
                FileName = $"Transcript_{studentData.StudentRegId}.pdf",
                PageSize = Rotativa.AspNetCore.Options.Size.A4
            };
        }
    }
}