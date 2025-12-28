using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ONT_3rdyear_Project.Data;
using ONT_3rdyear_Project.Models;
using ONT_3rdyear_Project.ViewModels;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ONT_3rdyear_Project.Controllers
{
    [Authorize(Roles ="Sister")]
    public class NurseSisterController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public NurseSisterController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        
        public async Task<IActionResult> Dashboard()
        {
            var today = DateTime.Today;

            var totalPatients = await _context.Patients.CountAsync();

            var treatmentsToday = await _context.Treatments
                .Where(t => t.IsActive)
                .CountAsync();

            var medicationsGivenToday = await _context.PatientMedicationScripts
                .Where(m => m.isActive)
                .CountAsync();
            var totalVitals = await _context.Vitals.Where(v=>v.IsActive).CountAsync();

            var model = new DashboardViewModel
            {
                Stats = new DashboardStatsViewModel
                {
                    TotalPatients = totalPatients,
                    TreatmentsToday = treatmentsToday,
                    MedicationsGivenToday = medicationsGivenToday,
                    //HoursOnDuty = 24 
                    TotalVitals = totalVitals
                },
                Patients = await (
                    from p in _context.Patients
                    join a in _context.Admissions on p.PatientID equals a.PatientID into admissionGroup
                    from a in admissionGroup.DefaultIfEmpty()
                    join w in _context.Wards on a.WardID equals w.WardID into wardGroup
                    from w in wardGroup.DefaultIfEmpty()
                    join b in _context.Beds on a.BedID equals b.BedId into bedGroup
                    from b in bedGroup.DefaultIfEmpty()
                    select new PatientDashboardViewModel
                    {
                        PatientID = p.PatientID,
                        FirstName = p.FirstName,
                        LastName = p.LastName,
                        WardName = w != null ? w.Name : "N/A",
                        BedNo = b != null ? b.BedNo : "N/A"
                    }
                ).ToListAsync()
            };

            return View(model);
        }

        public async Task<IActionResult> PatientsList(string sortOrder)
        {

            

            var beds = await _context.Beds.Where(b => !b.IsDeleted).ToListAsync();
            ViewBag.Beds = new SelectList(beds, "BedId", "BedNo");

            var wards = await _context.Wards.ToListAsync();
            ViewBag.Wards = new SelectList(wards, "WardID", "Name");

            // Set the sorting parameters for the view
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";

            var data = await (
                from p in _context.Patients
                join a in _context.Admissions on p.PatientID equals a.PatientID into admissionGroup
                from a in admissionGroup.DefaultIfEmpty()
                join w in _context.Wards on a.WardID equals w.WardID into wardGroup
                from w in wardGroup.DefaultIfEmpty()
                join b in _context.Beds on a.BedID equals b.BedId into bedGroup
                from b in bedGroup.DefaultIfEmpty()
                join d in _context.Discharges on p.PatientID equals d.PatientID into dischargeGroup
                from d in dischargeGroup.OrderByDescending(x => x.DischargeDate).Take(1).DefaultIfEmpty()
                select new PatientDashboardViewModel
                {
                    PatientID = p.PatientID,
                    FirstName = p.FirstName,
                    LastName = p.LastName,
                    WardName = w != null ? w.Name : "N/A",
                    BedNo = b != null ? b.BedNo : "N/A",
                    Status = d != null && d.IsDischarged == true ? "Discharged" :
                     a != null ? "Admitted" : "Not Admitted",
                    AdmissionDate = a.AdmissionDate,
                    UnreadAdviceCount = _context.Instructions
                .Count(i => i.PatientID == p.PatientID && !i.IsRead && i.isActive)
                }
            ).ToListAsync();

            // Apply sorting based on the sortOrder parameter
            switch (sortOrder)
            {
                case "name_desc":
                    data = data.OrderByDescending(p => p.FirstName).ToList();
                    break;
                case "Date":
                    data = data.OrderBy(p => p.AdmissionDate).ToList();
                    break;
                case "date_desc":
                    data = data.OrderByDescending(p => p.AdmissionDate).ToList();
                    break;
                default:
                    data = data.OrderBy(p => p.FirstName).ToList(); // Default sort
                    break;
            }

            return View(data);
        }


        [HttpPost]
        public async Task<IActionResult> MarkAdviceAsRead(int patientId)
        {
            var unreadInstructions = await _context.Instructions
                .Where(i => i.PatientID == patientId && !i.IsRead && i.isActive)
                .ToListAsync();

            foreach (var instruction in unreadInstructions)
            {
                instruction.IsRead = true;
                instruction.ReadAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }






        //CRUD for administering

        public async Task<IActionResult> ListAdministered(string searchPatient, DateTime? fromDate, DateTime? toDate, string sortOrder)
        {
            var scheduledScripts = await _context.PatientMedicationScripts
                .Include(p => p.Prescription)
                .Include(pm => pm.Medication)
                .Include(p => p.AdministeredBy)
                .Include(p => p.VisitSchedule)
                .Include(p => p.Patient)
                .Where(s => s.isActive)
                .ToListAsync();

            // Set the sorting parameters for the view
            ViewData["NameSortParm"] = System.String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";

            // Filter by patient name
            if (!string.IsNullOrEmpty(searchPatient))
            {
                scheduledScripts = scheduledScripts
                    .Where(m => (m.Patient.FirstName + " " + m.Patient.LastName)
                    .Contains(searchPatient, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // Filter by date
            if (fromDate.HasValue)
                scheduledScripts = scheduledScripts
                    .Where(m => m.AdministeredDate >= fromDate.Value)
                    .ToList();

            if (toDate.HasValue)
                scheduledScripts = scheduledScripts
                    .Where(m => m.AdministeredDate <= toDate.Value)
                    .ToList();

            var viewModel = scheduledScripts.Select(m => new AdministerMedicationViewModel
            {
                Id = m.Id,
                PatientName = m.Patient.FirstName + " " + m.Patient.LastName,
                MedicationName = m.Medication.Name,
                Dosage = m.Dosage,
                ApplicationUserName = m.AdministeredBy.FullName,
                PrescriptionId = m.PrescriptionId,
                AdministeredDate = m.AdministeredDate,
                //HasPrescription = m.PrescriptionId != null
            }).ToList();

            // Apply sorting based on the sortOrder parameter
            switch (sortOrder)
            {
                case "name_desc":
                    viewModel = viewModel.OrderByDescending(v => v.PatientName).ToList();
                    break;
                case "Date":
                    viewModel = viewModel.OrderBy(v => v.AdministeredDate).ToList();
                    break;
                case "date_desc":
                    viewModel = viewModel.OrderByDescending(v => v.AdministeredDate).ToList();
                    break;
                default:
                    viewModel = viewModel.OrderBy(v => v.PatientName).ToList(); // Default sort
                    break;
            }

            return View(viewModel);
        }


        public async Task<IActionResult> AdministeredPdf()
        {
            var medications = await _context.PatientMedicationScripts
                .Include(p => p.Patient)
                .Include(m => m.Medication)
                .Include(a => a.AdministeredBy)
                .Include(p => p.Prescription)
                .Where(m => m.isActive)
                .OrderByDescending(m => m.AdministeredDate)
                .ToListAsync();

            using (var ms = new MemoryStream())
            {
                var doc = new iTextSharp.text.Document(PageSize.A4, 40, 40, 60, 40);
                var writer = PdfWriter.GetInstance(doc, ms);
                doc.Open();

                // Company info
                string companyName = "DEVDYNAMO LTD";
                string reportTitle = "ADMINISTERED MEDICATIONS REPORT";
                string generatedBy = User.Identity?.Name ?? "System";
                string companyAddress = "123 Medical Center, Healthcare City, 10001";
                string companyContact = "Tel: (555) 123-4567 | Email: info@healthcarefacility.com";
                string logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "logo.png");

                // Fonts
                var companyFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16, new BaseColor(0, 70, 140));
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14, BaseColor.Black);
                var infoFont = FontFactory.GetFont(FontFactory.HELVETICA, 9, BaseColor.Gray);
                var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.White);
                var bodyFont = FontFactory.GetFont(FontFactory.HELVETICA, 9, BaseColor.Black);
                var footerFont = FontFactory.GetFont(FontFactory.HELVETICA, 8, BaseColor.Gray);

                // Header section
                var headerTable = new PdfPTable(1) { WidthPercentage = 100 };
                headerTable.DefaultCell.Border = Rectangle.NO_BORDER;

                // Logo
                if (System.IO.File.Exists(logoPath))
                {
                    var logo = iTextSharp.text.Image.GetInstance(logoPath);
                    logo.ScaleAbsolute(80, 80);
                    logo.Alignment = Element.ALIGN_CENTER;
                    doc.Add(logo);
                }

                // Company name
                headerTable.AddCell(new PdfPCell(new Phrase(companyName, companyFont))
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    PaddingBottom = 4
                });

                // Report title
                headerTable.AddCell(new PdfPCell(new Phrase(reportTitle, titleFont))
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    PaddingBottom = 4
                });

                // Address and contact
                headerTable.AddCell(new PdfPCell(new Phrase(companyAddress, infoFont))
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    PaddingBottom = 2
                });

                headerTable.AddCell(new PdfPCell(new Phrase(companyContact, infoFont))
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    PaddingBottom = 10
                });

                // Generated info
                headerTable.AddCell(new PdfPCell(new Phrase($"Generated on: {DateTime.Now:MMM dd, yyyy HH:mm} | By: {generatedBy}", infoFont))
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    PaddingBottom = 15
                });

                doc.Add(headerTable);

                // Separator line
                var line = new iTextSharp.text.pdf.draw.LineSeparator(1f, 100f, BaseColor.LightGray, Element.ALIGN_CENTER, -2);
                doc.Add(new Chunk(line));
                doc.Add(new Paragraph(" "));

                if (medications.Any())
                {
                    // Table setup
                    PdfPTable table = new PdfPTable(6)
                    {
                        WidthPercentage = 100
                    };
                    table.SetWidths(new float[] { 2.5f, 2f, 1.5f, 2f, 1f, 2f });

                    // Header row
                    var headerBg = new BaseColor(70, 130, 180); // Steel blue
                    string[] headers = { "Patient", "Medication", "Dosage", "Administered By", "Prescription", "Date" };

                    foreach (var h in headers)
                    {
                        var cell = new PdfPCell(new Phrase(h, headerFont))
                        {
                            BackgroundColor = headerBg,
                            HorizontalAlignment = Element.ALIGN_CENTER,
                            VerticalAlignment = Element.ALIGN_MIDDLE,
                            Padding = 8,
                            Border = Rectangle.NO_BORDER
                        };
                        table.AddCell(cell);
                    }

                    // Alternating rows
                    var lightBlue = new BaseColor(240, 248, 255);
                    bool alternate = false;

                    foreach (var med in medications)
                    {
                        var bg = alternate ? lightBlue : BaseColor.White;

                        table.AddCell(new PdfPCell(new Phrase($"{med.Patient?.FirstName} {med.Patient?.LastName}", bodyFont)) { BackgroundColor = bg, Padding = 6 });
                        table.AddCell(new PdfPCell(new Phrase(med.Medication?.Name ?? "N/A", bodyFont)) { BackgroundColor = bg, Padding = 6 });
                        table.AddCell(new PdfPCell(new Phrase(med.Dosage ?? "N/A", bodyFont)) { BackgroundColor = bg, Padding = 6 });
                        table.AddCell(new PdfPCell(new Phrase(med.AdministeredBy?.FullName ?? "N/A", bodyFont)) { BackgroundColor = bg, Padding = 6 });
                        table.AddCell(new PdfPCell(new Phrase(med.Prescription != null ? "Yes" : "No", bodyFont)) { BackgroundColor = bg, Padding = 6, HorizontalAlignment = Element.ALIGN_CENTER });
                        table.AddCell(new PdfPCell(new Phrase(med.AdministeredDate.ToString("MMM dd, yyyy HH:mm"), bodyFont)) { BackgroundColor = bg, Padding = 6 });

                        alternate = !alternate;
                    }

                    doc.Add(table);
                }
                else
                {
                    var noData = new Paragraph("No administered medications found.", titleFont)
                    {
                        Alignment = Element.ALIGN_CENTER,
                        SpacingBefore = 20,
                        SpacingAfter = 20
                    };
                    doc.Add(noData);
                }

                // Footer
                doc.Add(new Paragraph(" "));
                doc.Add(new Chunk(line));

                var footerTable = new PdfPTable(3) { WidthPercentage = 100 };
                footerTable.SetWidths(new float[] { 1, 1, 1 });

                footerTable.AddCell(new PdfPCell(new Phrase("DevDynamo", footerFont))
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_LEFT,
                    PaddingTop = 10
                });

                footerTable.AddCell(new PdfPCell(new Phrase("Page 1", footerFont))
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    PaddingTop = 10
                });

                footerTable.AddCell(new PdfPCell(new Phrase("Confidential", footerFont))
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    PaddingTop = 10
                });

                doc.Add(footerTable);
                doc.Close();

                return File(ms.ToArray(), "application/pdf", $"AdministeredMedications_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
            }
        }



        // GET: show form and optionally fetch prescription when medicationId provided
        [HttpGet]
        public async Task<IActionResult> CreateAdminister(int patientId, int? medicationId = null)
        {
            
            var patient = await _context.Patients.Include(p => p.PatientAllergies).ThenInclude(pa => pa.Allergy).FirstOrDefaultAsync(p => p.PatientID == patientId);

            if (patient == null) return NotFound();
           

            var meds = await _context.Medications.Select(m => new
            {
                m.MedicationId,DisplayName = m.Name + " (Schedule " + m.Schedule + ")"
            }).ToListAsync();

            bool requiresPrescription = false;
            // If medication is selected, filter prescriptions for that patient + medication
            List<Prescription> prescriptions = new List<Prescription>();
            int? autoPrescriptionId = null;
            string? prescriptionDosage = null;
            if (medicationId.HasValue)
            {
                var med = await _context.Medications.FindAsync(medicationId.Value);
                if (med != null && med.Schedule > 4)
                {
                    requiresPrescription = true;
                    prescriptions = await _context.Prescriptions
                        .Include(p => p.Prescribed_Medication)
                        .Where(p => p.PatientId == patientId &&
                            p.Prescribed_Medication.Any(pm => pm.MedicationId == medicationId.Value) &&
                p.Status == "Approved")
                        .ToListAsync();
                }
                if (prescriptions.Count == 1)
                {
                    autoPrescriptionId = prescriptions.First().PrescriptionId;
                    // Get dosage from prescribed medication
                    var prescribedMed = prescriptions.First().Prescribed_Medication
                        .FirstOrDefault(pm => pm.MedicationId == medicationId.Value);
                    prescriptionDosage = prescribedMed?.Dosage;
                }
                else if (prescriptions.Count == 0)
                {
                    TempData["WarningMessage"] = "❌ Cannot administer – No approved prescription found for this medication.";
                }
            }

            var nurses = _context.Users.Where(u => u.RoleType == "NursingSister").Select(u => new { u.Id, u.FullName }).ToList();

            //var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var user = await _userManager.GetUserAsync(User);
            var model = new AdministerMedicationViewModel
            {
                PatientId = patient.PatientID,
                PatientName = $"{patient.FirstName} {patient.LastName}",
                ApplicationUserID = user.Id,
                ApplicationUserName = user.FullName,
                AdministeredDate = DateTime.Now,
                MedicationList = new SelectList(meds, "MedicationId", "DisplayName", medicationId),
                UserList = new SelectList(nurses, "Id", "FullName"),
                //PrescriptionList = new SelectList(prescriptions, "Id", "PrescriptionNote"), // or some meaningful display
                PrescriptionList = new SelectList(prescriptions, "PrescriptionId", "PrescriptionNote"),
                PrescriptionId = autoPrescriptionId,
                PrescriptionDosage = prescriptionDosage,
                RequiresPrescription = requiresPrescription,
                PatientAllergies = patient.PatientAllergies
                            .Select(pa => pa.Allergy.Name)
                            .ToList()
            };

            return View(model);
        }


        // POST: save administration
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAdminister(AdministerMedicationViewModel vm)
        {
            var user = await _userManager.GetUserAsync(User);
            vm.ApplicationUserID = user.Id;

            var med = await _context.Medications.FindAsync(vm.MedicationId);
            vm.RequiresPrescription = med?.Schedule > 4;
            

            // If medication schedule is high and no prescription found, add error
            if (vm.RequiresPrescription && vm.PrescriptionId == null)
            {
                ModelState.AddModelError("PrescriptionId", "A prescription is required for Schedule 5 and above medications.");
            }

            if (vm.RequiresPrescription && vm.PrescriptionId != null)
            {
                var prescription = await _context.Prescriptions
                    .Where(p => p.PrescriptionId == vm.PrescriptionId && p.Status == "Approved") 
                    .FirstOrDefaultAsync();

                if (prescription == null)
                {
                    ModelState.AddModelError("PrescriptionId", "The selected prescription is not approved.");
                }
            }
            // ADD ALLERGY CHECK 
            var patientAllergies = await _context.PatientAllergies.Include(pa => pa.Allergy).Where(pa => pa.PatientId == vm.PatientId).ToListAsync();

            bool hasAllergyConflict = patientAllergies.Any(pa => med.Name.Contains(pa.Allergy.Name, StringComparison.OrdinalIgnoreCase));

            if (hasAllergyConflict)
            {
                ModelState.AddModelError("", "This medication may cause an allergic reaction! Cannot administer.");
            }


            if (!ModelState.IsValid)
            {
                var patient = await _context.Patients.FindAsync(vm.PatientId);
                if (patient != null)
                    vm.PatientName = $"{patient.FirstName} {patient.LastName}";

                var meds = await _context.Medications
                    .Select(m => new
                    {
                        m.MedicationId,
                        DisplayName = m.Name + " (Schedule " + m.Schedule + ")"
                    })
                    .ToListAsync();

                var nurses = _context.Users.Where(u => u.RoleType == "NursingSister").Select(u => new { u.Id, u.FullName }).ToList();


                List<Prescription> prescriptions = new List<Prescription>();
                if (vm.RequiresPrescription && vm.MedicationId > 0)
                {
                    prescriptions = await _context.Prescriptions
                        .Include(p => p.Prescribed_Medication)
                        .Where(p => p.PatientId == vm.PatientId &&
                                   p.Prescribed_Medication.Any(pm => pm.MedicationId == vm.MedicationId) &&
                                   p.Status == "Approved")
                        .ToListAsync();
                }

                    vm.MedicationList = new SelectList(meds, "MedicationId", "DisplayName", vm.MedicationId);
                vm.UserList = new SelectList(nurses, "Id", "FullName", vm.ApplicationUserID);
                /*vm.PrescriptionList = new SelectList(prescriptions, "Id", "PrescriptionNote", vm.PrescriptionId);*/
                vm.PrescriptionList = new SelectList(prescriptions, "PrescriptionId", "PrescriptionNote", vm.PrescriptionId);

                return View(vm);
            }

            // Find prescription again
            var script = new PatientMedicationScript
            {
                PatientId = vm.PatientId,
                MedicationId = vm.MedicationId,
                Dosage = vm.Dosage,
                AdministeredDate = DateTime.Now,
                ApplicationUserID = vm.ApplicationUserID,
                PrescriptionId = vm.RequiresPrescription ? vm.PrescriptionId : null,
                isActive = true
            };

            if (med != null)
            {
                if (med.Quantity > 0)
                {
                    med.Quantity -= 1; // Reduce by 1 unit per administration
                }
                else
                {
                    ModelState.AddModelError("", "Medication is out of stock!");
                    var patient = await _context.Patients.FindAsync(vm.PatientId);
                    if (patient != null)
                        vm.PatientName = $"{patient.FirstName} {patient.LastName}";

                    var meds = await _context.Medications
                        .Select(m => new
                        {
                            m.MedicationId,
                            DisplayName = m.Name + " (Schedule " + m.Schedule + ")"
                        })
                        .ToListAsync();

                    var nurses = _context.Users.Where(u => u.RoleType == "NursingSister").Select(u => new { u.Id, u.FullName }).ToList();

                    var prescriptions = vm.RequiresPrescription
                        ? await _context.Prescriptions.Where(p => p.PatientId == vm.PatientId).ToListAsync()
                        : new List<Prescription>();

                    vm.MedicationList = new SelectList(meds, "MedicationId", "DisplayName", vm.MedicationId);
                    vm.UserList = new SelectList(nurses, "Id", "FullName", vm.ApplicationUserID);
                    /*vm.PrescriptionList = new SelectList(prescriptions, "Id", "PrescriptionNote", vm.PrescriptionId);*/
                    vm.PrescriptionList = new SelectList(prescriptions ?? new List<Prescription>(), "PrescriptionId", "PrescriptionNote");


                    return View(vm);
                }
            }

            _context.PatientMedicationScripts.Add(script);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Medication successfully administered.";
            return RedirectToAction("ListAdministered");
        }


        public async Task<JsonResult> GetPrescriptions(int patientId, int medicationId)
        {
            // medicationId is now non-nullable
            var prescriptions = await _context.Prescriptions
                .Include(p => p.Prescribed_Medication)
                .Where(p => p.PatientId == patientId &&
                            p.Prescribed_Medication.Any(pm => pm.MedicationId == medicationId) && p.Status == "Approved")
                .Select(p => new
                {
                    p.PrescriptionId,
                    dosage = p.Prescribed_Medication
                    .FirstOrDefault(pm => pm.MedicationId == medicationId)
                    .Dosage
                    /*,
                    p.PrescriptionInstruction*/
                })
                .ToListAsync();

            return Json(prescriptions );
        }



       





        
        public async Task<IActionResult> EditAdministered(int id)
        {
            var script = await _context.PatientMedicationScripts
                .Include(p => p.Patient)
                .Include(m => m.Medication)
                .Include(a => a.AdministeredBy)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (script == null)
                return NotFound();

            var meds = _context.Medications.Select(m => new
            {
                m.MedicationId,
                DisplayName = m.Name + " (Schedule " + m.Schedule + ")"
            }).ToList();

            var loggedInUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var loggedInUser = await _context.Users.FindAsync(loggedInUserId);

            // NEW: Determine if prescription is required and load appropriate prescriptions
            bool requiresPrescription = script.Medication?.Schedule > 4;
            List<Prescription> prescriptions = new List<Prescription>();

            if (requiresPrescription && script.MedicationId != 0)
            {
                prescriptions = await _context.Prescriptions
                    .Include(p => p.Prescribed_Medication)
                    .Where(p => p.PatientId == script.PatientId &&
                               p.Prescribed_Medication.Any(pm => pm.MedicationId == script.MedicationId) &&
                               p.Status == "Approved")
                    .ToListAsync();
            }
            if (script.PrescriptionId != null)
            {
                TempData["ErrorMessage"] = "You cannot edit prescribed medication.";
                return RedirectToAction("ListAdministered");
            }

            var vm = new AdministerMedicationViewModel
            {
                Id = script.Id, // Make sure to include the ID for editing
                PrescriptionId = script.PrescriptionId,
                PatientId = script.PatientId,
                PatientName = $"{script.Patient.FirstName} {script.Patient.LastName}",
                MedicationId = script.MedicationId,
                Dosage = script.Dosage,
                AdministeredDate = script.AdministeredDate,
                ApplicationUserID = loggedInUser.Id,
                ApplicationUserName = loggedInUser.FullName,
                MedicationList = new SelectList(meds, "MedicationId", "DisplayName", script.MedicationId),
                PrescriptionList = new SelectList(prescriptions.Select(p => new {
                    p.PrescriptionId,
                    Display = $"Prescription #{p.PrescriptionId}"
                }), "PrescriptionId", "Display", script.PrescriptionId),
                RequiresPrescription = requiresPrescription // Add this flag
            };

            return View(vm);
        }




        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAdministered(AdministerMedicationViewModel vm)
        {
            var med = await _context.Medications.FindAsync(vm.MedicationId);
            if (med == null)
                ModelState.AddModelError("", "Medication not found.");

            vm.RequiresPrescription = med?.Schedule > 4;

            // Require prescription validation for schedule 5 and above
            if (vm.RequiresPrescription)
            {
                if (vm.PrescriptionId == null)
                {
                    ModelState.AddModelError("PrescriptionId", "A prescription is required for Schedule 5 and above medications.");
                }
                else
                {
                    var prescription = await _context.Prescriptions
                        .Include(p => p.Prescribed_Medication)
                        .Where(p => p.PrescriptionId == vm.PrescriptionId &&
                                   p.PatientId == vm.PatientId &&
                                   p.Prescribed_Medication.Any(pm => pm.MedicationId == vm.MedicationId) &&
                                   p.Status == "Approved")
                        .FirstOrDefaultAsync();

                    if (prescription == null)
                    {
                        ModelState.AddModelError("PrescriptionId", "The selected prescription is not approved or doesn't match the medication.");
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                // Reload dropdowns
                var meds = _context.Medications
                    .Select(m => new
                    {
                        m.MedicationId,
                        DisplayName = m.Name + " (Schedule " + m.Schedule + ")"
                    })
                    .ToList();

                vm.MedicationList = new SelectList(meds, "MedicationId", "DisplayName", vm.MedicationId);

                // Reload prescriptions for the specific medication if needed
                List<Prescription> prescriptions = new List<Prescription>();
                if (vm.RequiresPrescription && vm.MedicationId > 0)
                {
                    prescriptions = await _context.Prescriptions
                        .Include(p => p.Prescribed_Medication)
                        .Where(p => p.PatientId == vm.PatientId &&
                                   p.Prescribed_Medication.Any(pm => pm.MedicationId == vm.MedicationId) &&
                                   p.Status == "Approved")
                        .ToListAsync();
                }

                vm.PrescriptionList = new SelectList(prescriptions.Select(p => new
                {
                    p.PrescriptionId,
                    DisplayName = $"Prescription #{p.PrescriptionId}"
                }), "PrescriptionId", "DisplayName", vm.PrescriptionId);

                return View(vm);
            }

            // Fetch existing record
            var existing = await _context.PatientMedicationScripts.FirstOrDefaultAsync(x => x.Id == vm.Id);
            if (existing == null) return NotFound();

            // Update entity properties from ViewModel
            existing.MedicationId = vm.MedicationId;
            existing.Dosage = vm.Dosage;
            existing.AdministeredDate = vm.AdministeredDate;
            existing.PrescriptionId = vm.RequiresPrescription ? vm.PrescriptionId : null; // Only set if required

            var loggedInUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            existing.ApplicationUserID = loggedInUserId;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Medication updated successfully.";
            return RedirectToAction("ListAdministered");
        }






        public async Task<IActionResult> DeleteAdministered(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var medication = await _context.PatientMedicationScripts.Include(p => p.Patient).Include(v => v.VisitSchedule).Include(a => a.AdministeredBy).Include(m => m.Medication).FirstOrDefaultAsync(a => a.Id == id);
            if (medication == null)
            {
                return NotFound();
            }

            ViewBag.CurrentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);


            return View(medication);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAdminister(int id)
        {
            var medication = await _context.PatientMedicationScripts.FindAsync(id);
            if (medication != null)
            {
                medication.isActive = false;
                _context.PatientMedicationScripts.Update(medication);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Medication successfully Deleted!";
            }
            return RedirectToAction(nameof(ListAdministered));
        }


        public async Task<IActionResult> LiveSearch(string query, string ward, string bed)
        {
            var wards = await _context.Wards.ToListAsync();
            var beds = await _context.Beds.Where(b => !b.IsDeleted).ToListAsync();

            // Pass to the view
            ViewBag.Wards = new SelectList(wards, "WardID", "Name");
            ViewBag.Beds = new SelectList(beds, "BedId", "BedNo");


            var patientsQuery = (
                    from p in _context.Patients
                    join a in _context.Admissions on p.PatientID equals a.PatientID into admissionGroup
                    from a in admissionGroup.DefaultIfEmpty()
                    join w in _context.Wards on a.WardID equals w.WardID into wardGroup
                    from w in wardGroup.DefaultIfEmpty()
                    join b in _context.Beds on a.BedID equals b.BedId into bedGroup
                    from b in bedGroup.DefaultIfEmpty()
                    join d in _context.Discharges on p.PatientID equals d.PatientID into dischargeGroup
                    from d in dischargeGroup.OrderByDescending(x => x.DischargeDate).Take(1).DefaultIfEmpty()
                    select new
                    {
                        Patient = p,
                        Admission = a,
                        Ward = w,
                        Bed = b,
                        LatestDischarge = d
                    }
                ).AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
            {
                patientsQuery = patientsQuery.Where(p => p.Patient.FirstName.Contains(query) || p.Patient.LastName.Contains(query));
            }

            if (!string.IsNullOrWhiteSpace(ward))
            {
                patientsQuery = patientsQuery.Where(p => p.Ward != null && p.Ward.Name == ward);
            }

            if (!string.IsNullOrWhiteSpace(bed))
            {
                patientsQuery = patientsQuery.Where(p => p.Bed != null && p.Bed.BedNo.StartsWith(bed));
            }

            var patients = await patientsQuery.ToListAsync();

            // Get unread counts for each patient
            var viewModel = new List<PatientDashboardViewModel>();

            foreach (var p in patients)
            {
                // Count unread instructions for this patient
                var unreadCount = await _context.Instructions
                    .CountAsync(i => i.PatientID == p.Patient.PatientID && !i.IsRead && i.isActive);

                viewModel.Add(new PatientDashboardViewModel
                {
                    PatientID = p.Patient.PatientID,
                    FirstName = p.Patient.FirstName,
                    LastName = p.Patient.LastName,
                    WardName = p.Ward != null ? p.Ward.Name : "N/A",
                    BedNo = p.Bed != null ? p.Bed.BedNo : "N/A",
                    Status = p.LatestDischarge != null && p.LatestDischarge.IsDischarged == true ? "Discharged" :
                    p.Admission != null ? "Admitted" : "Not Admitted",
                    UnreadAdviceCount = unreadCount  // Add this line
                });
            }

            return PartialView("_PatientSearchResults", viewModel);
        }

        


        public async Task<IActionResult> LiveSearchAll()
        {
            var data = await (
                from p in _context.Patients
                join a in _context.Admissions on p.PatientID equals a.PatientID into admissionGroup
                from a in admissionGroup.DefaultIfEmpty()
                join w in _context.Wards on a.WardID equals w.WardID into wardGroup
                from w in wardGroup.DefaultIfEmpty()
                join b in _context.Beds on a.BedID equals b.BedId into bedGroup
                from b in bedGroup.DefaultIfEmpty()
                join d in _context.Discharges on p.PatientID equals d.PatientID into dischargeGroup
                from d in dischargeGroup.OrderByDescending(x => x.DischargeDate).Take(1).DefaultIfEmpty()
                select new PatientDashboardViewModel
                {
                    PatientID = p.PatientID,
                    FirstName = p.FirstName,
                    LastName = p.LastName,
                    WardName = w != null ? w.Name : "N/A",
                    BedNo = b != null ? b.BedNo : "N/A",
                    Status = d != null && d.IsDischarged == true ? "Discharged" :
                              a != null ? "Admitted" : "Not Admitted",
                    UnreadAdviceCount = _context.Instructions.Count(i => i.PatientID == p.PatientID && !i.IsRead && i.isActive) // Add this line
                }
            ).ToListAsync();

            return PartialView("_PatientSearchResults", data);
        }



        public async Task<IActionResult> DownloadPrescription(int id)
        {
            var prescription = await _context.Prescriptions
                .Include(p => p.Patient)
                .Include(p => p.User)
                .Include(p => p.Prescribed_Medication)
                    .ThenInclude(pm => pm.Medication)
                .FirstOrDefaultAsync(p => p.PrescriptionId == id);

            if (prescription == null) return NotFound();

            using (var ms = new MemoryStream())
            {
                var doc = new Document(PageSize.A4, 50, 50, 50, 50);
                PdfWriter.GetInstance(doc, ms);
                doc.Open();

                
                // HEADER (Logo + Company)
               
                PdfPTable headerTable = new PdfPTable(2);
                headerTable.WidthPercentage = 100;

                // Logo 
                string logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "logo.png");
                if (System.IO.File.Exists(logoPath))
                {
                    iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(logoPath);
                    logo.ScaleAbsolute(60f, 60f);
                    PdfPCell logoCell = new PdfPCell(logo);
                    logoCell.Border = Rectangle.NO_BORDER;
                    logoCell.HorizontalAlignment = Element.ALIGN_LEFT;
                    headerTable.AddCell(logoCell);
                }
                else
                {
                    headerTable.AddCell(new PdfPCell(new Phrase("")) { Border = Rectangle.NO_BORDER });
                }

                // Company name
                var companyFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 20, BaseColor.Black);
                PdfPCell companyCell = new PdfPCell(new Phrase("CyberMed-Care / DevDynamo", companyFont));
                companyCell.Border = Rectangle.NO_BORDER;
                companyCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                companyCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                headerTable.AddCell(companyCell);

                doc.Add(headerTable);
                doc.Add(new Paragraph("\n"));

                
                // Title
                
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
                doc.Add(new Paragraph("Prescription", titleFont));
                doc.Add(new Paragraph("\n"));

               
                // Patient & Doctor Info
               
                doc.Add(new Paragraph($"Prescription ID: {prescription.PrescriptionId}"));
                doc.Add(new Paragraph($"Patient: {prescription.Patient.FirstName} {prescription.Patient.LastName}"));
                doc.Add(new Paragraph($"Issued By: {prescription.User.FullName}"));
                doc.Add(new Paragraph($"Date Issued: {prescription.DateIssued.ToString("yyyy-MM-dd")}"));
                doc.Add(new Paragraph("\n"));

                
                
                // Table of Medications
               
                if (prescription.Prescribed_Medication.Any())
                {
                    var table = new PdfPTable(3); // Columns: Medication, Dosage, Schedule
                    table.WidthPercentage = 100;

                    // Header
                    var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
                    table.AddCell(new Phrase("Medication", headerFont));
                    table.AddCell(new Phrase("Dosage", headerFont));
                    table.AddCell(new Phrase("Schedule", headerFont));

                    foreach (var pm in prescription.Prescribed_Medication)
                    {
                        table.AddCell(pm.Medication?.Name ?? "N/A");
                        table.AddCell(pm.Dosage ?? "N/A");
                        table.AddCell(pm.Medication?.Schedule.ToString() ?? "N/A");
                    }

                    doc.Add(table);
                }
                else
                {
                    doc.Add(new Paragraph("No medications listed for this prescription."));
                }

                doc.Close();

                return File(ms.ToArray(), "application/pdf",
                    $"Prescription_{prescription.Patient.FirstName}_{prescription.Patient.LastName}.pdf");
            }
        }





    }
}
