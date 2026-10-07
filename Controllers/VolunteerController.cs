using GiftOfTheGivers.Data;
using GiftOfTheGivers.Models;
using GiftOfTheGivers.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GiftOfTheGivers.Controllers
{
    public class VolunteerController : Controller
    {
        private readonly AppDbContext _db;
        private readonly ILogger<VolunteerController> _logger;

        public VolunteerController(AppDbContext db, ILogger<VolunteerController> logger)
        {
            _db = db;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View(new VolunteerSignupViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(VolunteerSignupViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var volunteer = new Volunteer
            {
                FullName = model.FullName.Trim(),
                Email = model.Email.Trim(),
                Phone = model.Phone.Trim(),
                Skills = string.IsNullOrWhiteSpace(model.Skills) ? null : model.Skills!.Trim(),
                Availability = model.Availability,
                AreaOfInterest = model.AreaOfInterest,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            _db.Volunteers.Add(volunteer);

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                // Never show visitors the raw error page - log it and explain gracefully.
                _logger.LogError(ex, "Failed to save volunteer registration for {Email}.", volunteer.Email);
                ModelState.AddModelError(string.Empty,
                    "We could not save your registration right now. Please try again in a few minutes. " +
                    "If the problem continues, please email your details to volunteers@gotg.org.");
                return View(model);
            }

            TempData["Success"] =
                $"Thank you, {volunteer.FullName}! Your volunteer registration has been received. " +
                "Our team will contact you soon.";

            return RedirectToAction(nameof(Index));
        }
    }
}
