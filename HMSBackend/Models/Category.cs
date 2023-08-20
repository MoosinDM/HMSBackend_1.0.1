namespace HMSBackend.Models
{
    public class Category
    {
        public string category_id { get; set; }
        public string category_name { get; set; }
        public string created_name { get; set; }
        public string updated_name { get; set; }
        public DateTime created_date {  get; set; }
        public DateTime updated_date { get; set; }
    }
}
