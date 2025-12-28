using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ONT_3rdyear_Project.Data;
using ONT_3rdyear_Project.Models;
using ONT_3rdyear_Project.ViewModels;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
namespace ONT_3rdyear_Project.Controllers
{
    public class ScriptController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;


        public ScriptController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole<int>> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult Dashboard()
        {
           ViewBag.NewPre = _context.Prescriptions.Where(s => s.Status == "New").Count();
           ViewBag.Preocessing = _context.Prescriptions.Where(s => s.Status == "Processing").Count();
           ViewBag.Rejected = _context.Prescriptions.Where(s => s.Status == "Rejected").Count();
           ViewBag.Forward = _context.Prescriptions.Where(s => s.Status == "Forwarded").Count();
           ViewBag.Delivered = _context.Prescriptions.Where(s => s.Status == "Approved").Count();
           ViewBag.Wards = new SelectList(_context.Wards.ToList(), "WardID", "Name");
            var scripts = _context.Prescriptions                
                .Include(s => s.Patient)
                .Include(s => s.User)
                .Include(s => s.Prescribed_Medication)
                .ToList();
            return View(scripts);            
        }
       
        //Get: Prescription info Modal
       [HttpGet]
        public async Task<IActionResult> PatientInfoModal(int id)
        {
            var script = await _context.Prescriptions
                .Where(s => s.PrescriptionId == id)
                .Include(s => s.Patient)
                .Include(s => s.User)
                .Include(s => s.Prescribed_Medication)
                .Select(s => new ViewScriptsViewModel
                {
                    PrescriptionId = s.PrescriptionId,
                    PatientID = s.PatientId,
                    PatientName = s.Patient.FirstName + " " + s.Patient.LastName,
                    DoctorName = s.User.FullName,
                    Schedule = s.Prescribed_Medication.Where(p => p.PrescriptionId == s.PrescriptionId).Select(pm => pm.Medication.Schedule).ToList(),
                    Ward = _context.Admissions.Where(w => w.PatientID == s.PatientId).Select(w => w.Ward.Name).FirstOrDefault(),
                    DateCreated = s.DateIssued.ToString("dd/MM/yyyy"),
                    Status =  _context.Prescriptions.Where(p => p.PrescriptionId == s.PrescriptionId).Select(s => s.Status).FirstOrDefault(),
                    medications = s.Prescribed_Medication.Where(p => p.PrescriptionId == s.PrescriptionId).Select(pm => pm.Medication.Name).ToList(),
                    Dosage = s.Prescribed_Medication.Select(pm => pm.Dosage).ToList()
                })
                .FirstOrDefaultAsync();

            return PartialView("PatientInfoModal", script);
        }
           // Quick actions controller
        [HttpPost]
        public async Task<IActionResult> QuickActions(int type)
        {
            var user = await _userManager.GetUserAsync(User);
            //Process all
            if (type == 1)
            {
                foreach (var p in _context.Prescriptions.Where(p => p.Status == "New").ToList())
                {
                    p.Status = "Processing";
                }
            }
            else if (type == 2)
            {

                // Forward all "Processing" prescriptions
                var processingPrescriptions = _context.Prescriptions
                    .Where(p => p.Status == "Processing")
                    .ToList();

                foreach (var p in processingPrescriptions)
                {
                    p.Status = "Approved";

                    var forwarding = new PrescriptionForwarding
                    {
                        PrescriptionID = p.PrescriptionId,
                        EmployeeID = user.Id,
                        ForwardedDate = DateTime.Now
                    };

                    _context.PrescriptionForwardings.Add(forwarding);
                }

                TempData["Message"] = "All Prescriptions forwarded successfully.";
            }
            else { TempData["Message"] = "No Prescriptions to Process or Forward."; }

            await _context.SaveChangesAsync();
            return RedirectToAction("Dashboard");
        }
         //The Status Handling
        [HttpPost]
        public async Task<IActionResult> StatusHandling(int id, int value, string reason)
        {
            var prescription = await _context.Prescriptions.FindAsync(id);
            if (prescription == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);
            // var prescription = _context.Prescriptions.FirstOrDefault(p => p.PrescriptionId == id);
            //if (prescription == null) return NotFound();
            if (value == 1)
            {
                prescription.Status = "Processing";
                TempData["Message"] = "Prescription Processing.";
               
            }
            else if (value == 2 && prescription.Status == "Processing")
            {
                prescription.Status = "Approved";
                var forwarding = new PrescriptionForwarding
                {
                    PrescriptionID = prescription.PrescriptionId,
                    EmployeeID = user.Id,
                    ForwardedDate = DateTime.UtcNow
                };
                TempData["Message"] = "Prescription forwarded successfully.";
                _context.PrescriptionForwardings.Add(forwarding);
                Json(new { success = true });
                
            }
            else if (value == 3 && prescription.Status == "Processing")
            {
                prescription.Status = "Rejected";
                var rejection = new PrescriptionRejection
                {
                    PrescriptionID = prescription.PrescriptionId,
                    ApplicationUserID = user.Id,
                    RejectionDate = DateTime.UtcNow,
                    RejectionReason = reason
                };
                TempData["Message"] = "Prescription Rejected.";
                _context.PrescriptionRejections.Add(rejection);
                
            }
            else { TempData["Message"] = "Prescription has to be processed before any actions."; return RedirectToAction("Dashboard"); }

            _context.SaveChanges();
            return RedirectToAction("Dashboard");
        }
        //Generate reports
            public async Task<IActionResult> GenerateReports()
            {
                var scripts = await _context.Prescriptions                       
                        .Include(s => s.Patient)
                        .Include(s => s.User)
                        .Include(s => s.Prescribed_Medication)
                        .Select(s => new ViewScriptsViewModel
                        {
                            PrescriptionId = s.PrescriptionId,
                            PatientID = s.PatientId,
                            PatientName = s.Patient.FirstName + " " + s.Patient.LastName,
                            DoctorName = s.User.FullName,
                            Schedule = s.Prescribed_Medication.Select(pm => pm.Medication.Schedule).ToList(),
                            Ward = _context.Admissions.Where(w => w.PatientID == s.PatientId).Select(w => w.Ward.Name).FirstOrDefault(),
                            DateCreated = s.DateIssued.ToString("dd/MM/yyyy"),
                            Status =  s.Status,
                            medications = s.Prescribed_Medication.Select(pm => pm.Medication.Name).ToList(),
                            Dosage = s.Prescribed_Medication.Select(pm => pm.Dosage).ToList()
                        }).ToListAsync();

                var pdf = Document.Create(container => {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);

                        // Professional Header with Logo
                        page.Header()
                            .BorderBottom(2)
                            .BorderColor("#2C3E50")
                            .PaddingBottom(10)
                            .Row(row =>
                            {
                                // Logo section (optional - only if file exists)
                              

                                // Title section
                                row.RelativeItem()
                                    .Column(column =>
                                    {
                                        column.Item().Text("Prescription Report")
                                            .FontSize(24)
                                            .Bold()
                                            .FontColor("#2C3E50");

                                        column.Item().PaddingTop(5).Text($"Generated: {DateTime.Now:MMMM dd, yyyy}")
                                            .FontSize(10)
                                            .FontColor("#7F8C8D");

                                        column.Item().Text($"Prescription ID: {scripts.Count}")
                                            .FontSize(10)
                                            .FontColor("#7F8C8D");
                                    });
                            });


                    page.Content().PaddingVertical(10).Column(col =>
                    {
                        foreach (var p in scripts.OrderByDescending(m => m.Status).ThenByDescending(m => m.DateCreated))
                        {
                            col.Item().Background(Colors.Grey.Lighten3).Padding(8).Row(row =>
                            {
                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Text($"Patient: {p.PrescriptionId}").Bold();
                                    col.Item().Text($"Patient: {p.PatientName}").Bold();
                                    col.Item().Text($"Doctor: {p.DoctorName}");
                                    col.Item().Text($"Ward: {p.Ward}");
                                    col.Item().Text($"Date: {p.DateCreated}");
                                });

                                row.ConstantItem(150).AlignRight().Text($"Status: {p.Status}").Bold();
                            });

                            // Space between cards
                            col.Item().PaddingBottom(15);
                        }
                    });

                    // Footer with page numbers
                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                        x.Span(" of ");
                        x.TotalPages();
                    });
                });
            });
                QuestPDF.Settings.License = LicenseType.Community;
                var pdfBytes = pdf.GeneratePdf();
                if (pdfBytes == null || pdfBytes.Length == 0)
                {
                   TempData["Error"] = "No Reports generate .";
                    return RedirectToAction("Dashboard");
                }                 
                 TempData["Message"] = "Report generated successfully.";
                return  File(pdfBytes, "application/pdf", "PrescriptionReport.pdf");
           
               
            }
            //Generate reports for a specific patient
           public async Task<IActionResult> GenerateReport(int id)
            {
               
                var script = await _context.Prescriptions
                    .Where(s => s.PrescriptionId == id)
                    .Include(s => s.Patient)
                    .Include(s => s.User)
                    .Include(s => s.Prescribed_Medication)
                    .Select(s => new ViewScriptsViewModel
                    {
                        PrescriptionId = s.PrescriptionId,
                        PatientID = s.PatientId,
                        PatientName = s.Patient.FirstName + " " + s.Patient.LastName,
                        DoctorName = s.User.FullName,
                        Schedule = s.Prescribed_Medication.Select(pm => pm.Medication.Schedule).ToList(),
                        Ward = _context.Admissions.Where(w => w.PatientID == s.PatientId).Select(w => w.Ward.Name).FirstOrDefault(),
                        DateCreated = s.DateIssued.ToString("dd/MM/yyyy"),
                        Status = _context.Prescriptions.Where(s => s.PatientId == id).Select(s => s.Status).FirstOrDefault(),
                        medications = s.Prescribed_Medication.Select(pm => pm.Medication.Name).ToList(),
                        Dosage = s.Prescribed_Medication.Select(pm => pm.Dosage).ToList()
                    })
                    .FirstOrDefaultAsync();

                if (script == null)
                {
                    TempData["Error"] = "Prescription not found.";
                    return RedirectToAction("Dashboard");
                }

                QuestPDF.Settings.License = LicenseType.Community;

                

                var pdf = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);

                        // Professional Header with Logo
                        page.Header()
                            .BorderBottom(2)
                            .BorderColor("#2C3E50")
                            .PaddingBottom(10)
                            .Row(row =>
                            {
                                // Logo section (optional - only if file exists)
                              

                                // Title section
                                row.RelativeItem()
                                    .Column(column =>
                                    {
                                        column.Item().Text("Prescription Report")
                                            .FontSize(24)
                                            .Bold()
                                            .FontColor("#2C3E50");

                                        column.Item().PaddingTop(5).Text($"Generated: {DateTime.Now:MMMM dd, yyyy}")
                                            .FontSize(10)
                                            .FontColor("#7F8C8D");
                                        
                                    });
                            });

                        // Content
                        page.Content()
                            .PaddingVertical(20)
                            .Column(column =>
                            {
                                // Patient Information Card
                                column.Item().PaddingBottom(15)
                                    .Border(1)
                                    .BorderColor("#E8E8E8")
                                    .Background("#FFFFFF")
                                    .Padding(15)
                                    .Column(col =>
                                    {
                                        col.Item().PaddingBottom(10)
                                            .Text("Patient Information")
                                            .FontSize(14)
                                            .Bold()
                                            .FontColor("#2C3E50");

                                        col.Item().Row(row =>
                                        {
                                            // Left column
                                            row.RelativeItem().Column(leftCol =>
                                            {
                                                leftCol.Item().PaddingBottom(5).Row(r =>
                                                {
                                                    r.ConstantItem(80).Text("Patient:").FontSize(10).SemiBold().FontColor("#7F8C8D");
                                                    r.RelativeItem().Text(script.PatientName).FontSize(10).FontColor("#2C3E50");
                                                });

                                                leftCol.Item().PaddingBottom(5).Row(r =>
                                                {
                                                    r.ConstantItem(80).Text("Doctor:").FontSize(10).SemiBold().FontColor("#7F8C8D");
                                                    r.RelativeItem().Text(script.DoctorName).FontSize(10).FontColor("#2C3E50");
                                                });
                                            });

                                            // Right column
                                            row.RelativeItem().Column(rightCol =>
                                            {
                                                rightCol.Item().PaddingBottom(5).Row(r =>
                                                {
                                                    r.ConstantItem(80).Text("Ward:").FontSize(10).SemiBold().FontColor("#7F8C8D");
                                                    r.RelativeItem().Text(script.Ward ?? "N/A").FontSize(10).FontColor("#2C3E50");
                                                });

                                                rightCol.Item().PaddingBottom(5).Row(r =>
                                                {
                                                    r.ConstantItem(80).Text("Date Issued:").FontSize(10).SemiBold().FontColor("#7F8C8D");
                                                    r.RelativeItem().Text(script.DateCreated).FontSize(10).FontColor("#2C3E50");
                                                });
                                            });

                                            // Status badge
                                            row.ConstantItem(100).AlignRight().AlignMiddle()
                                                .Border(1)
                                                .BorderColor(script.Status == "Approved" ? "#27AE60" : "#E74C3C")
                                                .Background(script.Status == "Approved" ? "#E8F8F0" : "#FADBD8")
                                                .Padding(8)
                                                .Text(script.Status ?? "Rejected")
                                                .FontSize(10)
                                                .Bold()
                                                .FontColor(script.Status == "Approved" ? "#27AE60" : "#E74C3C")
                                                .AlignCenter();
                                        });
                                    });

                                // Medications Section Header
                                column.Item().PaddingBottom(10)
                                    .Text("Prescribed Medications")
                                    .FontSize(16)
                                    .Bold()
                                    .FontColor("#2C3E50");

                                // Medications Table
                                column.Item().Table(table =>
                                {
                                    // Define columns
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.ConstantColumn(50);  // #
                                        columns.RelativeColumn(2);   // Medication Name
                                        columns.RelativeColumn(4);   // Dosage
                                        columns.RelativeColumn(2);   // Schedule
                                    });

                                    // Professional Table Header
                                    table.Header(header =>
                                    {
                                        header.Cell().Element(HeaderStyle).Text("#").Bold();
                                        header.Cell().Element(HeaderStyle).Text("Medication Name").Bold();
                                        header.Cell().Element(HeaderStyle).Text("Dosage").Bold();
                                        header.Cell().Element(HeaderStyle).Text("Schedule").Bold();
                                    });

                                    // Table Rows with alternating colors
                                    for (int i = 0; i < script.medications.Count; i++)
                                    {
                                        var isEven = i % 2 == 0;

                                        table.Cell().Element(c => RowStyle(c, isEven))
                                            .Text((i + 1).ToString());

                                        table.Cell().Element(c => RowStyle(c, isEven))
                                            .Text(script.medications[i]);

                                        table.Cell().Element(c => RowStyle(c, isEven))
                                            .Text(script.Dosage[i]);

                                        table.Cell().Element(c => RowStyle(c, isEven))
                                            .AlignCenter()
                                            .Text(script.Schedule[i]);
                                    }
                                });

                                // Important Notes Section
                                column.Item().PaddingTop(20)
                                    .Border(1)
                                    .BorderColor("#FFA500")
                                    .Background("#FFF9E6")
                                    .Padding(15)
                                    .Column(col =>
                                    {
                                        col.Item().PaddingBottom(5)
                                            .Text("Important Notes")
                                            .FontSize(12)
                                            .Bold()
                                            .FontColor("#FFA500");

                                        col.Item().Text("• Please follow the prescribed dosage and schedule strictly")
                                            .FontSize(9)
                                            .FontColor("#2C3E50");

                                        col.Item().Text("• Contact your doctor if you experience any side effects")
                                            .FontSize(9)
                                            .FontColor("#2C3E50");

                                        col.Item().Text("• Do not share medication with others")
                                            .FontSize(9)
                                            .FontColor("#2C3E50");
                                    });
                            });

                        // Professional Footer
                        page.Footer()
                            .BorderTop(1)
                            .BorderColor("#CCCCCC")
                            .PaddingTop(10)
                            .AlignCenter()
                            .DefaultTextStyle(x => x.FontSize(9).FontColor("#7F8C8D"))
                            .Text(x =>
                            {
                                x.Span("Page ");
                                x.CurrentPageNumber();
                                x.Span(" of ");
                                x.TotalPages();
                            });
                    });
                });

                // Helper function for header styling
                IContainer HeaderStyle(IContainer container)
                {
                    return container
                        .Background("#34495E")
                        .Padding(10)
                        .AlignMiddle()
                        .DefaultTextStyle(x => x.FontColor("#FFFFFF").FontSize(11));
                }

                // Helper function for row styling
                IContainer RowStyle(IContainer container, bool isEven)
                {
                    return container
                        .Background(isEven ? "#FFFFFF" : "#F8F9FA")
                        .Padding(8)
                        .BorderBottom(1)
                        .BorderColor("#E8E8E8")
                        .AlignMiddle()
                        .DefaultTextStyle(x => x.FontSize(10).FontColor("#2C3E50"));
                }

                var pdfBytes = pdf.GeneratePdf();

                if (pdfBytes == null || pdfBytes.Length == 0)
                {
                    TempData["Error"] = "No reports generated.";
                    return RedirectToAction("Dashboard");
                }

                TempData["Message"] = "Report generated successfully.";
                return File(pdfBytes, "application/pdf", $"Prescription_Report_{script.PatientName}_{DateTime.Now:yyyyMMdd}.pdf");
            }



    }
}
