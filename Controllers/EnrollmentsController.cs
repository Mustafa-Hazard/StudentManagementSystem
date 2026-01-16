using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SMS.Data;
using SMS.Models.Entities;
using SMS.Services;

namespace SMS.Controllers
{
    public class EnrollmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public EnrollmentsController(ApplicationDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // 1. INDEX: Enrollments ki list dikhana
        public async Task<IActionResult> Index()
        {
            var enrollments = await _context.Enrollments
                .Include(e => e.Course)
                .Include(e => e.Student).ThenInclude(s => s.User)
                .ToListAsync();
            return View(enrollments);
        }

        // 2. CREATE (GET)
        public IActionResult Create()
        {
            ViewBag.StudentId = new SelectList(_context.Students.Include(s => s.User), "Id", "User.FullName");
            ViewBag.CourseId = new SelectList(_context.Courses, "Id", "Title");
            return View();
        }

        // 3. CREATE (POST): Enrollment aur Email Notification
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Enrollment enrollment)
        {
            ModelState.Remove("Student");
            ModelState.Remove("Course");

            if (ModelState.IsValid)
            {
                _context.Add(enrollment);
                await _context.SaveChangesAsync();

                // Email bhejne ka logic
                try
                {
                    var student = await _context.Students.Include(s => s.User).FirstOrDefaultAsync(s => s.Id == enrollment.StudentId);
                    var course = await _context.Courses.FindAsync(enrollment.CourseId);

                    if (student?.User != null && course != null)
                    {
                        string subject = "Course Enrollment Confirmation";
                        string body = $"<h4>Hello {student.User.FullName},</h4>" +
                                      $"<p>You have been successfully enrolled in <b>{course.Title}</b>.</p>" +
                                      $"<p>Enrollment Date: {enrollment.EnrollmentDate.ToShortDateString()}</p>";

                        await _emailService.SendEmailAsync(student.User.Email, subject, body);
                    }
                }
                catch { /* Email na bhi jaye toh record save ho chuka hai */ }

                return RedirectToAction(nameof(Index));
            }

            ViewBag.StudentId = new SelectList(_context.Students.Include(s => s.User), "Id", "User.FullName", enrollment.StudentId);
            ViewBag.CourseId = new SelectList(_context.Courses, "Id", "Title", enrollment.CourseId);
            return View(enrollment);
        }

        // 4. EDIT: Marks aur Grade update karne ke liye
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var enrollment = await _context.Enrollments.Include(e => e.Student).ThenInclude(s => s.User).Include(e => e.Course).FirstOrDefaultAsync(m => m.Id == id);
            return View(enrollment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Enrollment enrollment)
        {
            ModelState.Remove("Student");
            ModelState.Remove("Course");

            if (ModelState.IsValid)
            {
                // Grade calculation logic
                if (enrollment.Marks >= 85) enrollment.Grade = "A";
                else if (enrollment.Marks >= 70) enrollment.Grade = "B";
                else if (enrollment.Marks >= 50) enrollment.Grade = "C";
                else enrollment.Grade = "F";

                _context.Update(enrollment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(enrollment);
        }

        // 5. DELETE
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var enrollment = await _context.Enrollments.Include(e => e.Student).ThenInclude(s => s.User).Include(e => e.Course).FirstOrDefaultAsync(m => m.Id == id);
            return View(enrollment);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var enrollment = await _context.Enrollments.FindAsync(id);
            if (enrollment != null) _context.Enrollments.Remove(enrollment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}