using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TicketApp
{
    public class PersonClass 
    {
        public string subjectDN { get; set; }
        public string edipi { get; set; }
        public string FriendlyName { get; set; }
        public string GetEffectiveDateString { get; set; }
        public string GetExpirationDateString { get; set; }
        public string SimpleName { get; set; }
        public string UrlName { get; set; }
        public string EmailName { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }

    }
}
