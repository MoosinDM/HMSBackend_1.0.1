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

    }
}
