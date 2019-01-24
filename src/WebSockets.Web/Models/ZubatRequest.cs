using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebSockets.Web.Models
{
    public class ZubatRequest
    {
        public bool IsKillerRequest { get; set; }

        public string Character { get; set; }

        public string RequestText { get; set; }
    }
}
