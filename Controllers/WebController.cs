using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ETour.Web.Controllers
{
    public class WebController : Controller
    {
        protected string JavaScriptMsg(string msg)
        {

            return "alert('" + msg + "')";
        }

        protected string JavaScriptSuccessMsg(string msg)
        {
            return "alert('" + msg + "');history.back(-1);";
        }
        protected string JavaScriptTypeMsg(string msg, string type)
        {
            return "alert('" + msg + "');location.href='" + type + "';";
        }
    }
}
