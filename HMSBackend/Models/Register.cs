namespace HMSBackend.Models
{
    public class Register
    {
        public string name { get; set; }
        public string username { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public string mob_no { get; set; }
        public string role { get; set; }
        public Boolean is_default { get; set; }
        public string created_by { get; set; }
    }
}
