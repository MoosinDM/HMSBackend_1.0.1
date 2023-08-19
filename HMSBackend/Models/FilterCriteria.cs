namespace HMSBackend.Models
{
    public class FilterCriteria
    {
        public string doctor_id { get; set; }
        public string name { get; set; }
        public string specialist { get; set; } // Change this to an array of strings
        public int page { get; set; }
        public int pageSize { get; set; }
        public string order { get; set; }
        public string sortBy { get; set; }
    }


}
