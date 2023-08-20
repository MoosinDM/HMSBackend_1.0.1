namespace HMSBackend.Models
{
    public class FilterCategoryCriteria
    {
        public string category_id { get; set; }
        public string category_name { get; set; }
        public int page { get; set; }
        public int pageSize { get; set; }
        public string order { get; set; }
        public string sortBy { get; set; }
    }
}
