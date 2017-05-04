using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProtonAnalytics.Controllers
{
    public class AbstractController : Controller
    {
        public static readonly string FlashMessage = "FlashMessage";

        public void Flash(string message)
        {
            this.TempData[FlashMessage] = message;
        }
    }
}