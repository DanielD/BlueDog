using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueDog.Models
{
    public class TokenData
    {
        public DateTime? issued { get; set; }
        public DateTime? expires { get; set; }
        public string userid { get; set; }
    }
}
