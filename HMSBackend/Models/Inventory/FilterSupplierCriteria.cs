namespace HMSBackend.Models.Inventory
{
    public class FilterSupplierCriteria
    {
        public string supplier_id { get; set; }
        public string supplier_name { get; set; }
        public int page { get; set; }
        public int pageSize { get; set; }
        public string order { get; set; }
        public string sortBy { get; set; }
    }
}
