namespace HMSBackend.Models
{
    public class FilterPatientHistCriteria
    {
        public string patient_hist_id { get; set; }
        public string patient_id { get; set; }
        public int page { get; set; }
        public int pageSize { get; set; }
        public string order { get; set; }
        public string sortBy { get; set; }
        public string patient_type { get; set; }
    }
}
