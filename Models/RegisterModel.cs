using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ETour.Web.Models
{
    public class RegisterModel
    {
        public string Mobile { get; set; }

        public string Password { get; set; }

        public string RePassword { get; set; }

        public string valCode { get; set; }
    }
}