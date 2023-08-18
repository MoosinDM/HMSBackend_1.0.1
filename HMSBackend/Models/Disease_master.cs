namespace HMSBackend.Models
{
    public class Disease_master
    {
        public string disease_master_id { get; set; }
        public string specialist { get; set; }
        public string disease_type { get; set; }
        public string sub_disese_type { get; set; }
        public string treatment_type { get; set; }
        public string created_by { get; set; }
        public DateTime? created_date { get; set; }
        public string updated_by { get; set; }
        public DateTime? updated_date { get; set; }
    }
}
