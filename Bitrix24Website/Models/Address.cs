namespace Bitrix24Website.Models
{
    public class Address
    {
        public string ENTITY_ID { get; set; }
        public string TYPE_ID { get; set; }
        public string ENTITY_TYPE_ID { get; set; }
        // Street, house no 
        public string ADDRESS_1 { get; set; }
        // Apartment, office, room, floor
        public string ADDRESS_2 { get; set; }
        // Ward
        public string CITY { get; set; }
        // Province
        public string REGION { get; set; }
        // District
        public string PROVINCE { get; set; }
        public string COUNTRY { get; set; }
    }

}
