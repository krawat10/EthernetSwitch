using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EthernetSwitch.Data.Models
{
    public class Settings
    {
        [Key]
        public long Id { get; set; }
        public bool AllowTagging { get; set; }
        public bool RequireConfirmation { get; set; }
        public bool AllowRegistration { get; set; }
    }

}