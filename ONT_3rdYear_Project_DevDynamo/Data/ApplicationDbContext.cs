using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.EntityFrameworkCore;
using ONT_3rdyear_Project.Models;

namespace ONT_3rdyear_Project.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets
        public DbSet<Order> Orders { get; set; }
        public DbSet<Admission> Admissions { get; set; }
        public DbSet<Allergy> Allergies { get; set; }
        public DbSet<Bed> Beds { get; set; }
        public DbSet<Consumable> Consumables { get; set; }
        public DbSet<Discharge> Discharges { get; set; }
        public DbSet<Instruction> Instructions { get; set; }
        public DbSet<MedicalHistory> MedicalHistories { get; set; }
        public DbSet<Medication> Medications { get; set; }
        public DbSet<Movement> Movements { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<PatientAllergy> PatientAllergies { get; set; }
        public DbSet<Pharmacy> Pharmacies { get; set; }
        //Ward Consumable De Sets......
        public DbSet<StockTake> StockTakes { get; set; }
        public DbSet<StockTakeItem> StockTakeItems { get; set; }
        public DbSet<Delivery> Deliveries { get; set; }
        public DbSet<DeliveryItem> DeliveryItems { get; set; }
        public DbSet<WardConsumable> WardConsumables { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<SupplierItem> SupplierItems { get; set; }
        public DbSet<PharmacyMedication> PharmacyMedications { get; set; }
        public DbSet<PrescribeMedication> PrescribeMedications { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }
        public DbSet<PrescriptionForwarding> PrescriptionForwardings { get; set; }
        public DbSet<PrescriptionRejection> PrescriptionRejections { get; set; }
       
        public DbSet<TreatVisit> TreatVisits { get; set; }
        public DbSet<Treatment> Treatments { get; set; }
        public DbSet<VisitSchedule> VisitSchedules { get; set; }
        public DbSet<Vital> Vitals { get; set; }
        public DbSet<Ward> Wards { get; set; }
        public DbSet<ConsumableOrder> ConsumableOrders { get; set; }
        public DbSet<PatientMedicationScript> PatientMedicationScripts { get; set; }
        public DbSet<HospitalInfo> HospitalInfo { get; set; }
        public DbSet<DoctorAssignment> DoctorAssignments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Map ApplicationUser to "User" table
            //modelBuilder.Entity<ApplicationUser>().ToTable("User");

            // Configure relationships with cascade restriction to avoid multiple cascade paths
            modelBuilder.Entity<PrescriptionForwarding>()
                .HasOne(pf => pf.Prescription)
                .WithMany(p => p.PrescriptionForwarding)
                .HasForeignKey(pf => pf.PrescriptionID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PrescriptionRejection>()
                .HasOne(pr => pr.Prescription)
                .WithMany(p => p.PrescriptionRejection)
                .HasForeignKey(pr => pr.PrescriptionID)
                .OnDelete(DeleteBehavior.Restrict);

           

            //wardconsumable many-to-many join
            modelBuilder.Entity<WardConsumable>()
                .HasKey(wc => new { wc.WardID, wc.ConsumableID });

            modelBuilder.Entity<WardConsumable>()
                .HasOne(wc => wc.Ward)
                .WithMany(w => w.WardConsumables)
                .HasForeignKey(wc => wc.WardID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WardConsumable>()
                .HasOne(wc => wc.Consumable)
                .WithMany(c => c.WardConsumables)
                .HasForeignKey(wc => wc.ConsumableID)
                .OnDelete(DeleteBehavior.Restrict);

            //delivery items many-to-many join
            modelBuilder.Entity<DeliveryItem>()
                .HasKey(di => new { di.DeliveryID, di.ConsumableID });

            modelBuilder.Entity<DeliveryItem>()
                .HasOne(di => di.Delivery)
                .WithMany(d => d.DeliveryItems)
                .HasForeignKey(di => di.DeliveryID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DeliveryItem>()
                .HasOne(di => di.Consumable)
                .WithMany(c => c.DeliveryItems)
                .HasForeignKey(di => di.ConsumableID)
                .OnDelete(DeleteBehavior.Restrict);


            // SupplierItem many-to-many join
            modelBuilder.Entity<SupplierItem>()
                .HasKey(si => new { si.SupplierID, si.ConsumableID });

            modelBuilder.Entity<SupplierItem>()
                .HasOne(si => si.Supplier)
                .WithMany(s => s.SupplierItems)
                .HasForeignKey(si => si.SupplierID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SupplierItem>()
                .HasOne(si => si.Consumable)
                .WithMany(c => c.SupplierItems)
                .HasForeignKey(si => si.ConsumableID)
                .OnDelete(DeleteBehavior.Restrict);

            //stocktake items many-to-many join
            modelBuilder.Entity<StockTakeItem>()
                .HasKey(sti => new { sti.StockTakeID, sti.ConsumableID });

            modelBuilder.Entity<StockTakeItem>()
                .HasOne(sti => sti.StockTake)
                .WithMany(st => st.StockTakes)
                .HasForeignKey(sti => sti.StockTakeID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StockTakeItem>()
                .HasOne(sti => sti.Consumable)
                .WithMany(c => c.StockTakeItems)
                .HasForeignKey(sti => sti.ConsumableID)
                .OnDelete(DeleteBehavior.Restrict);

            // Consumable_Order many-to-many join
            modelBuilder.Entity<ConsumableOrder>()
                .HasKey(co => new { co.ConsumableId, co.OrderId });

            modelBuilder.Entity<ConsumableOrder>()
                .HasOne(co => co.Consumable)
                .WithMany(c => c.ConsumableOrders)
                .HasForeignKey(co => co.ConsumableId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ConsumableOrder>()
                .HasOne(co => co.Order)
                .WithMany(o => o.ConsumableOrders)
                .HasForeignKey(co => co.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            // Patient_Allergy many-to-many join
            modelBuilder.Entity<PatientAllergy>()
                .HasKey(pa => new { pa.PatientId, pa.AllergyId });

            modelBuilder.Entity<PatientAllergy>()
                .HasOne(pa => pa.Patient)
                .WithMany(p => p.PatientAllergies)
                .HasForeignKey(pa => pa.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PatientAllergy>()
                .HasOne(pa => pa.Allergy)
                .WithMany(a => a.PatientAllergies)
                .HasForeignKey(pa => pa.AllergyId)
                .OnDelete(DeleteBehavior.Restrict);

            // Pharmacy-Medication
            modelBuilder.Entity<PharmacyMedication>()
                .HasKey(pm => pm.PhamarcyMedId);

            modelBuilder.Entity<PharmacyMedication>()
                .HasOne(pm => pm.pharmacy)
                .WithMany(p => p.PharmacyMedications)
                .HasForeignKey(pm => pm.PhamarcyID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PharmacyMedication>()
                .HasOne(pm => pm.medication)
                .WithMany(m => m.PharmacyMedications)
                .HasForeignKey(pm => pm.MedicationID)
                .OnDelete(DeleteBehavior.Restrict);

            // Prescription-Medication
            modelBuilder.Entity<PrescribeMedication>()
                .HasKey(pm => pm.PrescribedMedicationId);

            modelBuilder.Entity<PrescribeMedication>()
                .HasOne(pm => pm.Prescription)
                .WithMany(p => p.Prescribed_Medication)
                .HasForeignKey(pm => pm.PrescriptionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PrescribeMedication>()
                .HasOne(pm => pm.Medication)
                .WithMany(m => m.PrescribeMadications)
                .HasForeignKey(pm => pm.MedicationId)
                .OnDelete(DeleteBehavior.Restrict);

            // Admission relationships with restrict delete
            modelBuilder.Entity<Admission>()
                .HasOne(a => a.Ward)
                .WithMany(w => w.Admissions)
                .HasForeignKey(a => a.WardID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Admission>()
                .HasOne(a => a.Bed)
                .WithOne(b => b.Admissions)
                .HasForeignKey<Admission>(a => a.BedID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Admission>()
                 .HasOne(a => a.Patient)
                 .WithOne(p => p.Admissions) 
                 .HasForeignKey<Admission>(a => a.PatientID)
                 .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Instruction>()
                .HasOne(i => i.User)
                .WithMany(u => u.Instructions)
                .HasForeignKey(i => i.ApplicationUserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Instruction>()
                .HasOne(i => i.VisitSchedule)
                .WithMany(vs => vs.Instructions)
                .HasForeignKey(i => i.VisitID)
                .OnDelete(DeleteBehavior.Restrict); // or DeleteBehavior.NoAction


            modelBuilder.Entity<Treatment>()
                .HasOne(t => t.User)
                .WithMany(u => u.Treatments)
                .HasForeignKey(t => t.ApplicationUserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Treatment>()
                .HasOne(t => t.VisitSchedule)
                .WithMany(vs => vs.Treatments)
                .HasForeignKey(t => t.VisitID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Discharge>()
                .HasOne(d => d.User)
                .WithMany(u => u.Discharges)
                .HasForeignKey(d => d.ApplicationUserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Prescription>()
                .HasOne(p => p.User)
                .WithMany(u => u.Prescriptions)
                .HasForeignKey(p => p.ApplicationUserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<VisitSchedule>()
                .HasOne(v => v.User)
                .WithMany(u => u.VisitSchedules)
                .HasForeignKey(v => v.ApplicationUserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PatientMedicationScript>()
                .HasOne(pm => pm.AdministeredBy)
                .WithMany(u => u.AdministraterMedications)
                .HasForeignKey(pm => pm.ApplicationUserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TreatVisit>()
                .HasOne(tv => tv.User)
                .WithMany(u => u.TreatVisits)
                .HasForeignKey(tv => tv.ApplicationUserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Vital>()
                .HasOne(v => v.User)
                .WithMany(u => u.Vitals)
                .HasForeignKey(v => v.ApplicationUserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Vital>()
                .HasOne(v => v.VisitSchedule)
                .WithMany(vs => vs.Vitals)
                .HasForeignKey(v => v.VisitID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PatientMedicationScript>()
                .HasOne(pms => pms.VisitSchedule)
                .WithMany(vs => vs.PatientMedicationScripts) 
                .HasForeignKey(pms => pms.VisitID)
                .OnDelete(DeleteBehavior.Restrict); // avoid cascade cycle

            modelBuilder.Entity<Movement>()
               .HasOne(m => m.Patient)
               .WithMany(p => p.Movements)
               .HasForeignKey(m => m.PatientID)
               .OnDelete(DeleteBehavior.Restrict); 

            // Movement -> Ward
            modelBuilder.Entity<Movement>()
                .HasOne(m => m.Ward)
                .WithMany(w => w.Movements)
                .HasForeignKey(m => m.WardID)
                .OnDelete(DeleteBehavior.Restrict);

            /* modelBuilder.Entity<IdentityRole<int>>().HasData(
                 new IdentityRole<int> { Id = 1, Name = "Admin", NormalizedName = "ADMIN" },
                 new IdentityRole<int> { Id = 2, Name = "Doctor", NormalizedName = "DOCTOR" },
                 new IdentityRole<int> { Id = 3, Name = "Nurse", NormalizedName = "NURSE" },
                 new IdentityRole<int> { Id = 4, Name = "Sister", NormalizedName = "SISTER" },
                 new IdentityRole<int> { Id = 5, Name = "ConsumableManager", NormalizedName = "ConsumableManager"},
                 new IdentityRole<int> { Id = 6, Name = "ScriptManager", NormalizedName = "ScriptManager"},
                 new IdentityRole<int> { Id = 7, Name = "WardManager", NormalizedName = "WardAdmin"}

             );*/

            var hasher = new PasswordHasher<ApplicationUser>();
            modelBuilder.Entity<ApplicationUser>().HasData(
                new ApplicationUser {Id = 1, UserName = "doctor@hospital.com",NormalizedUserName = "DOCTOR@HOSPITAL.COM", Email = "doctor@hospital.com",NormalizedEmail = "DOCTOR@HOSPITAL.COM", EmailConfirmed = true, PasswordHash = hasher.HashPassword(null, "Password123!"), FullName = "Dr. John Doe", RoleType = "Doctor" },
                 new ApplicationUser{Id = 2, UserName = "nurse@hospital.com", NormalizedUserName = "NURSE@HOSPITAL.COM", Email = "nurse@hospital.com", NormalizedEmail = "NURSE@HOSPITAL.COM", EmailConfirmed = true, PasswordHash = hasher.HashPassword(null, "Password123!"), FullName = "Nurse Thabo",  RoleType = "Nurse" } );


            // Seed Patients
            modelBuilder.Entity<Patient>().HasData(
                new Patient{PatientID = 1, FirstName = "Naledi",LastName = "Kgomo", DateOfBirth = new DateTime(2000, 01, 15),Gender = "Female",ChronicIllness = "Hypertension", Admitted = false},
                new Patient{PatientID = 2, FirstName = "Tshepo",LastName = "Mabasa",DateOfBirth = new DateTime(1995, 06, 21),Gender = "Male",ChronicIllness = "Diabetes", Admitted = true},
                new Patient { PatientID = 3, FirstName = "Thando", LastName = "Smith", DateOfBirth = new DateTime(2003, 02, 28), Gender = "Female", ChronicIllness = "Herpertension", Admitted = true}
            );

            // seed allergies
            modelBuilder.Entity<Allergy>().HasData(
                new Allergy { AllergyId = 1, Name = "Penicillin", Description = "Allergic reaction to penicillin antibiotics" },
                new Allergy { AllergyId = 2, Name = "Peanuts", Description = "Allergic reaction to peanut proteins" },
                new Allergy { AllergyId = 3, Name = "Shellfish", Description = "Allergic reaction to shellfish" },
                new Allergy { AllergyId = 4, Name = "Latex", Description = "Allergic reaction to latex materials" },
                new Allergy { AllergyId = 5, Name = "Dust Mites", Description = "Allergic reaction to dust mite proteins" }
            );
            // Seed admission
            modelBuilder.Entity<Admission>().HasData(
                new Admission { AdmissionID = 1, PatientID = 2, WardID = 2, DoctorId = 1, BedID = 4, AdmissionDate = new DateOnly(2025, 09, 04), DischargeDate = null, Notes = null, ReasonForAdmission = "Surgery"},
                new Admission { AdmissionID = 2, PatientID = 3, WardID = 1, DoctorId = 1, BedID = 2, AdmissionDate = new DateOnly(2025, 09, 04), DischargeDate = null, Notes = null, ReasonForAdmission = "Patient was admitted for having severe migraine" }
            );

            // Seed Wards
            modelBuilder.Entity<Ward>().HasData(
                new Ward {WardID = 1,Name = "General Ward",Capacity = 10},
                new Ward { WardID = 2, Name = "ICU", Capacity = 10 }
            );

            // Seed Beds
            modelBuilder.Entity<Bed>().HasData(
                new Bed { BedId = 1, WardID = 1, BedNo = "G1", IsOccupied = true },
                new Bed { BedId = 2, WardID = 1, BedNo = "G2", IsOccupied = true },
                new Bed { BedId = 3, WardID = 1, BedNo = "G3", IsOccupied = true },
                new Bed { BedId = 4, WardID = 2, BedNo = "C1", IsOccupied = true }
            );

            // Seed Medications
            modelBuilder.Entity<Medication>().HasData(
                new Medication{MedicationId = 1,Name = "Paracetamol",Schedule = 1,ExpiryDate = new DateOnly(2026, 1, 1)},
                new Medication{MedicationId = 2,Name = "Insulin",Schedule = 4,ExpiryDate = new DateOnly(2025, 12, 1)}
            );

            // Seed Vitals
            modelBuilder.Entity<Vital>().HasData(
                new Vital { VitalID = 1,Date = new DateTime(2025, 07, 03, 09, 30, 00, DateTimeKind.Utc), BP = "120/80", Temperature = 38.27, PulseRate = 72, SugarLevel = 5.5, PatientID = 1, ApplicationUserID = 1, IsActive = true },
                new Vital {  VitalID = 2, Date = new DateTime(2025, 07, 03, 10, 30, 00, DateTimeKind.Utc), BP = "130/85", Temperature = 37.12, PulseRate = 60, SugarLevel = 7.2,  PatientID = 2, ApplicationUserID = 2 , IsActive = true }
            );
            modelBuilder.Entity<VisitSchedule>().HasData(
                new VisitSchedule{VisitID = 1,ApplicationUserID = 1,PatientID = 1, VisitDate = new DateTime(2025, 09, 10, 09, 30,00),Feedback = "Initial checkup - stable condition.", NextVisit = new DateTime(2025, 8, 1),IsActive = true},
                new VisitSchedule{VisitID = 2,ApplicationUserID = 1, PatientID = 2,VisitDate = new DateTime(2025, 09, 30, 09, 00, 00),Feedback = "Follow-up visit required medication adjustment.", NextVisit = new DateTime(2025, 7, 15),IsActive = true}
            );

            //seed treatment
            modelBuilder.Entity<Treatment>().HasData(
                new Treatment { TreatmentID = 1, VisitID = 1, ApplicationUserID = 2, PatientID = 1, TreatmentType = "IV Drip setup", TreatmentDate = new DateTime(2025, 08, 08, 10, 00, 00), IsActive = true },
                new Treatment {  TreatmentID = 2, VisitID = 2, ApplicationUserID = 2, PatientID = 2, TreatmentType = "Wound Dressing", TreatmentDate = new DateTime(2025, 10, 11, 12,00,00), IsActive = true }
             );

            modelBuilder.Entity<TreatVisit>().HasData(
                new TreatVisit { TreatVisitID = 1, ApplicationUserID = 1, PatientID = 1, VisitDate = new DateTime(2025, 09, 10, 09, 00, 00), Notes = "Initial wound dressing and IV fluid administered.", ReasonForVisit = "monitor patient recovery process"},
                new TreatVisit { TreatVisitID = 2, ApplicationUserID = 2, PatientID = 2, VisitDate = new DateTime(2025, 08,11,10,00,00), Notes = "Follow-up visit to monitor fever.", ReasonForVisit = "monitor patient temperature" });

            modelBuilder.Entity<Instruction>().HasData(
                new Instruction { InstructionID = 1, PatientID = 1, ApplicationUserID = 1, TreatVisitID = null, VisitID = 1, NurseRequest = "Please advise on wound management.", Instructions = "Monitor vitls every 4 hours", DateRecorded = new DateTime(2025, 10, 11, 12, 00, 00)},
                new Instruction { InstructionID = 2, PatientID = 2, ApplicationUserID = 2, TreatVisitID = 1, VisitID = 2, NurseRequest = "Please advise on wound management.", Instructions = "Administer antibiotics as prescribed.", DateRecorded = new DateTime(2025, 11, 12, 10, 00, 00)}
             );

            modelBuilder.Entity<PatientMedicationScript>().HasData(
                new PatientMedicationScript { Id = 1, PatientId = 1, MedicationId = 1, VisitID = 1, ApplicationUserID = 2, PrescriptionId = null, AdministeredDate = new DateTime(2025, 7, 14, 10, 30, 0), Dosage = "2 tablets twice a day" },
                new PatientMedicationScript { Id = 2, PatientId = 2, MedicationId = 2, VisitID = 2, ApplicationUserID = 2, PrescriptionId = null, AdministeredDate = new DateTime(2025, 7, 14, 11, 15, 0), Dosage = "1 tablet daily after meal"}
             );

            modelBuilder.Entity<HospitalInfo>().HasData(
               new HospitalInfo { HospitalInfoId = 1, Name = "Sunrise Medical centre", Address = "123 Health Avenue, Cape Town, Western Cape, 8000", Phone = "+27 21 555 1234", EmailAddress = "info@sunrisemedical.co.za", Description = "Sunrise Medical Centre is a state-of-the-art healthcare facility offering comprehensive care, modern technology, and highly qualified staff.", Website = "https://www.sunrisemedical.co.za", DirectorName = "Dr. Lindiwe Mokoena", LastUpdated = DateTime.Now }
               //new PatientMedicationScript { Id = 2, PatientId = 2, MedicationId = 2, VisitID = 2, ApplicationUserID = 2, PrescriptionId = null, AdministeredDate = new DateTime(2025, 7, 14, 11, 15, 0), Dosage = "1 tablet daily after meal" }
            );
        }
    }
}
