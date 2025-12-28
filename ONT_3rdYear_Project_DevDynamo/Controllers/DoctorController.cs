using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using ONT_3rdyear_Project.Data;
using ONT_3rdyear_Project.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;
using System.Threading.Tasks;
using ONT_3rdyear_Project.ViewModels;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks.Sources;
using System.Security.Claims;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.Rendering;
using ONT_3rdyear_Project.SereroViewModels;

namespace ONT_3rdyear_Project.Controllers
{
	[Authorize(Roles = "Doctor")]
	public class DoctorController : Controller
	{
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

		public DoctorController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
		{
			_context = context;
			_userManager = userManager;
		}
		//dashboard
        public async Task<IActionResult> DashBoard()
        {

            return  View();
        }
		//Navigate to all patients 
        public async Task<IActionResult> PatientFolders(string searchQuery)
        {
            var viewModelList = from patient in _context.Patients
                                join adm in _context.Admissions on patient.PatientID equals adm.PatientID into admGroup
                                from adm in admGroup.DefaultIfEmpty()
                                join ward in _context.Wards on adm.WardID equals ward.WardID into wardGroup
                                from ward in wardGroup.DefaultIfEmpty()
                                join bed in _context.Beds on adm.BedID equals bed.BedId into bedGroup
                                from bed in bedGroup.DefaultIfEmpty()
                                select new PatientInfoViewModel
                                {
									PatientID = patient.PatientID,
                                    FirstName = patient.FirstName,
                                    LastName = patient.LastName,
                                    Name = ward != null ? ward.Name : "Not Currently Admitted",
                                    BedNo = bed != null ? bed.BedNo : "No Bed Allocated"
                                };


            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                searchQuery = searchQuery.Trim();

                viewModelList = viewModelList
                    .Where(p =>
                        p.FirstName.Contains(searchQuery) ||
                        p.LastName.Contains(searchQuery) ||
                        ($"{p.FirstName} {p.LastName}").Contains(searchQuery));
            }
            return View(viewModelList.ToList());

        }
		//View specific patient folder
		//[Route("Doctor/SinglePatientFolder/{id}")]
        public async Task<IActionResult> SinglePatientFolder(int id)
        {
            //var patient = await _context.Patients
            //    .Include(p => p.VisitSchedules)
            //    .Include(p => p.MedicalHistories)
            //    .Include(p => p.Vitals)
            //    .Include(p => p.Treatments)
            //    .FirstOrDefaultAsync(p => p.PatientID == id);
			var patient = (from p in _context.Patients
                          where p.PatientID == id
                          join vst in _context.VisitSchedules on p.PatientID equals vst.PatientID into vstGroup
                          from vst in vstGroup.DefaultIfEmpty()
                          join medHis in _context.MedicalHistories on p.PatientID equals medHis.PatientId into medHisGroup
                          from medHis in medHisGroup.DefaultIfEmpty()
                          join vit in _context.Vitals on p.PatientID equals vit.PatientID into vitGroup
                          from vit in vitGroup.DefaultIfEmpty()
                          join treat in _context.Treatments on p.PatientID equals treat.PatientID into treatGroup
                          from treat in treatGroup.DefaultIfEmpty()
                          select new SinglePatientFileViewModel
                          {
                              FirstName = p.FirstName,
                              LastName = p.LastName,

                              ChronicCondition = medHis != null ? medHis.ChronicCondition : "None reported",
                              MedicationHistory = medHis != null ? medHis.MedicationHistory : "No medications",
                              PastSurgicalHistory = medHis != null ? medHis.PastSurgicalHistory : "No surgeries",
                              RecorderDate = medHis != null ? medHis.RecorderDate : DateTime.MinValue,

                              // Vitals
                              BP = vit != null ? vit.BP : "120/80",
                              Temperature = vit != null ? vit.Temperature : 36.5,
                              SugarLevel = vit != null ? vit.SugarLevel.ToString() : "Normal",
                              PulseRate = vit != null ? vit.PulseRate.ToString() + " bmp" : "72 bpm",
                              Date = vit != null ? vit.Date : DateTime.MinValue,

                              // Treatment
                              TreatmentType = treat != null ? treat.TreatmentType : "Not Available",
                              TreatmentDate = treat!= null? treat.TreatmentDate: DateTime.MinValue,
                          }).FirstOrDefault();
            if (patient == null)
                return NotFound();

            return View(patient);
        }

        //Record a visit
        [HttpGet]
        public IActionResult RecordVisit(int id)
        {
            // You can pass the patientId to the view
            return View(model: id);
        }

        [HttpPost]
	   [ValidateAntiForgeryToken]
		public async Task<IActionResult> RecordVisit(int patientId, string notes, string VisitReason, string instructions, DateTime date)
		{
			string userid = User.FindFirstValue(ClaimTypes.NameIdentifier);
			int? ApplicationUserId = null;
			int id = 0;
			if(int.TryParse(userid, out int parsedid))
			{
				ApplicationUserId = parsedid;
				id = parsedid;
			}
			
            if (notes == null)
			{
				notes = "N/A";
			}
			var visit = new TreatVisit
			{
				PatientID = patientId,
				ApplicationUserID = ApplicationUserId,
				VisitDate = DateTime.Now,
				ReasonForVisit = VisitReason,
				Notes = notes,
			};


			_context.TreatVisits.Add(visit);
			await _context.SaveChangesAsync();
			//add instructions
			//returning the primary key after adding to the database
			int newID = visit.TreatVisitID;
			var instructns = new Instruction
			{
				PatientID = patientId,
				ApplicationUserID = id,
				TreatVisitID = newID,
				Instructions = instructions,
				DateRecorded = date,

			};
			_context.Instructions.Add(instructns);
			//var viewModel = new VisitInstructionViewModel
			//{
			//	Visit = visit,
			//	Instructns = instructns
			//};
			//return View("VisitDetails", viewModel);

			TempData["Success"] = "Visit recorded successfully.";
			//fix the id code, the applicaation is returning to the collection view
			return RedirectToAction(nameof(PatientFolders), new { id = patientId });
		}
        //inside the visit page there should be a button for instructions it will come as a pop up then i can add the  instructions if required
        //also add a message notification where you are able to view instruction requests from nurse
        //java script to count number of requests available use bool to check them as viewed or unviewed
        // Prescribe medication

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PrescribeMedicationToPatient(PrescribeMedicationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadAvailableMedicationsAsync(model);
                return View(model);
            }

            try
            {
                // Get current user ID
                var userId = int.Parse(User.FindFirst("UserId").Value);

                // Validate patient exists
                var patient = await _context.Patients.FindAsync(model.PatientId);
                if (patient == null)
                {
                    TempData["Error"] = "Patient not found.";
                    return RedirectToAction("PatientFolders");
                }

                // Validate medications exist and are not expired
                var medicationIds = model.MedicationsWithDosage.Select(m => m.MedicationId).ToList();
                var validMedications = await _context.Medications
                    .Where(m => medicationIds.Contains(m.MedicationId) &&
                               m.ExpiryDate >= DateOnly.FromDateTime(DateTime.Today))
                    .ToListAsync();

                if (validMedications.Count != medicationIds.Count)
                {
                    TempData["Error"] = "One or more selected medications are invalid or expired.";
                    await LoadAvailableMedicationsAsync(model);
                    return View(model);
                }

                // Create prescription with medications
                var prescription = new Prescription
                {
                    ApplicationUserID = userId,
                    PatientId = model.PatientId,
                    DateIssued = DateTime.UtcNow,
                    Status = "New",
                    Prescribed_Medication = model.MedicationsWithDosage.Select(med => new PrescribeMedication
                    {
                        MedicationId = med.MedicationId,
                        Dosage = med.Dosage
                    }).ToList()
                };

                _context.Prescriptions.Add(prescription);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Prescription issued for {patient.FirstName} {patient.LastName} with {model.MedicationsWithDosage.Count} medication(s).";
                return RedirectToAction("PatientFolders");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while processing the prescription.";
                await LoadAvailableMedicationsAsync(model);
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> PrescribeMedicationToPatient(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient == null)
            {
                TempData["Error"] = "Patient not found.";
                return RedirectToAction("PatientFolders");
            }

            var model = new PrescribeMedicationViewModel
            {
                PatientId = id,
                PatientName = $"{patient.FirstName} {patient.LastName}",
                MedicationsWithDosage = new List<MedicationDosageItem>()
            };

            await LoadAvailableMedicationsAsync(model);
            return View(model);
        }

        private async Task LoadAvailableMedicationsAsync(PrescribeMedicationViewModel model)
        {
            model.AvailableMedications = await _context.Medications
                .Where(m => m.ExpiryDate >= DateOnly.FromDateTime(DateTime.Today))
                .OrderBy(m => m.Name)
                .Select(m => new SelectListItem
                {
                    Value = m.MedicationId.ToString(),
                    Text = $"{m.Name} (Schedule {m.Schedule})"
                })
                .ToListAsync();
        }

		// 📤 Discharge Patient
		[HttpPost]
        [ValidateAntiForgeryToken]
		public async Task<IActionResult> DischargePatient(int id, DateTime DischargeDate, string DischargeInstruction)
		{

            //Getting the user ID
            string userid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int ApplicationUserId = 0;
            if (int.TryParse(userid, out int parsedid))
            {
                ApplicationUserId = parsedid;
            }
            //FINDING THE PATIENT USING THEIR ID
            var patient = await _context.Patients.FindAsync(id);
			if (patient == null)
				return NotFound();

            //VARIFYING THAT PATIENT IS NOT ALREADY DISCHARGED
			//var discharge = await (from p in _context.Patients
			//					   join d in _context.Discharges on p.PatientID equals d.PatientID
			//					   where p.PatientID == id && p.Admitted == true
			//					   select d).FirstOrDefaultAsync();
            var discharge = await (from p in  _context.Patients
                                   where p.Admitted == true
                                   select p).FirstOrDefaultAsync();
            //IF PATIENT IS DISCHARGED RETURN BAD REQUEST ELSE DISCHARGE PATIENT

			if (discharge == null)
				return BadRequest("Patient already discharged or discharge record not found.");

            var dischargePatient = new Discharge
            {
                ApplicationUserID = ApplicationUserId,
                PatientID = id,
                DischargeDate = DischargeDate,
                DischargeInstructions = DischargeInstruction,
                IsDischarged = true
            };

            _context.Discharges.Add(dischargePatient);


			await _context.SaveChangesAsync();


			return RedirectToAction("Index", "Dashboard");
		}

	}
}
