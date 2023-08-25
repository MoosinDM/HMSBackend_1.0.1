namespace HMSBackend.Models.Inventory
{
    public class Supplier
    {
        public string supplier_id { get; set; }
        public string supplier_name { get; set; }
        public string contact_name { get; set; }
        public string contact_email { get; set; }
        public string contact_phone { get; set; }
        public string address { get; set; }
        public string created_by { get; set; }
        public DateTime created_date { get; set; }
        public string updated_by { get; set; }
        public DateTime updated_date { get; set; }
    }
}
