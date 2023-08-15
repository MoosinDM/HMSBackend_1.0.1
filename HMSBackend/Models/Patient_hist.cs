namespace HMSBackend.Models
{
    public class Patient_hist
    {
        public string patient_hist_id { get; set; }
        public DateTime? date { get; set; }
        public string patient_type { get; set; }
        public string patient_id { get; set; }
        public string patient_name { get; set; }
        public string treatment_type { get; set; }
        public string[] specialist { get; set; }
        public string patient_description { get; set; }
        public string comment { get; set; }
        public string doctor_id { get; set; }
    }
}
