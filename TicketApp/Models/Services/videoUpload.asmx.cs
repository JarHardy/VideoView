using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Configuration;
using System.Web.Script.Serialization;

namespace TicketApp.Models
{
    /// <summary>
    /// Summary description for cacInformation
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class cacInformation : System.Web.Services.WebService
    {

        
        [WebMethod]
        public void uploadVideo(string videoStr)
        {
            localCacPull cacInfo = new localCacPull();
            var personinfo = cacInfo.GetCertInfo();
            
            JavaScriptSerializer js = new JavaScriptSerializer();
            Context.Response.Write(js.Serialize(personinfo));
           
        }

    }
}
