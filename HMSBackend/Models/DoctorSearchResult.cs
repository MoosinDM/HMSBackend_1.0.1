namespace HMSBackend.Models
{
    public class DoctorSearchResult
    {
        public int TotalCount { get; set; }
        public List<Doctor> Doctors { get; set; }
        public List<Patient> Patients { get; set; }
        public List<Patient_hist> PatientsHist { get; set; }
    }
}
