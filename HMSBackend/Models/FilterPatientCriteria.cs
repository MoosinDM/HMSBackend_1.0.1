namespace HMSBackend.Models
{
    public class FilterPatientCriteria
    {
        public string patient_id { get; set; }
        public string name { get; set; }
        public string sortBy { get; set; }
        public string order { get; set; }
        public int pageSize { get; set; }
        public int page { get; set; }

    }
}
