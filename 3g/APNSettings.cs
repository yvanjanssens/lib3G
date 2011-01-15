using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace _3g
{
    public enum APNType {
        PPP,
        IPv4,
        IPv6
    }

    public class APNSettings
    {
        public string APNname { get; set; }
        public APNType ApnType { get; set; }
    }
}
