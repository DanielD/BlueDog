using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueDog.Models
{

    public enum ResponseCode
    {
        Ok,
        NeedsAuthentication,
        EmailInUse,
        Invalid,
        Expired,
        Error
    }



    public class ServerResponse
    {
        public ResponseCode status { get; set; }
        public string message { get; set; }
        public string Token { get; set; }
        public object data { get; set; }
    }

    public class UserData
    {
        public UserData(User user)
        {
            id = user.id;
            email = user.email;
            created = user.created;
            emailValidated = user.emailValidated;
            resetPasswordExpires = user.resetPasswordExpires;
        }
        [JsonProperty(PropertyName = "id")]
        public string id { get; set; }
        public string email { get; set; }
        //public string password { get; set; }
        //public string salt { get; set; }
        public DateTime? created { get; set; }
        public bool emailValidated { get; set; }
        //public string emailValidationCode { get; set; }
        //public string resetPasswordCode { get; set; }
        public DateTime? resetPasswordExpires { get; set; }
    }

}
