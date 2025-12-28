using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ONT_3rdyear_Project.AdminViewModels;
using ONT_3rdyear_Project.Data;
using ONT_3rdyear_Project.Models;
using ONT_3rdyear_Project.Services;
using ONT_3rdyear_Project.ViewModels;
using System.Security.Claims;

namespace ONT_3rdyear_Project.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly IEmailSender _emailSender;


        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole<int>> roleManager, IEmailSender emailSender)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _emailSender = emailSender;
        }
        /* public async Task<IActionResult> Dashboard()
         {
             ViewBag.TotalStaff = await _context.Users.CountAsync(u => !u.IsDeleted);
             ViewBag.MedicationCount = await _context.Medications.CountAsync(m => !m.IsDeleted);
             ViewBag.WardCount = await _context.Wards.CountAsync(w => w.IsActive);
             ViewBag.BedCount = await _context.Beds.CountAsync(b => !b.IsDeleted);

             return View();
         }*/
        public async Task<IActionResult> Dashboard()
        {
            var totalBeds = await _context.Beds.CountAsync(b => !b.IsDeleted);
            var occupiedBeds = await _context.Beds.CountAsync(b => !b.IsDeleted && b.IsOccupied);
            var availableBeds = totalBeds - occupiedBeds;
            var totalEmployees = await _userManager.Users.CountAsync();
            var totalWards = await _context.Wards.CountAsync(w => w.IsActive);
            var totalMedications = await _context.Medications.CountAsync(m => !m.IsDeleted);
            var totalAllergies = await _context.Allergies.CountAsync(a => !a.IsDeleted);
            var totalConsumables = await _context.Consumables.CountAsync(c => !c.IsDeleted);

            var model = new AdminDashboardViewModel
            {
                TotalBeds = totalBeds,
                OccupiedBeds = occupiedBeds,
                AvailableBeds = availableBeds,
                TotalEmployees = totalEmployees,
                TotalWards = totalWards,
                TotalMedications = totalMedications,
                TotalAllergies = totalAllergies,
                TotalConsumables = totalConsumables
            };
            return View(model);
        }
        public async Task<IActionResult> MedicationStock()
        {
            // MEDICATION REPORT
            var medications = await _context.Medications
                .Where(m => !m.IsDeleted)
                .ToListAsync();

            ViewBag.MedicationLabels = medications.Select(m => m.Name).ToList();
            ViewBag.MedicationStockData = medications.Select(m => m.Quantity).ToList();
            ViewBag.MedicationExpiringData = medications
                .Select(m => m.ExpiryDate <= DateOnly.FromDateTime(DateTime.Now.AddDays(30)) ? m.Quantity : 0)
                .ToList();

            // Low stock alert (threshold: 10)
            ViewBag.LowStockMeds = medications.Where(m => m.Quantity <= 10).Select(m => m.Name).ToList();


            // BED REPORT
            var totalBeds = await _context.Beds.CountAsync(b => !b.IsDeleted);
            var occupiedBeds = await _context.Beds.CountAsync(b => !b.IsDeleted && b.IsOccupied);
            var availableBeds = totalBeds - occupiedBeds;

            ViewBag.BedLabels = new List<string> { "Total Beds", "Occupied", "Available" };
            ViewBag.BedData = new List<int> { totalBeds, occupiedBeds, availableBeds };

            // ALLERGIES REPORT
            var allergies = await _context.Allergies.Where(a => !a.IsDeleted).ToListAsync();
            ViewBag.AllergyLabels = allergies.Select(a => a.Name).ToList();
            ViewBag.AllergyData = allergies.Select(a => 1).ToList();


            // PATIENT TRENDS (Admissions last 7 days)
            var last7Days = Enumerable.Range(0, 7)
    .Select(i => DateTime.Now.Date.AddDays(-i))
    .Reverse()
    .ToList();

            ViewBag.PatientTrendLabels = last7Days.Select(d => d.ToString("MMM dd")).ToList();
            ViewBag.AdmissionsData = last7Days
    .Select(d => _context.Admissions
        .Count(a => a.AdmissionDate.Day == d.Day
                 && a.AdmissionDate.Month == d.Month
                 && a.AdmissionDate.Year == d.Year))
    .ToList();


            return View();
        }

        public async Task<IActionResult> ListMedication()
        {
            var medications = await _context.Medications.Where(m => !m.IsDeleted).ToListAsync();
            return View(medications);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if(id == null || _context.Medications == null)
            {
                return NotFound();
            }

            var medication = await _context.Medications.FirstOrDefaultAsync(m=>m.MedicationId == id);
            if (medication == null)
            {
                return NotFound();
            }
            return View(medication);

        }

        public IActionResult CreateMedication()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMedication(Medication medication)
        {
            try
            {
                _context.Add(medication);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(ListMedication));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "An error has occured");
            }
            return View(medication);
        }

        public async Task<IActionResult> EditMedication(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }
            var medication = await _context.Medications.FindAsync(id);
            if(medication == null)
            {
                return NotFound();
            }
            return View(medication);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMedication(Medication medication)
        {
            if (ModelState.IsValid)
            {
                _context.Medications.Update(medication);
                await _context.SaveChangesAsync();
                return RedirectToAction("ListMedication");
            }
            return View(medication);
        }

        
        public async Task<IActionResult> DeleteMedication(int id)
        {
            var medication = await _context.Medications.FindAsync(id);
            if (medication != null)
            {
                medication.IsDeleted = true;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Medications");
        }

        //crud for managing employees
        public async Task<IActionResult> Employees()
        {
            var users = await _userManager.Users.ToListAsync();

            var model = new List<EmployeeViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                model.Add(new EmployeeViewModel
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    Role = roles.FirstOrDefault() ?? "N/A",
                    IsActive = !user.IsDeleted
                });
            }

            return View(model);
        }

        public async Task<IActionResult> ToggleEmployeeStatus(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user != null)
            {
                user.IsDeleted = !user.IsDeleted;

                // Ensure SecurityStamp is not null
                if (string.IsNullOrEmpty(user.SecurityStamp))
                {
                    user.SecurityStamp = Guid.NewGuid().ToString();
                }

                await _userManager.UpdateAsync(user);
            }
            return RedirectToAction("Employees");
        }

        public async Task<IActionResult> AddEmployee()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            var model = new EmployeeViewModel
            {
                FullName = string.Empty,
                Email = string.Empty,
                Role = string.Empty,
                Roles = new SelectList(roles, "Name", "Name")
            };
            return View("AddEmployee", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEmployee(EmployeeViewModel vm, string password)
        {
            if (!ModelState.IsValid)
            {
                vm.Roles = new SelectList(_roleManager.Roles, "Name", "Name");
                return View("AddEmployee", vm);
            }

            var user = new ApplicationUser
            {
                UserName = vm.Email,
                Email = vm.Email,
                FullName = vm.FullName
            };

            var result = await _userManager.CreateAsync(user, password); // default password

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, vm.Role);
                await _userManager.AddClaimAsync(user, new Claim("ForcePasswordReset", "true"));

                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmationLink = Url.Action(
                    "ConfirmEmail",
                    "Account",
                    new { userId = user.Id, token = token },
                    protocol: HttpContext.Request.Scheme);

                // Build login link
                //var loginUrl = Url.Action("Login", "Account", null, Request.Scheme);

                // Build email body
                var message = $@"Hello {vm.FullName},<br/><br/>Your  profile has been created.<br/> <b>Username:</b> {vm.Email}<br/>
                                                     <b>Temporary Password:</b> {password}<br/><br/>
                                                     <p>Please <a href='{confirmationLink}'>click here</a> to confirm your email address.</p>
                                                     <p>After confirming, log in with your temporary password. 
                                                      You’ll be required to change it immediately.</p>";

                // Send email
                await _emailSender.SendEmailAsync(vm.Email, "Your Account Has Been Created", message);

                return RedirectToAction("Employees");
            }

            // if failed, show errors
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            vm.Roles = new SelectList(_roleManager.Roles, "Name", "Name", vm.Role);
            return View("AddEmployee", vm);
        }

        public async Task<IActionResult> EditEmployee(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await _roleManager.Roles.ToListAsync();
            var userRoles = await _userManager.GetRolesAsync(user);

            var model = new EmployeeViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = userRoles.FirstOrDefault(),
                Roles = new SelectList(roles, "Name", "Name", userRoles.FirstOrDefault())
            };

            return View("EditEmployee", model);
        }

        /*[HttpPost]
        public async Task<IActionResult> EditEmployee(ApplicationUser user)

        {
            ViewBag.RoleList = new SelectList( _context.Roles.ToList(),"Id","Name");

            if (ModelState.IsValid)
            {   
                var existingUser = await _context.Users.FindAsync(user.Id);
                if (existingUser == null)
                {
                    return NotFound(); // user deleted meanwhile
                }
                 existingUser.FullName = user.FullName;
                 existingUser.Email = user.Email;
                 existingUser.RoleType = user.RoleType;
                var currentRole = await _context.UserRoles.FirstOrDefaultAsync(ur => ur.UserId == existingUser.Id);       
                if(existingUser.RoleType != user.RoleType)
                {
                    if (currentRole != null)
                    {    // Remove old role
                        _context.UserRoles.Remove(currentRole);
                    }
                    existingUser.RoleType = user.RoleType;
                     // Assign new role by name
                    var selectedRole = _context.Roles.FirstOrDefault(r => r.Name == user.RoleType);
                    var userRole = new IdentityUserRole<int>
                    {
                        RoleId = selectedRole.Id,
                        UserId = user.Id,
                    
                    };
                    _context.Add(userRole);
                }                  
                await _context.SaveChangesAsync();
                return RedirectToAction("Employees");
            }
            return View(user);
        }*/
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEmployee(EmployeeViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Roles = new SelectList(_roleManager.Roles, "Name", "Name", vm.Role);
                return View("EmployeeForm", vm);
            }

            var user = await _userManager.FindByIdAsync(vm.Id.ToString());
            if (user == null) return NotFound();

            // Update allowed fields
            user.FullName = vm.FullName;
            user.Email = vm.Email;
            user.UserName = vm.Email; // keep in sync

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);

                vm.Roles = new SelectList(_roleManager.Roles, "Name", "Name", vm.Role);
                return View("EmployeeForm", vm);
            }

            // Update role
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Any())
                await _userManager.RemoveFromRolesAsync(user, roles);

            await _userManager.AddToRoleAsync(user, vm.Role);

            return RedirectToAction("Employees");
        }
        /* [HttpGet]
         public async Task<IActionResult> DeleteEmployee(int id)
         {
             var user = await _context.Users.FindAsync(id);
             if (user == null) return NotFound();
             return View(user); // confirmation page
         }*/

        [HttpPost, ActionName("DeleteEmployee")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmEmployee(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                user.IsDeleted = true;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Employees");
        }
        // Reactivate Employee
        [HttpPost]
        [HttpPost, ActionName("ReactivateEmployee")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReactivateEmployee(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                user.IsDeleted = false;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Employees");
        }

        // WARD MANAGEMENT
        public async Task<IActionResult> Wards()
        {
            var wards = await _context.Wards.Where(w => w.IsActive).ToListAsync();
            return View(wards);
        }

        public IActionResult AddWard()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddWard(Ward ward)
        {
            if (ModelState.IsValid)
            {
                _context.Wards.Add(ward);
                await _context.SaveChangesAsync();
                return RedirectToAction("Wards");
            }
            return View(ward);
        }

        public async Task<IActionResult> EditWard(int id)
        {
            var ward = await _context.Wards.FindAsync(id);
            return View(ward);
        }

        [HttpPost]
        public async Task<IActionResult> EditWard(Ward ward)
        {
            if (ModelState.IsValid)
            {
                _context.Wards.Update(ward);
                await _context.SaveChangesAsync();
                return RedirectToAction("Wards");
            }
            return View(ward);
        }

        public async Task<IActionResult> DeleteWard(int id)
        {
            var ward = await _context.Wards.FindAsync(id);
            if (ward != null)
            {
                ward.IsActive = false;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Wards");
        }

        //BEDS MANAGEMENT
        public async Task<IActionResult> Beds()
        {
            var beds = await _context.Beds.Include(b => b.Ward).Where(b => !b.IsDeleted).ToListAsync();
            return View(beds);
        }

        public IActionResult AddBed()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddBed(Bed bed)
        {
            if (ModelState.IsValid)
            {
                _context.Beds.Add(bed);
                await _context.SaveChangesAsync();
                return RedirectToAction("Beds");
            }
            return View(bed);
        }

        public async Task<IActionResult> EditBed(int id)
        {
            var bed = await _context.Beds.FindAsync(id);
            return View(bed);
        }

        [HttpPost]
        public async Task<IActionResult> EditBed(Bed bed)
        {
            if (ModelState.IsValid)
            {
                _context.Beds.Update(bed);
                await _context.SaveChangesAsync();
                return RedirectToAction("Beds");
            }
            return View(bed);
        }

        public async Task<IActionResult> DeleteBed(int id)
        {
            var bed = await _context.Beds.FindAsync(id);
            if (bed != null)
            {
                bed.IsDeleted = true;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Beds");
        }

        // CONSUMABLES MANAGEMENT
        //public async Task<IActionResult> Consumables()
        //{
        //    var consumables = await _context.Consumables.Where(c => !c.IsDeleted).ToListAsync();
        //    return View(consumables);
        //}

        //public IActionResult AddConsumable() => View();

        //[HttpPost]
        //public async Task<IActionResult> AddConsumable(Consumable item)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _context.Consumables.Add(item);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction("Consumables");
        //    }
        //    return View(item);
        //}

        //public async Task<IActionResult> EditConsumable(int id)
        //{
        //    var item = await _context.Consumables.FindAsync(id);
        //    return View(item);
        //}

        //[HttpPost]
        //public async Task<IActionResult> EditConsumable(Consumable item)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _context.Consumables.Update(item);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction("Consumables");
        //    }
        //    return View(item);
        //}

        //public async Task<IActionResult> DeleteConsumable(int id)
        //{
        //    var item = await _context.Consumables.FindAsync(id);
        //    if (item != null)
        //    {
        //        item.IsDeleted = true;
        //        await _context.SaveChangesAsync();
        //    }
        //    return RedirectToAction("Consumables");
        //}

        //ALLERGY MANAGEMENT
        public async Task<IActionResult> Allergies()
        {
            var list = await _context.Allergies.Where(a => !a.IsDeleted).ToListAsync();
            return View(list);
        }

        public IActionResult AddAllergy()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAllergy(Allergy model)
        {
            if (ModelState.IsValid)
            {
                _context.Allergies.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction("Allergies");
            }
            return View(model);
        }

        public async Task<IActionResult> EditAllergy(int id)
        {
            var allergy = await _context.Allergies.FindAsync(id);
            if (allergy == null || allergy.IsDeleted)
            {
                return NotFound();
            }
            return View(allergy);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAllergy(Allergy model)
        {
            if (ModelState.IsValid)
            {
                _context.Allergies.Update(model);
                await _context.SaveChangesAsync();
                return RedirectToAction("Allergies");
            }
            return View(model);
        }

        public async Task<IActionResult> DeleteAllergy(int id)
        {
            var allergy = await _context.Allergies.FindAsync(id);
            if (allergy != null)
            {
                allergy.IsDeleted = true;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Allergies");
        }

        // HOSPITAL INFO MANAGEMENT
        public async Task<IActionResult> HospitalInfo()
        {
            var info = await _context.HospitalInfo.FirstOrDefaultAsync();
            return View(info);
        }

        [HttpPost]
        public async Task<IActionResult> HospitalInfo(HospitalInfo info)
        {
            _context.HospitalInfo.Update(info);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Hospital Info Updated";
            return RedirectToAction("HospitalInfo");
        }
    }
}
