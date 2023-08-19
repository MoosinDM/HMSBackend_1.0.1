namespace HMSBackend.Models
{
    public class FilterDiseaseMasterCriteria
    {
        public string disease_id { get; set; }
        public int page { get; set; }
        public int pageSize { get; set; }
        public string order { get; set; }
        public string sortBy { get; set; }
    }
}
