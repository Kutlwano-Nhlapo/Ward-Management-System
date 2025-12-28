using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ONT_3rdyear_Project.Data;
using ONT_3rdyear_Project.Models;
using ONT_3rdyear_Project.ViewModels;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace ONT_3rdyear_Project.Controllers
{
    [Authorize(Roles = "Nurse, Sister")]
    public class NurseController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public NurseController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
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
                .Where(m =>  m.isActive)
                .CountAsync();

            var totalVitals = await _context.Vitals.Where(v=>v.IsActive).CountAsync();
            var model = new DashboardViewModel
            {
                Stats = new DashboardStatsViewModel
                {
                    TotalPatients = totalPatients,
                    TreatmentsToday = treatmentsToday,
                    MedicationsGivenToday = medicationsGivenToday,
                    
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
            ViewData["NameSortParm"] = System.String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
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
                    /*Status = a != null ? "Admitted" : "Not Admitted"*/
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












        //CRUD for Vitals

        public async Task<IActionResult> Vitals(string searchPatient, DateTime? fromDate, DateTime? toDate, string sortOrder)
        {
            var vitalsQuery = _context.Vitals
                .Include(v => v.Patient)
                .Include(v => v.VisitSchedule)
                .Include(v => v.User)
                .Where(v => v.IsActive);

            // Set the sorting parameters for the view
            ViewData["NameSortParm"] = System.String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";

            if (!string.IsNullOrEmpty(searchPatient))
            {
                vitalsQuery = vitalsQuery.Where(v => v.Patient.FirstName.Contains(searchPatient) ||  v.Patient.LastName.Contains(searchPatient));
            }

            if (fromDate.HasValue)
            {
                vitalsQuery = vitalsQuery.Where(v => v.Date >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                vitalsQuery = vitalsQuery.Where(v => v.Date <= toDate.Value);
            }

            var vitals = await vitalsQuery.OrderByDescending(v => v.Date).ToListAsync();

            // Apply sorting based on the sortOrder parameter
            switch (sortOrder)
            {
                case "name_desc":
                    vitals = vitals.OrderByDescending(p => p.Patient.FirstName).ToList();
                    break;
                case "Date":
                    vitals = vitals.OrderBy(p => p.Date).ToList();
                    break;
                case "date_desc":
                    vitals = vitals.OrderByDescending(p => p.Date).ToList();
                    break;
                default:
                    vitals = vitals.OrderBy(p => p.Patient.FirstName).ToList(); // Default sort
                    break;
            }
            return View(vitals);
        }

        public async Task<IActionResult> VitalsPdf()
        {
            var vitals = await _context.Vitals
                .Include(v => v.Patient)
                .Include(v => v.User)
                .Where(v => v.IsActive)
                .OrderByDescending(v => v.Date)
                .ToListAsync();

            // Custom report info
            string CompanyName = "DevDynamo LTD";
            string ReportTitle = "Patient Progress Report";
            string GeneratedBy = User.Identity?.Name ?? "System";
            string CompanyAddress = "123 Medical Center, Healthcare City, 10001";
            string CompanyContact = "Tel: (555) 123-4567 | Email: info@healthcarefacility.com";
            string GeneratedDate = DateTime.Now.ToString("MMM dd, yyyy HH:mm");

            using (var ms = new MemoryStream())
            {
                var doc = new iTextSharp.text.Document(PageSize.A4, 40, 40, 80, 60);
                var writer = PdfWriter.GetInstance(doc, ms);
                doc.Open();

                // ===== Fonts =====
                var companyFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, new BaseColor(40, 40, 40));
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14, new BaseColor(70, 130, 180));
                var smallFont = FontFactory.GetFont(FontFactory.HELVETICA, 9, BaseColor.Gray);
                var subHeaderFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.DarkGray);
                var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11, BaseColor.White);
                var bodyFont = FontFactory.GetFont(FontFactory.HELVETICA, 9, BaseColor.Black);
                var footerFont = FontFactory.GetFont(FontFactory.HELVETICA, 8, BaseColor.Gray);

                // ===== Header with Company Info =====
                var headerTable = new PdfPTable(2) { WidthPercentage = 100 };
                headerTable.SetWidths(new float[] { 4f, 1.2f });

                // Left: Company info (centered block)
                var infoTable = new PdfPTable(1) { WidthPercentage = 100 };
                infoTable.DefaultCell.Border = Rectangle.NO_BORDER;

                infoTable.AddCell(new PdfPCell(new Phrase(CompanyName, companyFont))
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    PaddingBottom = 2
                });
                infoTable.AddCell(new PdfPCell(new Phrase(ReportTitle, titleFont))
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    PaddingBottom = 3
                });
                infoTable.AddCell(new PdfPCell(new Phrase($"Generated by: {GeneratedBy}", smallFont))
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    PaddingBottom = 2
                });
                infoTable.AddCell(new PdfPCell(new Phrase($"Generated on: {GeneratedDate}", smallFont))
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    PaddingBottom = 2
                });
                infoTable.AddCell(new PdfPCell(new Phrase(CompanyAddress, smallFont))
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    PaddingBottom = 1
                });
                infoTable.AddCell(new PdfPCell(new Phrase(CompanyContact, smallFont))
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_CENTER
                });

                var infoCell = new PdfPCell(infoTable)
                {
                    Border = Rectangle.NO_BORDER,
                    VerticalAlignment = Element.ALIGN_MIDDLE
                };

                // Right: Logo
                var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "logo.png");
                PdfPCell logoCell;
                if (System.IO.File.Exists(logoPath))
                {
                    var logo = iTextSharp.text.Image.GetInstance(logoPath);
                    logo.ScaleToFit(80f, 80f);
                    logoCell = new PdfPCell(logo)
                    {
                        Border = Rectangle.NO_BORDER,
                        HorizontalAlignment = Element.ALIGN_RIGHT,
                        VerticalAlignment = Element.ALIGN_MIDDLE
                    };
                }
                else
                {
                    logoCell = new PdfPCell(new Phrase("Company Logo", subHeaderFont))
                    {
                        Border = Rectangle.NO_BORDER,
                        HorizontalAlignment = Element.ALIGN_RIGHT,
                        PaddingTop = 10
                    };
                }

                headerTable.AddCell(infoCell);
                headerTable.AddCell(logoCell);
                doc.Add(headerTable);

                // Line separator
                var line = new iTextSharp.text.pdf.draw.LineSeparator(1f, 100f, new BaseColor(70, 130, 180), Element.ALIGN_CENTER, -2);
                doc.Add(new Paragraph(" "));
                doc.Add(new Chunk(line));
                doc.Add(new Paragraph(" "));

                // ===== Summary Section =====
                var infoSummary = new PdfPTable(2) { WidthPercentage = 100 };
                infoSummary.SetWidths(new float[] { 1, 1 });

                infoSummary.AddCell(new PdfPCell(new Phrase($"Total Records: {vitals.Count}", subHeaderFont))
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_LEFT
                });
                infoSummary.AddCell(new PdfPCell(new Phrase($"Report Period: Last Updated - {DateTime.Now:MMM dd, yyyy}", subHeaderFont))
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_RIGHT
                });
                doc.Add(infoSummary);
                doc.Add(new Paragraph(" "));

                // ===== Data Table =====
                if (vitals.Any())
                {
                    var table = new PdfPTable(7) { WidthPercentage = 100 };
                    table.SetWidths(new float[] { 2.2f, 2.2f, 1f, 1f, 1f, 1f, 2f });

                    var headerBg = new BaseColor(70, 130, 180);
                    string[] headers = { "Date & Time", "Patient", "BP", "Sugar", "Temp", "Pulse", "Recorded By" };

                    foreach (var h in headers)
                    {
                        var cell = new PdfPCell(new Phrase(h, headerFont))
                        {
                            BackgroundColor = headerBg,
                            HorizontalAlignment = Element.ALIGN_CENTER,
                            Padding = 8,
                            Border = Rectangle.NO_BORDER
                        };
                        table.AddCell(cell);
                    }

                    bool alternate = false;
                    var altColor = new BaseColor(245, 250, 255);

                    foreach (var v in vitals)
                    {
                        var bg = alternate ? altColor : BaseColor.White;
                        alternate = !alternate;

                        table.AddCell(new PdfPCell(new Phrase(v.Date.ToString("MMM dd, yyyy HH:mm"), bodyFont)) { BackgroundColor = bg, Padding = 6 });
                        table.AddCell(new PdfPCell(new Phrase($"{v.Patient?.FirstName} {v.Patient?.LastName}", bodyFont)) { BackgroundColor = bg, Padding = 6 });
                        table.AddCell(new PdfPCell(new Phrase(v.BP ?? "-", bodyFont)) { BackgroundColor = bg, Padding = 6, HorizontalAlignment = Element.ALIGN_CENTER });
                        table.AddCell(new PdfPCell(new Phrase(v.SugarLevel.ToString() ?? "-", bodyFont)) { BackgroundColor = bg, Padding = 6, HorizontalAlignment = Element.ALIGN_CENTER });
                        table.AddCell(new PdfPCell(new Phrase(v.Temperature.ToString("F1"), bodyFont)) { BackgroundColor = bg, Padding = 6, HorizontalAlignment = Element.ALIGN_CENTER });
                        table.AddCell(new PdfPCell(new Phrase(v.PulseRate.ToString() ?? "-", bodyFont)) { BackgroundColor = bg, Padding = 6, HorizontalAlignment = Element.ALIGN_CENTER });
                        table.AddCell(new PdfPCell(new Phrase(v.User?.FullName ?? "Unknown", bodyFont)) { BackgroundColor = bg, Padding = 6 });
                    }

                    doc.Add(table);
                }
                else
                {
                    var noData = new Paragraph("No active vital records found.", subHeaderFont)
                    {
                        Alignment = Element.ALIGN_CENTER,
                        SpacingBefore = 20
                    };
                    doc.Add(noData);
                }

                // ===== Footer =====
                doc.Add(new Paragraph(" "));
                doc.Add(new Chunk(line));
                var footerTable = new PdfPTable(3) { WidthPercentage = 100 };
                footerTable.SetWidths(new float[] { 1, 1, 1 });

                footerTable.AddCell(new PdfPCell(new Phrase("DevDynamo Ltd", footerFont))
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_LEFT
                });
                footerTable.AddCell(new PdfPCell(new Phrase("Confidential", footerFont))
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_CENTER
                });
                footerTable.AddCell(new PdfPCell(new Phrase($"Page 1", footerFont))
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_RIGHT
                });

                doc.Add(footerTable);

                doc.Close();
                return File(ms.ToArray(), "application/pdf", $"VitalsReport_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
            }
        }




        public async Task<IActionResult> CreateVital(int PatientID)
        {
            var patient = await _context.Patients.FindAsync(PatientID);
            if (patient == null)
            {
                return NotFound(); // patient ID doesn't exist
            }

            ViewBag.PatientName = $"{patient.FirstName} {patient.LastName}";
            ViewBag.PatientId = PatientID;

            
            var user = await _userManager.GetUserAsync(User);
            ViewBag.CurrentUserName = user.FullName;

            return View();
        }


        //vitals details
        public async Task<IActionResult> VitalsDetails(int? id)
        {
            if(id == null || _context.Vitals == null)
            {
                return NotFound();
            }
            var vitals = await _context.Vitals.Include(v => v.Patient).Include(v => v.User).FirstOrDefaultAsync(v => v.VitalID == id);
            return View(vitals);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateVital(Vital vital)
        {
            var user = await _userManager.GetUserAsync(User);
            vital.ApplicationUserID = user.Id;
            if (ModelState.IsValid)
            {
                _context.Add(vital);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Vitals successfully taken!";
                return RedirectToAction(nameof(Vitals));
            }

            ViewBag.PatientList = new SelectList(_context.Patients.ToList(), "PatientID", "FirstName", vital.PatientID);
            var nurses = await _context.Users.Where(u => u.RoleType == "Nurse").ToListAsync();
            ViewBag.UserList = new SelectList(nurses, "Id", "FullName", vital.ApplicationUserID);
            ViewBag.VisitList = new SelectList(_context.VisitSchedules.ToList(), "VisitID", "VisitDate", vital.VisitID);


            var userId = int.Parse(_userManager.GetUserId(User));
            vital.ApplicationUserID = userId;
            return View(vital);
        }

       

        public async Task<IActionResult> EditVital(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }
            var vital = await _context.Vitals.FindAsync(id);
            if(vital == null)
            {
                return NotFound();
            }
            var nurseId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var nurse = await _context.Users.FindAsync(nurseId);
            ViewBag.NurseName = nurse?.FullName;
            ViewBag.PatientList = new SelectList(_context.Patients, "PatientID", "FirstName", vital.PatientID);
            ViewBag.UserList = new SelectList(_context.Users, "Id", "FullName", vital.ApplicationUserID);
            ViewBag.VisitList = new SelectList(_context.VisitSchedules, "VisitID", "VisitDate", vital.VisitID);

            return View(vital);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditVital(int id, Vital vital)
        {
            if (id != vital.VitalID)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    vital.ApplicationUserID = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                    _context.Update(vital);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Vitals successfully updated!";

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Vitals.Any(e => e.VitalID == id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Vitals));
            }

           
            ViewBag.PatientList = new SelectList(_context.Patients, "PatientID", "FirstName", vital.PatientID);
            ViewBag.UserList = new SelectList(_context.Users, "Id", "FullName", vital.ApplicationUserID);
            ViewBag.VisitList = new SelectList(_context.VisitSchedules, "VisitID", "VisitDate", vital.VisitID);

            return View(vital);
        }

        public async Task<IActionResult> DeleteVital(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var vital = await _context.Vitals.Include(p => p.Patient).Include(u => u.User).Include(v => v.VisitSchedule).FirstOrDefaultAsync(v => v.VitalID == id);
            if(vital == null)
            {
                return NotFound();
            }
            return View(vital);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteVital(int id)
        {
            
            var vital = await _context.Vitals.FindAsync(id);
            if (vital == null)
                return NotFound();

            vital.IsActive = false; // soft delete
            _context.Vitals.Update(vital);
            await _context.SaveChangesAsync();
            
            return Ok();
        }
































        //TREATMENT CRUD
      
       public async Task<IActionResult>Treatments(string searchPatient, DateTime? fromDate, DateTime? toDate, string sortOrder)
        {
            var treatmentQuery = _context.Treatments.Include(p => p.Patient).Include(p => p.User).Include(p => p.VisitSchedule).Where(p => p.IsActive);

            // Set the sorting parameters for the view
            ViewData["NameSortParm"] = System.String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";

            if (!string.IsNullOrEmpty(searchPatient))
            {
                treatmentQuery = treatmentQuery.Where(p => p.Patient.FirstName.Contains(searchPatient) || p.Patient.LastName.Contains(searchPatient));
            }

            if (fromDate.HasValue)
            {
                treatmentQuery = treatmentQuery.Where(p => p.TreatmentDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                treatmentQuery = treatmentQuery.Where(p => p.TreatmentDate <= toDate.Value);
            }
            var treatments = await treatmentQuery.OrderByDescending(p => p.TreatmentDate).ToListAsync();

            // Apply sorting based on the sortOrder parameter
            switch (sortOrder)
            {
                case "name_desc":
                    treatments = treatments.OrderByDescending(p => p.Patient.FirstName).ToList();
                    break;
                case "Date":
                    treatments = treatments.OrderBy(p => p.TreatmentDate).ToList();
                    break;
                case "date_desc":
                    treatments = treatments.OrderByDescending(p => p.TreatmentDate).ToList();
                    break;
                default:
                    treatments = treatments.OrderBy(p => p.Patient.FirstName).ToList(); // Default sort
                    break;
            }
            return View(treatments);
        }
        public async Task<IActionResult> TreatmentsPdf()
        {
            var treatments = await _context.Treatments
                .Include(v => v.Patient)
                .Include(v => v.User)
                .Where(v => v.IsActive)
                .OrderByDescending(v => v.TreatmentDate)
                .ToListAsync();

            using (var ms = new MemoryStream())
            {
                var doc = new iTextSharp.text.Document(PageSize.A4, 40, 40, 60, 40);
                var writer = PdfWriter.GetInstance(doc, ms);
                doc.Open();

                // Company details
                string companyName = "DEVDYNAMO LTD";
                string reportTitle = "TREATMENTS REPORT";
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

                // Header table (Logo + Company Info)
                var headerTable = new PdfPTable(1) { WidthPercentage = 100 };
                headerTable.DefaultCell.Border = Rectangle.NO_BORDER;

                // Company Logo (if exists)
                if (System.IO.File.Exists(logoPath))
                {
                    var logo = iTextSharp.text.Image.GetInstance(logoPath);
                    logo.ScaleAbsolute(80, 80);
                    logo.Alignment = Element.ALIGN_CENTER;
                    doc.Add(logo);
                }

                // Company Name
                var companyCell = new PdfPCell(new Phrase(companyName, companyFont))
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    PaddingBottom = 4
                };
                headerTable.AddCell(companyCell);

                // Report Title
                var titleCell = new PdfPCell(new Phrase(reportTitle, titleFont))
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    PaddingBottom = 4
                };
                headerTable.AddCell(titleCell);

                // Address & Contact
                var addressCell = new PdfPCell(new Phrase(companyAddress, infoFont))
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    PaddingBottom = 2
                };
                headerTable.AddCell(addressCell);

                var contactCell = new PdfPCell(new Phrase(companyContact, infoFont))
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    PaddingBottom = 10
                };
                headerTable.AddCell(contactCell);

                // Generated info
                var genInfoCell = new PdfPCell(new Phrase($"Generated on: {DateTime.Now:MMM dd, yyyy HH:mm}  |  By: {generatedBy}", infoFont))
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    PaddingBottom = 15
                };
                headerTable.AddCell(genInfoCell);

                doc.Add(headerTable);

                // Line separator
                var line = new iTextSharp.text.pdf.draw.LineSeparator(1f, 100f, BaseColor.LightGray, Element.ALIGN_CENTER, -2);
                doc.Add(new Chunk(line));
                doc.Add(new Paragraph(" "));

                if (treatments.Any())
                {
                    // Create data table
                    var table = new PdfPTable(4) { WidthPercentage = 100 };
                    table.SetWidths(new float[] { 2.5f, 2.5f, 2.5f, 2.5f });

                    var headerBg = new BaseColor(70, 130, 180); // Steel blue
                    string[] headers = { "Date & Time", "Patient", "Treatment Type", "Treated By" };

                    foreach (string header in headers)
                    {
                        var headerCell = new PdfPCell(new Phrase(header, headerFont))
                        {
                            BackgroundColor = headerBg,
                            HorizontalAlignment = Element.ALIGN_CENTER,
                            VerticalAlignment = Element.ALIGN_MIDDLE,
                            Padding = 8,
                            Border = Rectangle.BOX,
                            BorderColor = BaseColor.White,
                            BorderWidth = 1
                        };
                        table.AddCell(headerCell);
                    }

                    var lightBlue = new BaseColor(240, 248, 255); // Alice blue
                    bool isAlternate = false;

                    foreach (var treatment in treatments)
                    {
                        var rowColor = isAlternate ? lightBlue : BaseColor.White;

                        table.AddCell(new PdfPCell(new Phrase(treatment.TreatmentDate.ToString("MMM dd, yyyy\nHH:mm"), bodyFont))
                        {
                            BackgroundColor = rowColor,
                            Padding = 6,
                            VerticalAlignment = Element.ALIGN_MIDDLE
                        });

                        table.AddCell(new PdfPCell(new Phrase($"{treatment.Patient?.FirstName} {treatment.Patient?.LastName}", bodyFont))
                        {
                            BackgroundColor = rowColor,
                            Padding = 6,
                            VerticalAlignment = Element.ALIGN_MIDDLE
                        });

                        table.AddCell(new PdfPCell(new Phrase(treatment.TreatmentType ?? "N/A", bodyFont))
                        {
                            BackgroundColor = rowColor,
                            Padding = 6,
                            VerticalAlignment = Element.ALIGN_MIDDLE
                        });

                        table.AddCell(new PdfPCell(new Phrase(treatment.User?.FullName ?? "Unknown", bodyFont))
                        {
                            BackgroundColor = rowColor,
                            Padding = 6,
                            VerticalAlignment = Element.ALIGN_MIDDLE
                        });

                        isAlternate = !isAlternate;
                    }

                    doc.Add(table);
                }
                else
                {
                    var noDataParagraph = new Paragraph("No active treatment records found.", titleFont)
                    {
                        Alignment = Element.ALIGN_CENTER,
                        SpacingBefore = 20,
                        SpacingAfter = 20
                    };
                    doc.Add(noDataParagraph);
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

                return File(ms.ToArray(), "application/pdf", $"TreatmentsReport_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
            }
        }





        public async Task<IActionResult> CreateTreatment(int patientId)
        {
            var patient = await _context.Patients.FindAsync(patientId);
            if (patient == null)
                return NotFound();

            ViewBag.PatientName = $"{patient.FirstName} {patient.LastName}";
            ViewBag.PatientId = patientId;
            
            

            var visits = await _context.VisitSchedules.Where(v => v.PatientID == patientId).ToListAsync();
            ViewBag.VisitList = new SelectList(visits, "VisitID", "VisitDate");

            ViewBag.TreatmentTypes = new List<SelectListItem>
            {
                new SelectListItem { Text = "Wound Dressing", Value = "Wound Dressing" },
                new SelectListItem { Text = "IV Drip", Value = "IV Drip" },
                new SelectListItem { Text = "Catheter Change", Value = "Catheter Change" },
                new SelectListItem { Text = "Physiotherapy", Value = "Physiotherapy" },
                new SelectListItem { Text = "Other", Value = "Other" }
            };

            return View(new Treatment { PatientID = patientId });
        }


        // POST: CreateTreatment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTreatment(Treatment treatment)
        {
            // Handle custom treatment type
            if (treatment.TreatmentType == "Other")
            {
                var otherTreatment = Request.Form["TreatmentTypeOther"].ToString();
                if (!string.IsNullOrWhiteSpace(otherTreatment))
                    treatment.TreatmentType = otherTreatment;
                else
                    ModelState.AddModelError("TreatmentTypeOther", "Please specify the treatment type.");
            }
            


            if (!ModelState.IsValid)
            {
                // Reload necessary data to redisplay the form
                var patient = await _context.Patients.FindAsync(treatment.PatientID);
                if (patient == null) return NotFound();

                ViewBag.PatientName = $"{patient.FirstName} {patient.LastName}";
                ViewBag.PatientId = treatment.PatientID;

                var visits = await _context.VisitSchedules.Where(v => v.PatientID == treatment.PatientID).ToListAsync();
                ViewBag.VisitList = new SelectList(visits, "VisitID", "VisitDate", treatment.VisitID);

                ViewBag.TreatmentTypes = new List<SelectListItem>
                {
                    new SelectListItem { Text = "Wound Dressing", Value = "Wound Dressing" },
                    new SelectListItem { Text = "IV Drip", Value = "IV Drip" },
                    new SelectListItem { Text = "Catheter Change", Value = "Catheter Change" },
                    new SelectListItem { Text = "Physiotherapy", Value = "Physiotherapy" },
                    new SelectListItem { Text = "Other", Value = "Other" }
                };

                return View(treatment);
            }

            // Validate patient exists
            var existingPatient = await _context.Patients.FindAsync(treatment.PatientID);
            if (existingPatient == null)
                return NotFound("Patient not found.");

            // Get current logged-in user ID
            var userIdStr = _userManager.GetUserId(User);
            if (!int.TryParse(userIdStr, out int userId))
                return Unauthorized();

            treatment.ApplicationUserID = userId;
            treatment.TreatmentDate = DateTime.Now;
            treatment.IsActive = true;

            _context.Treatments.Add(treatment);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Treatment successfully performed!";
            return RedirectToAction("Treatments", new { PatientId = treatment.PatientID });
        }


        //editing treatment
        

        public async Task<IActionResult> EditTreatment(int id)
        {
            var treatment = await _context.Treatments.Include(t => t.Patient).FirstOrDefaultAsync(t => t.TreatmentID == id);

            if (treatment == null)
            {
                return NotFound();
            }

            // Load visits for the patient to populate dropdown
            var visits = await _context.VisitSchedules.Where(v => v.PatientID == treatment.PatientID).ToListAsync();
            ViewBag.VisitList = new SelectList(visits, "VisitID", "VisitDate", treatment.VisitID);

            // Determine if this is a custom treatment type
            var standardOptions = new List<string> { "Wound Dressing", "IV Drip", "Catheter Change", "Physiotherapy", "Other" };
            var isCustomTreatment = !string.IsNullOrEmpty(treatment.TreatmentType) && !standardOptions.Contains(treatment.TreatmentType);

            // Create treatment types list
            var treatmentTypes = new List<SelectListItem>
            {
                new SelectListItem { Text = "Wound Dressing", Value = "Wound Dressing" },
                new SelectListItem { Text = "IV Drip", Value = "IV Drip" },
                new SelectListItem { Text = "Catheter Change", Value = "Catheter Change" },
                new SelectListItem { Text = "Physiotherapy", Value = "Physiotherapy" },
                new SelectListItem { Text = "Other", Value = "Other" }
            };

            // For custom treatments, we need to add the custom option to the dropdown
            if (isCustomTreatment)
            {
                // Add the custom treatment as an option and select it
                treatmentTypes.Add(new SelectListItem
                {
                    Text = $"{treatment.TreatmentType} (Custom)",
                    Value = treatment.TreatmentType,
                    Selected = true
                });

                // Also select "Other" in the standard options
                treatmentTypes.First(x => x.Value == "Other").Selected = true;
            }
            else
            {
                // Select the actual value for standard treatments
                treatmentTypes.ForEach(x => x.Selected = x.Value == treatment.TreatmentType);
            }

            ViewBag.TreatmentTypes = treatmentTypes;

            var nurseId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var nurse = await _context.Users.FindAsync(nurseId);
            ViewBag.NurseName = nurse?.FullName;

            // Pass whether it's a custom treatment to the view
            ViewBag.IsCustomTreatment = isCustomTreatment;

            return View(treatment);
        }

        

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTreatment(int id, Treatment model)
        {
            if (id != model.TreatmentID)
                return NotFound();

            // Handle the case where a custom value is selected from the dropdown
            var standardOptions = new List<string> { "Wound Dressing", "IV Drip", "Catheter Change", "Physiotherapy", "Other" };
            var isCustomValueSelected = !standardOptions.Contains(model.TreatmentType);

            if (isCustomValueSelected)
            {
                // The user selected a custom value from the dropdown, use it directly
                // No need to look for TreatmentTypeOther
            }
            else if (model.TreatmentType == "Other")
            {
                // Handle custom "Other" treatment type from the input field
                var otherTreatment = Request.Form["TreatmentTypeOther"].ToString();
                if (!string.IsNullOrWhiteSpace(otherTreatment))
                {
                    model.TreatmentType = otherTreatment;
                }
                else
                {
                    ModelState.AddModelError("TreatmentTypeOther", "Please specify the treatment type.");
                }
            }

            if (!ModelState.IsValid)
            {
                // Reload dropdowns if invalid
                var visits = await _context.VisitSchedules.Where(v => v.PatientID == model.PatientID).ToListAsync();
                ViewBag.VisitList = new SelectList(visits, "VisitID", "VisitDate", model.VisitID);

                // Recreate treatment types list for the view
                var treatmentTypes = new List<SelectListItem>
                {
                    new SelectListItem { Text = "Wound Dressing", Value = "Wound Dressing" },
                    new SelectListItem { Text = "IV Drip", Value = "IV Drip" },
                    new SelectListItem { Text = "Catheter Change", Value = "Catheter Change" },
                    new SelectListItem { Text = "Physiotherapy", Value = "Physiotherapy" },
                    new SelectListItem { Text = "Other", Value = "Other" }
                };

                // If we have a custom value, add it to the dropdown
                if (!standardOptions.Contains(model.TreatmentType))
                {
                    treatmentTypes.Add(new SelectListItem
                    {
                        Text = $"{model.TreatmentType} (Custom)",
                        Value = model.TreatmentType,
                        Selected = true
                    });
                    treatmentTypes.First(x => x.Value == "Other").Selected = true;
                }
                else
                {
                    treatmentTypes.ForEach(x => x.Selected = x.Value == model.TreatmentType);
                }

                ViewBag.TreatmentTypes = treatmentTypes;
                ViewBag.IsCustomTreatment = !standardOptions.Contains(model.TreatmentType);

                return View(model);
            }

            try
            {
                // Attach existing treatment and update fields
                var existingTreatment = await _context.Treatments.FindAsync(id);
                if (existingTreatment == null)
                    return NotFound();

                existingTreatment.VisitID = model.VisitID;
                existingTreatment.TreatmentType = model.TreatmentType;
                existingTreatment.TreatmentDate = model.TreatmentDate;

                // Keep other fields like PatientID, ApplicationUserID unchanged or adjust as needed
                model.ApplicationUserID = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                existingTreatment.ApplicationUserID = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);

                _context.Update(existingTreatment);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Treatment successfully updated!";
                return RedirectToAction("Treatments");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Treatments.Any(e => e.TreatmentID == id))
                    return NotFound();

                throw;
            }
        }

        public async Task<IActionResult> DeleteTreatment(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var treatment = await _context.Treatments.Include(p => p.Patient).Include(t=>t.TreatVisit).Include(t=>t.VisitSchedule).Include(a => a.User).FirstOrDefaultAsync(t => t.TreatmentID == id );

            if (treatment == null)
            {
                return NotFound();
            }

            return View(treatment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTreatment(int id)
        {
            var treatment = await _context.Treatments.FindAsync(id);

            if (treatment != null)
            {
                treatment.IsActive = false;
                _context.Treatments.Update(treatment);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Treatment successfully deleted!";
            }

            //return RedirectToAction(nameof(Treatments));\
            return Ok();

        }











































        //administer medication
        
        public async Task<IActionResult> Administered(string searchPatient, DateTime? fromDate, DateTime? toDate, string sortOrder)
        {
            var medsQuery = _context.PatientMedicationScripts
                .Include(p => p.Patient)
                .Include(v => v.VisitSchedule)
                .Include(p => p.Prescription)
                .Include(a => a.AdministeredBy)
                .Include(m => m.Medication)/*.Where(m=>m.Medication.Schedule <= 4)*/
                .Where(m => m.isActive);

            ViewData["NameSortParm"] = System.String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";


            if (!string.IsNullOrEmpty(searchPatient))
            {
                medsQuery = medsQuery.Where(m =>
                    m.Patient.FirstName.Contains(searchPatient) ||
                    m.Patient.LastName.Contains(searchPatient));
            }

            if (fromDate.HasValue)
            {
                medsQuery = medsQuery.Where(m => m.AdministeredDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                var endOfDay = toDate.Value.Date.AddDays(1).AddTicks(-1);
                medsQuery = medsQuery.Where(m => m.AdministeredDate <= endOfDay);
            }

            var medications = await medsQuery.ToListAsync();
            // Apply sorting based on the sortOrder parameter
            switch (sortOrder)
            {
                case "name_desc":
                    medications = medications.OrderByDescending(m => m.Patient.FirstName).ToList();
                    break;
                case "Date":
                    medications = medications.OrderBy(m => m.AdministeredDate).ToList();
                    break;
                case "date_desc":
                    medications = medications.OrderByDescending(m => m.AdministeredDate).ToList();
                    break;
                default:
                    medications = medications.OrderBy(m => m.Patient.FirstName).ToList(); // Default sort
                    break;
            }


            return View(medications);
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


        [HttpGet]
        public async Task<IActionResult> CreateAdminister(int patientId)
        {
            
            var patient = await _context.Patients.Include(p => p.PatientAllergies).ThenInclude(pa => pa.Allergy).FirstOrDefaultAsync(p => p.PatientID == patientId);
            if (patient == null)
            {
                return NotFound();
            }

          

            ViewBag.PatientId = patient.PatientID;
            ViewBag.PatientName = $"{patient.FirstName} {patient.LastName}";
            ViewBag.PatientAllergies = patient.PatientAllergies.Select(pa => pa.Allergy.Name).ToList();
            
            ViewBag.MedicationList = _context.Medications.ToList();
            ViewBag.UserList = new SelectList(_context.Users.ToList(), "ApplicationUserID", "FullName");
            return View();
        }
        

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAdminister(PatientMedicationScript model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                model.ApplicationUserID = user.Id; 
            }
            // Load the selected medication to check its schedule
            var medication = await _context.Medications.FirstOrDefaultAsync(m => m.MedicationId == model.MedicationId);
            var patient = await _context.Patients.Include(p => p.PatientAllergies).ThenInclude(pa => pa.Allergy).FirstOrDefaultAsync(p => p.PatientID == model.PatientId);
            var patientAllergies = patient?.PatientAllergies.Select(pa => pa.Allergy.Name).ToList() ?? new List<string>();


            if (medication == null)
            {
                ModelState.AddModelError("", "Invalid medication selected.");
            }
            else if (medication.Schedule > 4)
            {
                ModelState.AddModelError("", "You cannot administer medication with a schedule higher than 4.");
            }
            else if (patientAllergies.Any(a => medication.Name.Contains(a, StringComparison.OrdinalIgnoreCase)))
                ModelState.AddModelError("", "⚠ This patient is allergic to the selected medication!");


            if (!ModelState.IsValid)
            {
                // Re-populate dropdowns when returning view
                
                ViewBag.PatientId = patient?.PatientID;
                ViewBag.PatientName = $"{patient?.FirstName} {patient?.LastName}";
                ViewBag.MedicationList = _context.Medications.ToList();
                ViewBag.UserList = new SelectList(_context.Users, "ApplicationUserID", "FullName", model.ApplicationUserID);
                ViewBag.PatientAllergies = patientAllergies;

                return View(model);
            }

            try
            {
                if (medication.Quantity > 0)
                {
                    medication.Quantity -= 1; // Assuming one unit per administration
                }
                else
                {
                    ModelState.AddModelError("", "This Medication is out of stock!");
                    ViewBag.PatientId = patient?.PatientID;
                    ViewBag.PatientName = $"{patient?.FirstName} {patient?.LastName}";
                    ViewBag.MedicationList = _context.Medications.ToList();
                    ViewBag.UserList = new SelectList(_context.Users, "ApplicationUserID", "FullName", model.ApplicationUserID);
                    ViewBag.PatientAllergies = patientAllergies;
                    return View(model);
                }

                _context.PatientMedicationScripts.Add(model);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Medication administered successfully!";
                return RedirectToAction(nameof(Administered));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "An error has occurred.");
            }

            // In case of error, reload dropdowns
            var patientReload = await _context.Patients.FindAsync(model.PatientId);
            
            ViewBag.PatientId = patient?.PatientID;
            ViewBag.PatientName = $"{patient?.FirstName} {patient?.LastName}";
            ViewBag.MedicationList = _context.Medications.ToList();
            ViewBag.UserList = new SelectList(_context.Users, "ApplicationUserID", "FullName", model.ApplicationUserID);
            ViewBag.PatientAllergies = patientAllergies;
            return View(model);
        }


        



        public async Task<IActionResult> EditAdministered(int? id)
        {
            if (id == null)
                return NotFound();

            var medication = await _context.PatientMedicationScripts.Include(p => p.Patient).Include(m => m.Medication).Include(a => a.AdministeredBy).Include(v => v.VisitSchedule).FirstOrDefaultAsync(m => m.Id == id);

            
            if (medication == null)
                return NotFound();
            if (medication.PrescriptionId != null)
            {
                TempData["ErrorMessage"] = "You cannot edit prescribed medication.";
                return RedirectToAction("ListAdministered");
            }

            ViewBag.MedicationList = _context.Medications.ToList();
            ViewBag.UserList = new SelectList(_context.Users, "ApplicationUserID", "FullName", medication.ApplicationUserID);
            return View(medication);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAdministered(int id, PatientMedicationScript model)
        {
            if (id != model.Id)
                return NotFound();

            var existing = await _context.PatientMedicationScripts.FindAsync(id);
            if (existing == null)
                return NotFound();

            var medication = await _context.Medications.FirstOrDefaultAsync(m => m.MedicationId == model.MedicationId);

            if (medication == null)
            {
                ModelState.AddModelError("", "Invalid medication selected.");
            }
            else if (medication.Schedule > 4)
            {
                ModelState.AddModelError("", "You cannot administer medication with a schedule higher than 4.");
            }

            if (!ModelState.IsValid)
            {
                // Re-populate dropdowns
                var allowedMedications = _context.Medications.Where(m => m.Schedule <= 4).ToList();
                ViewBag.MedicationList = new SelectList(_context.Medications.ToList(), "MedicationId", "Name", model.MedicationId);
                return View(model);
            }

            // **CRITICAL: Check if schedule changed from high to low**
            var originalMedication = await _context.Medications.FirstOrDefaultAsync(m => m.MedicationId == existing.MedicationId);
            bool wasHighSchedule = originalMedication?.Schedule > 4;
            bool isNowLowSchedule = medication.Schedule <= 4;

            // If changing from high schedule to low schedule, remove prescription
            if (wasHighSchedule && isNowLowSchedule)
            {
                existing.PrescriptionId = null; // Remove prescription link
                Console.WriteLine($"Removed prescription link for medication change from Schedule {originalMedication.Schedule} to Schedule {medication.Schedule}");
            }

            // Update only editable fields
            existing.MedicationId = model.MedicationId;
            
            existing.ApplicationUserID = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);

            existing.AdministeredDate = model.AdministeredDate;
            existing.Dosage = model.Dosage;
            
            

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Medication successfully updated!";
            return RedirectToAction(nameof(Administered));
        }


        public async Task<IActionResult> DeleteAdministered(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var medication = await _context.PatientMedicationScripts.Include(p => p.Patient).Include(v => v.VisitSchedule).Include(a => a.AdministeredBy).Include(m => m.Medication).FirstOrDefaultAsync(a=>a.Id == id);
            if (medication == null)
            {
                return NotFound();
            }
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
            return RedirectToAction(nameof(Administered));
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












        
        public async Task<IActionResult> InstructionList(string searchPatient, string statusFilter, DateTime? fromDate, DateTime? toDate, string sortOrder)
        {
            var instructionsQuery = _context.Instructions
                .Include(i => i.Patient)
                .Where(i => i.isActive);

            // Set sorting parameters
            ViewData["NameSortParm"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";

            // Search by patient name
            if (!string.IsNullOrEmpty(searchPatient))
            {
                instructionsQuery = instructionsQuery.Where(i =>
                    i.Patient.FirstName.Contains(searchPatient) ||
                    i.Patient.LastName.Contains(searchPatient));
            }

            // Status filter - FIXED
            if (!string.IsNullOrEmpty(statusFilter))
            {
                if (statusFilter == "pending")
                {
                    instructionsQuery = instructionsQuery.Where(i => string.IsNullOrEmpty(i.Instructions));
                }
                else if (statusFilter == "reviewed")
                {
                    instructionsQuery = instructionsQuery.Where(i => !string.IsNullOrEmpty(i.Instructions));
                }
            }

           

            var instructions = await instructionsQuery.OrderByDescending(i => i.Patient.FirstName).ToListAsync();
            return View(instructions);
        }


        
        public async Task<IActionResult> ViewAdvice(int patientId)
        {
            var patient = await _context.Patients.FindAsync(patientId);
            if (patient == null)
                return NotFound();

            // MARK INSTRUCTIONS AS READ when viewing advice
            var unreadInstructions = await _context.Instructions
                .Where(i => i.PatientID == patientId && !i.IsRead && i.isActive)
                .ToListAsync();

            Console.WriteLine($"Found {unreadInstructions.Count} unread instructions for patient {patientId}");

            // FIXED: Using 'unreadInstruction' as loop variable to avoid conflict
            foreach (var unreadInstruction in unreadInstructions)
            {
                unreadInstruction.IsRead = true;
                unreadInstruction.ReadAt = DateTime.Now;
                Console.WriteLine($"Marked instruction {unreadInstruction.InstructionID} as read");
            }

            await _context.SaveChangesAsync();

           

            // FIXED: Using 'latestInstruction' instead of 'instruction' to avoid conflict
            var latestInstruction = await _context.Instructions
                .Include(i => i.Patient)
                .Where(i => i.PatientID == patientId && i.isActive)
                .OrderByDescending(i => i.InstructionID)
                .FirstOrDefaultAsync();

            if (latestInstruction == null)
            {
                // Create a temporary instruction object with fallback text
                latestInstruction = new Instruction
                {
                    Patient = patient,
                    Instructions = "No advice from doctor",
                    IsRead = false
                   
                };
            }
            else
            {
                Console.WriteLine($"Using instruction ID: {latestInstruction.InstructionID} with content: {latestInstruction.Instructions}, IsRead: {latestInstruction.IsRead}");
            }

            return View(latestInstruction);
        }

        public async Task<IActionResult> DoctorResponse(int id)
        {
            var instruction = await _context.Instructions.Include(i => i.Patient).FirstOrDefaultAsync(/*i => i.InstructionID == id*/);

            if (instruction == null || string.IsNullOrEmpty(instruction.Instructions))
            {
                return NotFound();
            }

            return View(instruction);
        }



        [HttpGet]
        public async Task<IActionResult> CreateRequest(int patientId)
        {
            var patient = await _context.Patients.FindAsync(patientId);
            if (patient == null)
            {
                return NotFound();
            }

            var viewModel = new RequestInstructionViewModel
            {
                PatientID = patient.PatientID,
                PatientName = patient.FirstName + " " + patient.LastName
            };
            return View(viewModel);
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRequest(RequestInstructionViewModel viewModel)
        {
            //var userId = _userManager.GetUserId(User);
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }
            var userId = _userManager.GetUserId(User);

            var request = new Instruction
            {
                PatientID = viewModel.PatientID,
                NurseRequest = viewModel.Message, 
                DateRecorded = DateTime.Now,
                ApplicationUserID = int.Parse(userId),
                IsRead = true
            };

            _context.Instructions.Add(request);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Advice successfully requested!";
            return RedirectToAction("InstructionList");
        }

        public async Task<IActionResult> EditRequest(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }
            var request = await _context.Instructions.Include(i => i.Patient).FirstOrDefaultAsync(i => i.InstructionID == id);
            if (request == null)
            {
                return NotFound();
            }
            if (!string.IsNullOrEmpty(request.Instructions))
            {
                TempData["ErrorMessage"] = "Cannot edit reviewed instructions.";
                return RedirectToAction("InstructionList");
            }

            return View(request);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRequest(int? id, Instruction request)
        {
            if(id != request.InstructionID)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    var existingRequest = await _context.Instructions.FindAsync(id);
                    if(existingRequest == null)
                    {
                        return NotFound();
                    }
                    existingRequest.NurseRequest = request.NurseRequest;
                    _context.Update(existingRequest);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Request successfully updated!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Instructions.Any(e => e.InstructionID == id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(InstructionList)); 
            }
            return View(request);
        }

        public async Task<IActionResult> DeleteRequest(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }
            var request = await _context.Instructions.Include(p => p.Patient).FirstOrDefaultAsync(p => p.InstructionID == id);
            if(request == null)
            {
                return NotFound();
            }
            return View(request);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>DeleteRequest(int id)
        {
            var request = await _context.Instructions.FindAsync(id);
            if(request == null)
            {
                return NotFound();
            }
            _context.Instructions.Remove(request);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Request Successfully Deleted!";
            return RedirectToAction(nameof(InstructionList));
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





        public IActionResult LiveSearchInstructions(string query)
        {
            var results = _context.Instructions
                .Include(i => i.Patient)
                .Include(i => i.User)
                .Where(i => i.Patient.FirstName.Contains(query) ||
                           i.Patient.LastName.Contains(query) ||
                           i.NurseRequest.Contains(query))
                .ToList();

            return PartialView("_InstructionSearchResultsPartial", results);
        }

        public IActionResult LiveSearchAllInstructions()
        {
            var allInstructions = _context.Instructions
                .Include(i => i.Patient)
                .Include(i => i.User)
                .ToList();

            return PartialView("_InstructionSearchResultsPartial", allInstructions);
        }

        public async Task<IActionResult> Progress(int? patientId, DateTime? fromDate, DateTime? toDate)
        {

            var patients = await _context.Patients.Where(p => !p.IsDeleted).OrderBy(p => p.FirstName).ToListAsync();

            if (!patients.Any())
            {
                return View(new ProgressViewModel
                {
                    PatientsList = new SelectList(new List<Patient>(), "PatientID", "FirstName"),
                });
            }

            //patient selection
            int selectedPatientId;
            if (patientId.HasValue && patients.Any(p => p.PatientID == patientId.Value))
            {
                selectedPatientId = patientId.Value;
            }
            else
            {
                selectedPatientId = patients.First().PatientID;
            }

            var vm = new ProgressViewModel
            {
                SelectedPatientId = selectedPatientId,
                PatientsList = new SelectList(patients, "PatientID", "FirstName", selectedPatientId),
                FromDate = fromDate,
                ToDate = toDate,
                CompanyName = "HEALTHCARE FACILITY LTD",
                ReportTitle = "Patient Progress Report",
                GeneratedDate = DateTime.Now,
                GeneratedBy = User.Identity?.Name ?? "System",
                CompanyAddress = "123 Medical Center, Healthcare City, 10001",
                CompanyContact = "Tel: (555) 123-4567 | Email: info@healthcarefacility.com"
            };

            // Build report period string
            if (fromDate.HasValue && toDate.HasValue)
            {
                vm.ReportPeriod = $"{fromDate.Value:yyyy-MM-dd} to {toDate.Value:yyyy-MM-dd}";
            }
            else if (fromDate.HasValue)
            {
                vm.ReportPeriod = $"From {fromDate.Value:yyyy-MM-dd}";
            }
            else if (toDate.HasValue)
            {
                vm.ReportPeriod = $"Until {toDate.Value:yyyy-MM-dd}";
            }
            else
            {
                vm.ReportPeriod = "All Time";
            }

            var patient = patients.FirstOrDefault(p => p.PatientID == selectedPatientId);
            vm.PatientName = patient != null ? $"{patient.FirstName} {patient.LastName}" : "Unknown";


            // Vitals (filter by patient and optional date range)
            var vitalsQuery = _context.Vitals
                .Include(v => v.Patient)
                .Include(v => v.User)
                .Where(v => v.PatientID == selectedPatientId && v.IsActive);

            if (fromDate.HasValue) vitalsQuery = vitalsQuery.Where(v => v.Date >= fromDate.Value);
            if (toDate.HasValue) vitalsQuery = vitalsQuery.Where(v => v.Date <= toDate.Value);

            var vitals = await vitalsQuery.OrderBy(v => v.Date).ToListAsync();
            vm.Vitals = vitals;


            // Medications (filter by patient and optional date range)
            var medsQuery = _context.PatientMedicationScripts
                .Include(m => m.Medication)
                .Include(m => m.Patient)
                .Include(m => m.AdministeredBy)
                .Where(m => m.PatientId == selectedPatientId && m.isActive);

            if (fromDate.HasValue) medsQuery = medsQuery.Where(m => m.AdministeredDate >= fromDate.Value);
            if (toDate.HasValue) medsQuery = medsQuery.Where(m => m.AdministeredDate <= toDate.Value);

            var meds = await medsQuery.OrderBy(m => m.AdministeredDate).ToListAsync();
            vm.Medications = meds;

            var treatmentsQuery = _context.Treatments
        .Include(t => t.Patient)
        .Where(t => t.PatientID == selectedPatientId & t.IsActive);


            if (fromDate.HasValue) treatmentsQuery = treatmentsQuery.Where(t => t.TreatmentDate >= fromDate.Value);
            if (toDate.HasValue) treatmentsQuery = treatmentsQuery.Where(t => t.TreatmentDate <= toDate.Value);

            var treatments = await treatmentsQuery.OrderBy(t => t.TreatmentDate).ToListAsync();
            vm.Treatments = treatments;


            // Build combined chronological label list 
            var allDates = vitals.Select(v => v.Date)
                .Union(meds.Select(m => m.AdministeredDate))
                .Union(treatments.Select(t => t.TreatmentDate))
                .Where(d => d != default(DateTime))
                .Distinct()
                .OrderBy(d => d)
                .ToList();

            var labels = allDates.Select(d => d.ToString("yyyy-MM-dd HH:mm")).ToList();

            // prepare arrays aligned with labels
            int n = labels.Count;
            var bpArr = Enumerable.Repeat<double?>(null, n).ToArray();
            var pulseArr = Enumerable.Repeat<double?>(null, n).ToArray();
            var sugarArr = Enumerable.Repeat<double?>(null, n).ToArray();
            var tempArr = Enumerable.Repeat<double?>(null, n).ToArray();

            // helper: label -> index
            var labelIndex = labels
                .Select((lbl, idx) => new { lbl, idx })
                .ToDictionary(a => a.lbl, a => a.idx);


            // fill vitals
            foreach (var v in vitals)
            {
                var key = v.Date.ToString("yyyy-MM-dd HH:mm");
                if (!labelIndex.ContainsKey(key)) continue;
                var idx = labelIndex[key];

                // parse systolic from "120/80" if possible
                if (!string.IsNullOrWhiteSpace(v.BP))
                {
                    var parts = v.BP.Split('/');
                    if (parts.Length > 0 && int.TryParse(parts[0].Trim(), out var s))
                        bpArr[idx] = s;
                }

                if (v.PulseRate != null) { double parsed; if (double.TryParse(v.PulseRate.ToString(), out parsed)) pulseArr[idx] = parsed; }
                if (v.SugarLevel != null) { double parsed; if (double.TryParse(v.SugarLevel.ToString(), out parsed)) sugarArr[idx] = parsed; }
                if (v.Temperature != null) { double parsed; if (double.TryParse(v.Temperature.ToString(), out parsed)) tempArr[idx] = parsed; }
            }

            // prepare med markers and tooltips
            var medData = Enumerable.Repeat<double?>(null, n).ToArray();
            var medTooltips = new List<List<string>>(Enumerable.Repeat<List<string>>(null, n));

            for (int i = 0; i < n; i++) medTooltips[i] = null;

            foreach (var m in meds)
            {
                var key = m.AdministeredDate.ToString("yyyy-MM-dd HH:mm");
                if (!labelIndex.ContainsKey(key)) continue;
                var idx = labelIndex[key];


                if (medTooltips[idx] == null) medTooltips[idx] = new List<string>();
                medTooltips[idx].Add($"{m.Medication?.Name ?? "Medication"} ({m.Dosage ?? ""})");
            }

            // treatments markers & tooltips
            var treatData = Enumerable.Repeat<double?>(null, n).ToArray();
            var treatTooltips = new List<List<string>>(Enumerable.Repeat<List<string>>(null, n));
            for (int i = 0; i < n; i++) treatTooltips[i] = null;

            foreach (var t in treatments)
            {
                var key = t.TreatmentDate.ToString("yyyy-MM-dd HH:mm");
                if (!labelIndex.ContainsKey(key)) continue;
                var idx = labelIndex[key];

                if (treatTooltips[idx] == null) treatTooltips[idx] = new List<string>();

                treatTooltips[idx].Add($"{t?.TreatmentType ?? "Treatment"}");
            }

            // determine a marker Y 
            var numericValues = new List<double>();
            if (bpArr.Any(x => x.HasValue)) numericValues.Add(bpArr.Where(x => x.HasValue).Max().Value);
            if (pulseArr.Any(x => x.HasValue)) numericValues.Add(pulseArr.Where(x => x.HasValue).Max().Value);
            if (sugarArr.Any(x => x.HasValue)) numericValues.Add(sugarArr.Where(x => x.HasValue).Max().Value);
            if (tempArr.Any(x => x.HasValue)) numericValues.Add(tempArr.Where(x => x.HasValue).Max().Value);

            var maxVal = numericValues.Any() ? numericValues.Max() : 100.0;
            var markerY = maxVal + Math.Max(5, maxVal * 0.08); // put markers a bit above the max

            // place numeric marker value where med/treatment exist
            for (int i = 0; i < n; i++)
            {
                if (medTooltips[i] != null && medTooltips[i].Any()) medData[i] = markerY;
                if (treatTooltips[i] != null && treatTooltips[i].Any()) treatData[i] = markerY + (maxVal * 0.03);
            }

            // serialize to JSON for the view
            vm.ChartLabelsJson = JsonConvert.SerializeObject(labels);
            vm.BpJson = JsonConvert.SerializeObject(bpArr.Select(x => x.HasValue ? x.Value : (double?)null));
            vm.PulseJson = JsonConvert.SerializeObject(pulseArr.Select(x => x.HasValue ? x.Value : (double?)null));
            vm.SugarJson = JsonConvert.SerializeObject(sugarArr.Select(x => x.HasValue ? x.Value : (double?)null));
            vm.TempJson = JsonConvert.SerializeObject(tempArr.Select(x => x.HasValue ? x.Value : (double?)null));

            vm.MedDataJson = JsonConvert.SerializeObject(medData.Select(x => x.HasValue ? x.Value : (double?)null));
            vm.MedTooltipsJson = JsonConvert.SerializeObject(medTooltips.Select(list => list == null ? null : list.ToArray()));

            vm.TreatmentDataJson = JsonConvert.SerializeObject(treatData.Select(x => x.HasValue ? x.Value : (double?)null));
            vm.TreatmentTooltipsJson = JsonConvert.SerializeObject(treatTooltips.Select(list => list == null ? null : list.ToArray()));

            return View(vm);

        }




    }
}
