using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiChat.Web.Auth.Models
{
    public class EmailGitHub
    {
        public string email { get; set; }
        public bool primary { get; set; }
        public bool verified { get; set; }
        public string visibilit { get; set; }
}
}
