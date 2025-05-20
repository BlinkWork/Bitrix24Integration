namespace Bitrix24Website.Models
{
    public class AddressCreateModel
    {
        public string Type_id { get; set; }
        public int Entity_type_id { get; set; }
        public int Entity_id { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string Province { get; set; }
        public string Country { get; set; }
    }
}
