namespace HMSBackend.Models
{
    public class Doctor
    {
        public string doctor_id { get; set; }
        public string name { get; set; }
        public string mob_no { get; set; }
        public string email { get; set; }
        public string[] specialist { get; set; }
        public string qualification { get; set; }
        public string address { get; set; }
        public string created_by { get; set; }
        public DateTime created_date { get; set; }
        public string updated_by { get; set; }
        public DateTime updated_date { get; set; }

    }
}
