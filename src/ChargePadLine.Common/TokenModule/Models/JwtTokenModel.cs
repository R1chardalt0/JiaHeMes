using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChargePadLine.Common.TokenModule.Models
{
    public class JwtTokenModel
    {
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public DateTime Expiration { get; set; }
        public int Expires { get; set; }
        public string Security { get; set; }
        public long Id { get; set; }
        public string UserName { get; set; }
        public string RefreshToken { get; set; }
    }
}