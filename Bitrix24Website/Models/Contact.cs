using System.ComponentModel.DataAnnotations;

namespace Bitrix24Website.Models
{
    public class Contact
    {
        public int Id { get; set; }
        public string NAME { get; set; }
        public List<ContactField> PHONE { get; set; }
        public List<ContactField> EMAIL { get; set; }
        public List<ContactField> WEB { get; set; }
        public DateTime DATE_CREATE { get; set; } = DateTime.Now;
    }

    public class ContactField
    {
        public string ID { get; set; }
        public string VALUE_TYPE { get; set; }
        public string VALUE { get; set; }
        public string TYPE_ID { get; set; }
    }
}
