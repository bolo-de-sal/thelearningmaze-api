using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using TheLearningMaze_API.Models;

namespace TheLearningMaze_API.Filters
{
    public class ApiAuthFilter : AuthorizationFilterAttribute
    {
        private bool v;
        private ApplicationDbContext db = new ApplicationDbContext();

        public ApiAuthFilter(bool v)
        {
            this.v = v;
        }

        protected bool OnAuthorizeUser(string token, HttpActionContext actionContext)
        {
            return true;
        }

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            if (AuthorizeRequest(actionContext))
            {
                return;
            }
            HandleUnauthorizedRequest(actionContext);
        }

        protected void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            actionContext.Response.StatusCode = HttpStatusCode.Forbidden;
            return;
        }

        private bool AuthorizeRequest(HttpActionContext actionContext)
        {
            if (actionContext.Request.Headers.Authorization != null)
            {
                string token = actionContext.Request.Headers.Authorization.ToString();
                Token tokenEntity = db.Tokens.Find(token);
                if (tokenEntity.expiraEm >= DateTime.Now)
                {
                    return true;
                }
            }

            //Write your code here to perform authorization
            return false;
        }
    }
}