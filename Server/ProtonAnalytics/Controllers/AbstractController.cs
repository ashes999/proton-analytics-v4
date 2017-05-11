using ProtonAnalytics.Repositories;
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

        protected IGenericRepository repository;

        public AbstractController(IGenericRepository repository)
        {
            this.repository = repository;
        }

        public void Flash(string message)
        {
            this.TempData[FlashMessage] = message;
        }

        public Guid CurrentUserId
        {
            get
            {
                if (this.User.Identity.IsAuthenticated)
                {
                    var email = this.User.Identity.Name;
                    // MVC/EF already guarantee unique user names.
                    // I don't see where (there's no DB constraint).

                    // TODO: BAD! BAD! BAD! No SQL in my web application layer!!!
                    var id = this.repository.ExecuteScalar<string>("SELECT Id FROM AspNetUsers WHERE email = @email", new { email = email });
                    return Guid.Parse(id);
                }
                else
                {
                    throw new InvalidOperationException("User is not logged in.");
                }
            }
        }
    }
}