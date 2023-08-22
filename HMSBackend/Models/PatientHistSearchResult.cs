namespace HMSBackend.Models
{
    public class PatientHistSearchResult
    {
        public int TotalCount { get; set; }
        public List<Patient_hist> Patients { get; set; }
    }
}
