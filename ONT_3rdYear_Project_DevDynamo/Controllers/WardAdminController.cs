    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.EntityFrameworkCore;
    using Newtonsoft.Json;
    using ONT_3rdyear_Project.Data;
    using ONT_3rdyear_Project.Models;
    using ONT_3rdyear_Project.WardAdminViewModels;
    using System.Security.Claims;

namespace ONT_3rdyear_Project.Controllers
{
    [Authorize(Roles = "WardAdmin")]
    public class WardAdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public WardAdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {


            ViewBag.ActivePatients = _context.Patients
                .Count();

            ViewBag.AvailableBeds = _context.Beds
                .Count(b => !b.IsOccupied);

            var totalBeds = _context.Beds.Count();
            var occupiedBeds = _context.Beds.Count(b => b.IsOccupied);
            ViewBag.BedOccupancy = totalBeds > 0 ? (occupiedBeds * 100 / totalBeds) : 0; ;

            ViewBag.PendingAdmissions = _context.Patients
                .Count(p => !p.Admitted);
            ViewBag.TotalAdmissions = _context.Patients.Count(p => p.Admitted);

            return View();
        }

        // Search for patients
        [HttpPost]
        public async Task<IActionResult> Search(string query)
        {
            var patients = await _context.Patients
                .Include(p => p.Admissions)
                .ThenInclude(a => a.Bed)
                .Where(p => !p.IsDeleted &&
                            (p.FirstName.Contains(query) || p.LastName.Contains(query)))
                .Select(p => new
                {
                    p.PatientID,
                    FullName = p.FirstName + " " + p.LastName,
                    p.Gender,
                    p.DateOfBirth,
                    IsAdmitted = p.Admissions != null && p.Admissions.DischargeDate == null,
                    AdmissionWard = p.Admissions.Ward.Name,
                    AdmissionBed = p.Admissions.Bed.BedNo
                })
                .OrderBy(p => p.FullName)
                .ToListAsync();

            return View("SearchResults", patients);
        }

        // Get All patients
        public async Task<IActionResult> AllPatients()
        {
            var patients = await _context.Patients
                .Include(p => p.Admissions)
                .ThenInclude(a => a.Bed)
                .Include(p => p.DoctorAssignments.Where(da => da.IsActive))
                .ThenInclude(da => da.ApplicationUser)
                .ToListAsync();

            return View(patients);
        }

        // Create patient info
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Patient patient)
        {
            if (ModelState.IsValid)
            {
                _context.Patients.Add(patient);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Patient {patient.FirstName} {patient.LastName} has been successfully registered and added to the system.";
                TempData["SuccessType"] = "success";
                TempData["PatientId"] = patient.PatientID;
                return RedirectToAction(nameof(AllPatients));
            }
            return View(patient);
        }

        // GET: Edit patient info
        //public async Task<IActionResult> EditPatient(int id)
        //{
        //    var patient = await _context.Patients.FindAsync(id);
        //    if (patient == null || patient.IsDeleted)
        //    {
        //        return NotFound();
        //    }
        //    return View(patient);
        //}

        // GET: Edit patient modal
        public async Task<IActionResult> EditPatientModal(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient == null || patient.IsDeleted)
            {
                return NotFound();
            }
            return PartialView("_EditPatientPartial", patient);
        }

        // POST: Edit patient info
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> EditPatient(int id, string firstName, string lastName, string gender, DateTime dateOfBirth, string chronicIllness)
        //{
        //    try
        //    {
        //        var patient = await _context.Patients.FindAsync(id);
        //        if (patient == null || patient.IsDeleted)
        //        {
        //            return Json(new { success = false, message = "Patient not found." });
        //        }


        //        patient.FirstName = firstName;
        //        patient.LastName = lastName;
        //        patient.Gender = gender;
        //        patient.DateOfBirth = dateOfBirth;
        //        patient.ChronicIllness = chronicIllness;

        //        // Don't update Admitted status or other system properties
        //        _context.Update(patient);
        //        await _context.SaveChangesAsync();

        //        return Json(new
        //        {
        //            success = true,
        //            message = $"Patient {firstName} {lastName} has been updated successfully.",
        //            patientId = id
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new
        //        {
        //            success = false,
        //            message = $"Error updating patient: {ex.Message}"
        //        });
        //    }
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPatient(Patient model)
        {
            if (!ModelState.IsValid)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Please correct the validation errors." });
                }
                return View(model);
            }

            try
            {
                // Fetch the existing patient from database
                var existingPatient = await _context.Patients.FindAsync(model.PatientID);
                if (existingPatient == null || existingPatient.IsDeleted)
                {
                    return Json(new { success = false, message = "Patient not found." });
                }

                // Only update editable properties
                existingPatient.FirstName = model.FirstName;
                existingPatient.LastName = model.LastName;
                existingPatient.Gender = model.Gender;
                existingPatient.DateOfBirth = model.DateOfBirth;
                existingPatient.ChronicIllness = model.ChronicIllness;

                _context.Update(existingPatient);
                await _context.SaveChangesAsync();

                //if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                //{
                //    return Json(new
                //    {
                //        success = true,
                //        message = $"Patient {model.FirstName} {model.LastName} has been updated successfully.",
                //        patientId = model.PatientID
                //    });
                //}

                TempData["SuccessMessage"] = $"Patient {model.FirstName} {model.LastName} has been updated successfully.";
                return RedirectToAction(nameof(AllPatients));
            }
            catch (Exception)
            {
                TempData["SuccessMessage"] = $"Fail to update {model.FirstName} {model.LastName}'s information";
                return RedirectToAction(nameof(AllPatients));
            }
        }

        // Soft delete
        public async Task<IActionResult> SoftDelete(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient == null)
            {
                return NotFound();
            }
            return View(patient);
        }

        [HttpPost, ActionName("SoftDelete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SoftDeleteConfirmed(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient == null)
            {
                return NotFound();
            }

            patient.IsDeleted = true;
            _context.Patients.Update(patient);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Patient deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        // View patient details
        public async Task<IActionResult> Details(int id)
        {
            var patient = await _context.Patients
                .Include(p => p.Admissions)
                .ThenInclude(a => a.Bed)
                .ThenInclude(b => b.Ward)
                .Include(p => p.DoctorAssignments.Where(da => da.IsActive))
                .ThenInclude(da => da.ApplicationUser)
                .Include(p => p.MedicalHistories)
                .Include(p => p.PatientAllergies)
                .ThenInclude(pa => pa.Allergy)
                .FirstOrDefaultAsync(p => p.PatientID == id);

            if (patient == null || patient.IsDeleted)
            {
                return NotFound();
            }

            return PartialView("_PatientDetailsPartial", patient);
        }

        // Get admit patient page
        public IActionResult AdmitPatient()
        {
            ViewBag.Patients = _context.Patients
                .Where(p => !p.IsDeleted && (p.Admissions == null || p.Admissions.DischargeDate != null))
                .Select(p => new { p.PatientID, FullName = p.FirstName + " " + p.LastName })
                .ToList();

            ViewBag.Beds = _context.Beds
                .Where(b => !b.IsOccupied)
                .Select(b => new { b.BedId, BedName = b.BedNo + " - Ward " + b.Ward.Name })
                .ToList();
            ViewBag.Wards = _context.Wards
                .Where(w => w.IsActive)
                .Select(w => new { w.WardID, WardName = w.Name })
                .ToList();

            ViewBag.Doctors = _context.Users
                .Where(u => u.RoleType == "Doctor")
                .Select(d => new { d.Id, d.FullName })
                .ToList();

            return View();
        }

        public async Task<IActionResult> ActiveAdmission()
        {
            var admissions = await _context.Admissions
                .Include(a => a.ApplicationUser)
                .Include(a => a.Patient)
                .Include(a => a.Bed)
                .ThenInclude(b => b.Ward)
                .Where(a => a.DischargeDate == null)
                .OrderByDescending(a => a.AdmissionDate)
                .ToListAsync();

            return View(admissions);
        }

        public async Task<IActionResult> PatientAdmission(int patientId)
        {
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.PatientID == patientId);

            if (patient == null || patient.IsDeleted)
            {
                return NotFound();
            }

            var admissions = await _context.Admissions
                .Include(a => a.Bed)
                .ThenInclude(b => b.Ward)
                .Where(a => a.PatientID == patientId)
                .OrderByDescending(a => a.AdmissionDate)
                .FirstOrDefaultAsync();

            ViewBag.PatientName = $"{patient.FirstName} {patient.LastName}";
            return PartialView("_AdmissionDetailsPartial", admissions);
        }

        public async Task<IActionResult> DischargePatientList()
        {
            var admittedPatients = await _context.Admissions
                .Include(a => a.Patient)
                .Include(a => a.Bed)
                .Include(a => a.Ward)
                .Where(a => a.DischargeDate == null)
                .ToListAsync();

            return View(admittedPatients);
        }

        public async Task<IActionResult> DischargePatient(int id)
        {
            var admission = await _context.Admissions
            .Include(a => a.Patient)
            .Include(a => a.Ward)
            .Include(a => a.Bed)
            .FirstOrDefaultAsync(a => a.PatientID == id && a.DischargeDate == null);

            if (admission == null )
            {
                return NotFound();
            }
            return PartialView("_DischargePartial", admission);
        }

        // Discharge patient
        [HttpPost]
        public async Task<IActionResult> Discharge(int patientId, string instructions)
        {
            if (string.IsNullOrWhiteSpace(instructions))
            {
                TempData["Error"] = "Discharge instructions are required";
                return RedirectToAction("DischargePatientList");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var admission = await _context.Admissions
                    .Include(a => a.Bed)
                    .FirstOrDefaultAsync(a => a.PatientID == patientId);

                if (admission == null)
                {
                    TempData["Error"] = "Patient is not currently admitted";
                    return RedirectToAction("DischargePatientList");
                }

                // Get current user ID
                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                // Update admission
                admission.DischargeDate = DateOnly.FromDateTime(DateTime.Now);
                await _context.SaveChangesAsync();

                // Create discharge record
                var dischargeRecord = new Discharge
                {
                    PatientID = patientId,
                    ApplicationUserID = currentUserId,
                    DischargeDate = DateTime.Now,
                    DischargeInstructions = instructions,
                    IsDischarged = true
                };
                await _context.SaveChangesAsync();

                // update patient admission status
                admission.Patient.Admitted = false;
                await _context.SaveChangesAsync();

                // Free bed
                admission.Bed.IsOccupied = false;
                await _context.SaveChangesAsync();

                // Deactivate doctor assignments
                var activeAssignments = await _context.Set<DoctorAssignment>()
                    .Where(da => da.PatientID == patientId && da.IsActive)
                    .ToListAsync();

                foreach (var assignment in activeAssignments)
                {
                    assignment.IsActive = false;
                    assignment.UnassignedDate = DateTime.Now;
                }
                await _context.SaveChangesAsync();

                _context.Add(dischargeRecord);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                TempData["Success"] = "Patient discharged successfully";
                return RedirectToAction("DischargePatientList");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = $"Error discharging patient: {ex.Message}";
                return RedirectToAction("DischargePatientList");
            }
        }

        public async Task<IActionResult> ViewPatients(string filter = "today")
        {
            var admittedPatients = await _context.Admissions
                .Include(a => a.ApplicationUser)
                .Include(a => a.Patient)
                .Include(a => a.Ward)
                .Include(a => a.Bed)
                .Where(a => a.DischargeDate == null) // only active admissions
                .ToListAsync();

            // Total currently admitted patients
            var totalAdmissions = await _context.Admissions
                .Where(a => a.DischargeDate == null)
                .CountAsync();

            // Active wards
            var wardCount = await _context.Wards
                .Where(w => w.IsActive)
                .CountAsync();

            // Available beds
            var availableBeds = await _context.Beds
                .Where(b => !b.IsOccupied)
                .CountAsync();

            // Date filter logic
            DateTime startDate = filter switch
            {
                "7days" => DateTime.Now.AddDays(-7),
                "30days" => DateTime.Now.AddDays(-30),
                _ => DateTime.Today // default: today
            };

            //Movements within selected range
            var recentMovements = await _context.Movements
                .Where(m => m.TimeStamp >= startDate)
                .CountAsync();

            ViewBag.Wards = _context.Wards
                    .Where(w => w.IsActive)
                    .Select(w => new { w.WardID, WardName = w.Name })
                    .ToList();

            ViewBag.TotalAdmissions = totalAdmissions;
            ViewBag.WardCount = wardCount;
            ViewBag.AvailableBeds = availableBeds;
            ViewBag.RecentMovements = recentMovements;



            return View(admittedPatients);
        }

        //show patient movement log form
        public async Task<IActionResult> LogMovement(int patientId)
        {
            var admission = await _context.Admissions
                .Include(a => a.Patient)
                .Include(a => a.Ward)
                .Include(a => a.Bed)
                .FirstOrDefaultAsync(a => a.PatientID == patientId && a.DischargeDate == null);

            if (admission == null)
            {
                return NotFound();
            }

            // Get last movement if exists
            var lastMovement = await _context.Movements
                .Where(m => m.PatientID == patientId)
                .OrderByDescending(m => m.TimeStamp)
                .FirstOrDefaultAsync();

            string currentLocation;
            List<string> destinations = new List<string>();

            if (lastMovement == null || lastMovement.ToLocation.StartsWith("Ward"))
            {
                // Patient is in ward
                currentLocation = $"Ward: {admission.Ward.Name} - Bed: {admission.Bed.BedNo}";
                destinations.Add("X-Ray");
                destinations.Add("Theatre");
            }
            else
            {
                // Patient is outside (X-Ray or Theatre)
                currentLocation = lastMovement.ToLocation;
                destinations.Add($"Ward: {admission.Ward.Name} - Bed: {admission.Bed.BedNo}");
            }

            ViewBag.PatientID = admission.PatientID;
            ViewBag.PatientName = $"{admission.Patient.FirstName} {admission.Patient.LastName}";
            ViewBag.CurrentLocation = currentLocation;
            ViewBag.Destinations = destinations;
            return View();
        }

        // Record patient movement
        [HttpPost]
        public async Task<IActionResult> LogMovement(Movement model)
        {
            if (string.IsNullOrWhiteSpace(model.ToLocation) || string.IsNullOrWhiteSpace(model.FromLocation))
            {
                return Json(new { success = false, message = "From and To locations are required" });
            }

            try
            {
                var admission = await _context.Admissions
                                            .Include(a => a.Ward)
                                            .Include(a => a.Bed)
                                            .FirstOrDefaultAsync(a => a.PatientID == model.PatientID && a.DischargeDate == null);
                if (admission == null)
                {
                    return Json(new { success = false, message = "Patient is not currently admitted" });
                }

                var lastMovement = await _context.Movements
                    .Where(m => m.PatientID == model.PatientID)
                    .OrderByDescending(m => m.TimeStamp)
                    .FirstOrDefaultAsync();

                var fromLocation = lastMovement?.ToLocation
                                ?? $"Ward: {admission.Ward.Name} - Bed: {admission.Bed.BedNo}";
                var movement = new Movement
                {
                    PatientID = model.PatientID,
                    WardID = admission.WardID,
                    BedID = admission.BedID,
                    FromLocation = fromLocation,
                    ToLocation = model.ToLocation,
                    TimeStamp = model.TimeStamp == default ? DateTime.Now : model.TimeStamp,
                };

                _context.Add(movement);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Movement logged successfully";
                return RedirectToAction("MovementHistory", new { patientId = model.PatientID });
            }
            catch (Exception ex)
            {
                TempData["Error Message"] = $"Error logging movement: {ex.Message}";
                return RedirectToAction("LogMovement", new { patientId = model.PatientID });
            }

        }

        // View movement history
        public async Task<IActionResult> MovementHistory(int patientId)
        {
            var patient = await _context.Patients.FindAsync(patientId);
            if (patient == null)
            {
                return NotFound();
            }

            var movements = await _context.Movements
                .Where(m => m.PatientID == patientId)
                .OrderByDescending(m => m.TimeStamp)
                .ToListAsync();

            //TempData["Success"] = $"Movement for {patient.FirstName} {patient.LastName} logged successfully";
            //TempData["Success"] = $" Movement for {patient.FirstName} {patient.LastName} edited  successfully";
            ViewBag.PatientName = $"{patient.FirstName} {patient.LastName}";
            ViewBag.PatientID = patientId;
            return View(movements);
        }

        public async Task<IActionResult> EditMovement(int id)
        {


            var movement = await _context.Movements
                                            .Include(m => m.Patient)
                                            .FirstOrDefaultAsync(m => m.MovementID == id);

            if (movement == null)
            {
                return NotFound();
            }
            var admission = await _context.Admissions
                                        .Include(a => a.Ward)
                                        .Include(a => a.Bed)
                                        .FirstOrDefaultAsync(a => a.PatientID == movement.PatientID && a.DischargeDate == null);

            ViewBag.WardLocation = $"Ward: {admission?.Ward?.Name} - Bed: {admission?.Bed?.BedNo}";
            var destinations = new List<string> { "X-Ray", "Theatre", ViewBag.WardLocation };
            ViewBag.Destinations = destinations;

            return View(movement);
        }

        [HttpPost]
        public async Task<IActionResult> EditMovement(Movement updated, int patientId)
        {
            if (!ModelState.IsValid)
            {
                return View(updated);
            }

            var existingMovement = await _context.Movements
                    .FirstOrDefaultAsync(m => m.MovementID == updated.MovementID);

            if (existingMovement == null)
                return NotFound();

            var patient = await _context.Patients.FindAsync(patientId);
            if (patient == null)
            {
                return NotFound();
            }

            //Update only the fields we allow to change
            existingMovement.ToLocation = updated.ToLocation;
            existingMovement.TimeStamp = DateTime.Now;

            await _context.SaveChangesAsync();

            //TempData["SuccessMessage"] = "Movement updated successfully!";
            TempData["Success"] = $" Movement for {patient.FirstName} {patient.LastName} edited  successfully";
            return RedirectToAction("MovementHistory", new { patientId = updated.PatientID });
        }

        // Get patient medical history
        public async Task<IActionResult> GetMedicalHistory(int patientId)
        {
            var history = await _context.MedicalHistories
                .Where(m => m.PatientId == patientId)
                .OrderByDescending(m => m.RecorderDate)
                .ToListAsync();
            ViewBag.PatientId = patientId;

            return PartialView("_MedicalHistoryPartial", history);
        }

        // Form to add medical history
        //public IActionResult AddMedicalHistory(int patientId)
        //{
        //    ViewBag.PatientId = patientId;
        //    return PartialView("_AddMedicalHistoryPartial");
        //}

        // Save medical history
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMedicalHistory(MedicalHistory model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    model.RecorderDate = DateTime.Now;
                    _context.MedicalHistories.Add(model);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Medical history added successfully" });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = $"Error adding medical history: {ex.Message}" });
                }
            }

            ViewBag.PatientId = model.PatientId;
            return PartialView("_AddMedicalHistoryPartial", model);
        }


        // Get patient allergies
        public async Task<IActionResult> GetPatientAllergies(int patientId)
        {
            var allergies = await _context.PatientAllergies
                    .Include(pa => pa.Allergy)
                    .Where(pa => pa.PatientId == patientId)
                    .ToListAsync();
            ViewBag.PatientId = patientId;
            return PartialView("_AllergyPartial", allergies);
        }

        // Form to add allergy
        //public IActionResult AddAllergy(int patientId)
        //{
        //    ViewBag.PatientId = patientId;
        //    ViewBag.Allergies = _context.Allergies.ToList();
        //    return PartialView("_AddAllergyPartial");
        //}

        // Save allergy after adding
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAllergy(PatientAllergy model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Prevent duplicate allergy entry
                    bool allergyExists = await _context.PatientAllergies
                        .AnyAsync(pa => pa.PatientId == model.PatientId && pa.AllergyId == model.AllergyId);

                    if (!allergyExists)
                    {
                        _context.PatientAllergies.Add(model);
                        await _context.SaveChangesAsync();
                        return Json(new { success = true, message = "Allergy added successfully" });
                    }

                    return Json(new { success = false, message = "This allergy is already recorded for the patient" });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = $"Error adding allergy: {ex.Message}" });
                }
            }

            ViewBag.PatientId = model.PatientId;
            ViewBag.Allergies = _context.Allergies.ToList();
            return PartialView("_AddAllergyPartial", model);
        }

        // Get active admissions
        public async Task<IActionResult> Active()
        {
            var admissions = await _context.Admissions
                .Include(a => a.Patient)
                .Include(a => a.Bed)
                .ThenInclude(b => b.Ward)
                .Where(a => a.DischargeDate == null)
                .ToListAsync();

            return View(admissions);
        }

        //get bed by ward
        public async Task<IActionResult> bedIndex(int? wardId)
        {
            var bedsQuery = _context.Beds
                .Include(b => b.Ward)
                .Include(b=>b.Admissions)
                .ThenInclude(a=>a.Patient)
                .AsQueryable();

            if (wardId.HasValue)
            {
                bedsQuery = bedsQuery.Where(b => b.WardID == wardId);
            }
            // Get all active wards for dropdown
            ViewBag.Wards = await _context.Wards
                .Where(w => w.IsActive)
                .OrderBy(w => w.Name)
                .ToListAsync();

            ViewBag.WardsSelectList = new SelectList(
                await _context.Wards.Where(w => w.IsActive).OrderBy(w => w.Name).ToListAsync(),
                "WardID",
                "Name",
                wardId
            );

            var beds = await bedsQuery.ToListAsync(); 

            ViewBag.WardId = wardId;

            // Summary count
            ViewBag.TotalBeds = beds.Count;
            ViewBag.AvailableBeds = beds.Count(b => !b.IsOccupied);
            ViewBag.OccupiedBeds = beds.Count(b => b.IsOccupied);
            ViewBag.OccupancyRate = beds.Any()
                ? (int)Math.Round((double)ViewBag.OccupiedBeds * 100 / beds.Count)
                : 0;

            return View(beds);
        }

        [HttpGet]
        public async Task<IActionResult> GetCurrentAdmission(int patientId)
        {
            var admission = await _context.Admissions
                .Include(a => a.Ward)
                .Include(a => a.Bed)
                .FirstOrDefaultAsync(a => a.PatientID == patientId && a.DischargeDate == null);

            if (admission == null)
            {
                return Json(new { success = false, message = "No active admission found." });
            }

            var fromLocation = $"{admission.Ward.Name} - Bed {admission.Bed.BedNo}";
            return Json(new { success = true, fromLocation });
        }

        //get bed by ward
        [HttpGet]
        public JsonResult GetBedsByWard(int wardId)
        {
            var beds = _context.Beds
                .Where(b => b.WardID == wardId && !b.IsOccupied)
                .Select(b => new { value = b.BedId, text = b.BedNo })
                .ToList();

            return Json(beds);
        }

        // STEP 1 - GET
        public IActionResult AdmitPatientStep1(int? patientId)
        {
            var model = new AdmissionStep1ViewModel
            {
                Patients = _context.Patients
            .Where(p => !p.IsDeleted && !p.Admitted)
            .Select(p => new SelectListItem
            {
                Value = p.PatientID.ToString(),
                Text = p.FirstName + " " + p.LastName
            }).ToList(),

                Wards = _context.Wards
            .Where(w => w.IsActive)
            .Select(w => new SelectListItem
            {
                Value = w.WardID.ToString(),
                Text = w.Name
            }).ToList(),

                Doctors = _context.Users
            .Where(u => u.RoleType == "Doctor")
            .Select(d => new SelectListItem
            {
                Value = d.Id.ToString(),
                Text = d.FullName
            }).ToList(),

                AdmissionDate = DateOnly.FromDateTime(DateTime.Now)
            };

            if (patientId.HasValue)
                model.PatientID = patientId.Value;

            return View(model);
        }

        // STEP 1 - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AdmitPatientStep1(AdmissionStep1ViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            TempData["Step1"] = JsonConvert.SerializeObject(model);

            return RedirectToAction("AdmitPatientStep2", new { patientId = model.PatientID, admissionId = model.AdmissionID });
        }

        // STEP 2 - GET
        public IActionResult AdmitPatientStep2(int patientId, int admissionId)
        {
            var allergyOptions = _context.Allergies
                .Where(a => !a.IsDeleted)
                .Select(a => new SelectListItem
                {
                    Value = a.AllergyId.ToString(),
                    Text = a.Name
                })
                .ToList();

            allergyOptions.Add(new SelectListItem { Value = "0", Text = "Other (Specify)" });

            var model = new AllergiesStepViewModel
            {
                PatientID = patientId,
                AdmissionID = admissionId,
                AllergyOptions = allergyOptions,
                Allergies = new List<AllergyEntry>()
            };

            return View(model);
        }

        // STEP 2 - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AdmitPatientStep2(AllergiesStepViewModel model)
        {
            if (model.Allergies == null || model.Allergies.Count == 0)
            {
                ModelState.AddModelError("", "Please add at least one allergy.");
                return View(model);
            }

            TempData["Step2"] = JsonConvert.SerializeObject(model);

            return RedirectToAction("AdmitPatientStep3", new { patientId = model.PatientID, admissionId = model.AdmissionID });
        }

        // STEP 3 - GET
        public IActionResult AdmitPatientStep3(int patientId, int admissionId)
        {
            var model = new MedicalHistoryStepViewModel
            {
                PatientID = patientId,
                AdmissionID = admissionId,  // ✅ carry admissionId
                MedicalHistories = new List<MedicalHistoryItemViewModel>()
            };

            return View(model);
        }

        // STEP 3 - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AdmitPatientStep3(MedicalHistoryStepViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            TempData["Step3"] = JsonConvert.SerializeObject(model);

            // ✅ carry admissionId forward
            return RedirectToAction("AdmissionSummary", new { patientId = model.PatientID, admissionId = model.AdmissionID });
        }

        // STEP 4 - GET SUMMARY
        public IActionResult AdmissionSummary(int patientId, int admissionId)
        {
            var step1 = JsonConvert.DeserializeObject<AdmissionStep1ViewModel>(TempData.Peek("Step1").ToString());
            var step2 = JsonConvert.DeserializeObject<AllergiesStepViewModel>(TempData.Peek("Step2").ToString());
            var step3 = JsonConvert.DeserializeObject<MedicalHistoryStepViewModel>(TempData.Peek("Step3").ToString());

            // Fetch actual names from DB
            var patient = _context.Patients
                .Where(p => p.PatientID == step1.PatientID)
                .Select(p => p.FirstName + " " + p.LastName)
                .FirstOrDefault();

            var ward = _context.Wards
                .Where(w => w.WardID == step1.WardID)
                .Select(w => w.Name)
                .FirstOrDefault();

            var bedNo = _context.Beds
                .Where(b => b.BedId == step1.BedID)
                .Select(b => b.BedNo)
                .FirstOrDefault();

            var doctor = _context.Users
                .Where(d => d.Id == step1.DoctorID)
                .Select(d => d.FullName)
                .FirstOrDefault();
            var allergyIds = step2.Allergies
                .Where(a => a.SelectedAllergyId > 0)
                .Select(a => a.SelectedAllergyId)
                .ToList();

            var allergyMap = _context.Allergies
                .Where(a => allergyIds.Contains(a.AllergyId))
                .ToDictionary(a => a.AllergyId, a => a.Name);

            foreach (var allergy in step2.Allergies)
            {
                if (allergy.SelectedAllergyId > 0 && allergyMap.ContainsKey(allergy.SelectedAllergyId))
                {
                    allergy.ResolvedName = allergyMap[allergy.SelectedAllergyId];
                }
                else if (allergy.SelectedAllergyId == 0)
                {
                    allergy.ResolvedName = allergy.OtherAllergyName;
                }
            }

            var model = new AdmissionSummaryViewModel
            {
                Step1 = step1,
                Step2 = step2,
                Step3 = step3,

                PatientName = patient,
                WardName = ward,
                BedNo = bedNo,
                DoctorName = doctor
            };

            return View(model);
        }

        // STEP 4 - POST FINAL SUBMIT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmAdmission()
        {
            var step1 = JsonConvert.DeserializeObject<AdmissionStep1ViewModel>(TempData["Step1"].ToString());
            var step2 = JsonConvert.DeserializeObject<AllergiesStepViewModel>(TempData["Step2"].ToString());
            var step3 = JsonConvert.DeserializeObject<MedicalHistoryStepViewModel>(TempData["Step3"].ToString());

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                //Create new Admission
                var admission = new Admission
                {
                    PatientID = step1.PatientID,
                    BedID = step1.BedID,
                    WardID = step1.WardID,
                    DoctorId = step1.DoctorID,
                    AdmissionDate = DateOnly.FromDateTime(DateTime.Now),
                    ReasonForAdmission = step1.ReasonForAdmission,
                    Notes = step1.Notes
                };
                _context.Admissions.Add(admission);
                await _context.SaveChangesAsync();

                //Update patient admission status
                var patient = await _context.Patients.FindAsync(step1.PatientID);
                patient.Admitted = true;
                await _context.SaveChangesAsync();

                //Doctor Assignment
                var doctorAssignment = new DoctorAssignment
                {
                    DoctorID = step1.DoctorID,
                    PatientID = step1.PatientID,
                    AssignmentDate = DateTime.Now,
                    IsActive = true
                };
                _context.DoctorAssignments.Add(doctorAssignment);
                await _context.SaveChangesAsync();

                //Allergies
                foreach (var entry in step2.Allergies)
                {
                    int allergyId = entry.SelectedAllergyId;

                    if (allergyId == 0 && !string.IsNullOrWhiteSpace(entry.OtherAllergyName))
                    {
                        var existing = await _context.Allergies
                            .FirstOrDefaultAsync(a => a.Name.ToLower() == entry.OtherAllergyName.ToLower());

                        if (existing != null)
                        {
                            allergyId = existing.AllergyId;
                        }
                        else
                        {
                            var newAllergy = new Allergy
                            {
                                Name = entry.OtherAllergyName,
                                Description = entry.OtherAllergyDescription
                            };
                            _context.Allergies.Add(newAllergy);
                            await _context.SaveChangesAsync();
                            allergyId = newAllergy.AllergyId;
                        }
                    }

                    var patientAllergy = new PatientAllergy
                    {
                        PatientId = step1.PatientID,
                        AdmissionId = admission.AdmissionID,
                        AllergyId = allergyId,
                        Notes = entry.Notes,
                        Severity = entry.Severity
                    };
                    _context.PatientAllergies.Add(patientAllergy);
                }
                await _context.SaveChangesAsync();

                // 5. Medical History
                foreach (var mh in step3.MedicalHistories)
                {
                    var history = new MedicalHistory
                    {
                        PatientId = step1.PatientID,
                        AdmissionId = admission.AdmissionID,
                        ChronicCondition = mh.ChronicCondition,
                        MedicationHistory = mh.MedicationHistory,
                        PastSurgicalHistory = mh.PastSurgicalHistory,
                        RecorderDate = DateTime.Now,
                        ConditonSeverity = mh.ConditionSeverity
                    };
                    _context.MedicalHistories.Add(history);
                }
                await _context.SaveChangesAsync();

                //Update bed occupancy
                var bed = await _context.Beds.FindAsync(step1.BedID);
                bed.IsOccupied = true;
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                //Success message
                //admission.Patient = patient;
                TempData["Success"] = $"{patient.FirstName} {patient.LastName} has been admitted successfully!";

                return RedirectToAction("ActiveAdmission");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = $"Error during admission: {ex.Message}";
                Console.WriteLine($"Error occured: {ex}");
                return RedirectToAction("AdmitPatientStep1");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetQuickInfo(int patientId)
        {
            var patient = await _context.Patients
                .Include(p => p.Admissions)
                .ThenInclude(a => a.Bed)
                .ThenInclude(b => b.Ward)
                .Include(p => p.DoctorAssignments.Where(da => da.IsActive))
                .ThenInclude(da => da.ApplicationUser)
                .Include(p => p.Movements.OrderByDescending(m => m.TimeStamp).Take(5))
                .FirstOrDefaultAsync(p => p.PatientID == patientId);

            if (patient == null)
                return NotFound();

            var admission = patient.Admissions;

            var result = new
            {
                patient.PatientID,
                FullName = $"{patient.FirstName} {patient.LastName}",
                patient.DateOfBirth,
                AdmissionDate = admission?.AdmissionDate.ToString("yyyy-MM-dd"),
                Ward = admission?.Ward?.Name,
                Bed = admission?.Bed?.BedNo,
                Doctor = patient.DoctorAssignments.FirstOrDefault()?.ApplicationUser?.FullName,
                Movements = patient.Movements.Select(m => new
                {
                    m.FromLocation,
                    m.ToLocation,
                    Time = m.TimeStamp.ToString("dd/MM/yyyy HH:mm")
                })
            };

            return Json(result);
        }

        public async Task<IActionResult> GetBedDetails(int id)
        {
            var bed = await _context.Beds
                .Include(b => b.Ward)
                .Include(b => b.Admissions)
                    .ThenInclude(a => a.Patient)
                .FirstOrDefaultAsync(b => b.BedId == id);

            if (bed == null)
            {
                return NotFound();
            }

            var activeAdmission = bed.Admissions;

            return Json(new
            {
                bedId = bed.BedId,
                bedNo = bed.BedNo,
                ward = bed.Ward?.Name,
                isOccupied = bed.IsOccupied,
                patient = activeAdmission?.Patient != null
                    ? $"{activeAdmission.Patient.FirstName} {activeAdmission.Patient.LastName}"
                    : null,
                lastOccupied = activeAdmission?.AdmissionDate.ToString("MMM dd, yyyy"),
            });
        }
    }
}
