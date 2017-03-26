using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueDog.Models
{
    public class User
    {
        [JsonProperty(PropertyName = "id")]
        public string id { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public string salt { get; set; }
        public DateTime? created { get; set; }
        public bool emailValidated { get; set; }
        public string emailValidationCode { get; set; }
        public DateTime? emailValidationExpires { get; set; }
        public string resetPasswordCode { get; set; }
        public DateTime? resetPasswordExpires { get; set; }
    }
}
