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
    public class ProfAuthFilter : AuthorizationFilterAttribute
    {
        protected bool OnAuthorizeUser(string token, HttpActionContext actionContext)
        {
            return true;
        }

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            var status = AuthorizeRequest(actionContext);

            if ((HttpStatusCodeCustom)status.StatusCode == HttpStatusCodeCustom.OK)
            {
                return;
            }

            HandleUnauthorizedRequest(actionContext, status);
        }

        protected void HandleUnauthorizedRequest(HttpActionContext actionContext, HttpStatusCodeResult status)
        {
            actionContext.Response = actionContext.Request.CreateResponse((HttpStatusCode)status.StatusCode, status.StatusDescription);
        }

        private HttpStatusCodeResult AuthorizeRequest(HttpActionContext actionContext)
        {
            using (var dbContext = new ApplicationDbContext())
            {
                // Verifica se o cabeçalho contém "Authorization"
                if (actionContext.Request.Headers.Authorization == null)
                    return new HttpStatusCodeResult(HttpStatusCode.Unauthorized); //Se token não existe 

                var token = actionContext.Request.Headers.Authorization.ToString();

                // Faz decode do Token para extrair codProfessor e token original
                var tokenProf = new TokenProf().DecodeToken(token);

                var tokenEntity = dbContext.Tokens.FirstOrDefault(t => t.token == tokenProf.token);

                if (tokenEntity == null || tokenProf.codProfessor != tokenEntity.codProfessor || tokenEntity.expiraEm < DateTime.Now)
                    return new HttpStatusCodeResult(HttpStatusCode.RequestTimeout, "Token expirado"); //Se token não existe ou expirou

                // Adiciona 480 minutos (8 horas) ao tempo de expiração
                tokenEntity.expiraEm = DateTime.Now.AddMinutes(480);
                dbContext.Entry(tokenEntity).State = EntityState.Modified;
                dbContext.SaveChanges();

                return new HttpStatusCodeResult(HttpStatusCode.OK); //Se token existe e é válido
            }
        }
    }
}