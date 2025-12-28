namespace ONT_3rdyear_Project.ViewModels
{
    public class DashboardStatsViewModel
    {
        public int TotalPatients { get; set; }
        public int TreatmentsToday { get; set; }
        public int MedicationsGivenToday { get; set; }
        //public int HoursOnDuty { get; set; }  // if you want to calculate or keep static
        public int TotalVitals {get; set;}
    }
}
