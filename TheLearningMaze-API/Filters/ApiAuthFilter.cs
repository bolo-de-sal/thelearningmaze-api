using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Mvc;
using TheLearningMaze_API.Custom;
using TheLearningMaze_API.Models;

namespace TheLearningMaze_API.Filters
{
    public class ApiAuthFilter : AuthorizationFilterAttribute
    {
        private bool v = true;
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
            var status = AuthorizeRequest(actionContext);

            if ((HttpStatusCodeCustom)status.StatusCode == HttpStatusCodeCustom.OK)
                return;

            HandleUnauthorizedRequest(actionContext, status);
        }

        protected void HandleUnauthorizedRequest(HttpActionContext actionContext, HttpStatusCodeResult status)
        {
            //actionContext.Response = new HttpResponseMessage((HttpStatusCode)status);
            //actionContext.Request.CreateResponse((HttpStatusCode)status.StatusCode, status.StatusDescription);
            actionContext.Response = actionContext.Request.CreateResponse((HttpStatusCode)status.StatusCode, status.StatusDescription);
        }

        private HttpStatusCodeResult AuthorizeRequest(HttpActionContext actionContext)
        {
            // Verifica se o cabeçalho contém "Authorization"
            if (actionContext.Request.Headers.Authorization != null)
            {
                string token = actionContext.Request.Headers.Authorization.ToString().Substring(6); //Retira "Token "
                Token tokenEntity = db.Tokens
                                        .Where(t => t.token == token)
                                        .FirstOrDefault();
                if (tokenEntity != null)
                {
                    if (tokenEntity.expiraEm >= DateTime.Now)
                    {
                        // Adiciona 15 minutos ao tempo de expiração
                        tokenEntity.expiraEm = DateTime.Now.AddMinutes(15);
                        db.Entry(tokenEntity).State = EntityState.Modified;
                        db.SaveChanges();

                        return new HttpStatusCodeResult(HttpStatusCode.OK); //Se token existe e é válido
                    }
                    //return HttpStatusCodeCustom.TokenExpired; //Se token existe mas expirou
                    return new HttpStatusCodeResult(HttpStatusCode.Unauthorized, "Token expirado");
                }
            }

            return new HttpStatusCodeResult(HttpStatusCode.Unauthorized); //Se token não existe
        }
    }
}