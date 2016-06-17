using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
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
            HttpStatusCodeCustom status = AuthorizeRequest(actionContext);

            if (status == HttpStatusCodeCustom.OK)
            {
                return;
            }

            HandleUnauthorizedRequest(actionContext, status);
        }

        protected void HandleUnauthorizedRequest(HttpActionContext actionContext, HttpStatusCodeCustom status)
        {
            actionContext.Response = new HttpResponseMessage((HttpStatusCode)status);
        }

        private HttpStatusCodeCustom AuthorizeRequest(HttpActionContext actionContext)
        {
            using (var dbContext = new ApplicationDbContext())
            {
                // Verifica se o cabeçalho contém "Authorization"
                if (actionContext.Request.Headers.Authorization == null)
                    return HttpStatusCodeCustom.Unauthorized; //Se token não existe

                var token = actionContext.Request.Headers.Authorization.ToString();

                // Faz decode do Token para extrair codProfessor e token original
                var tokenProf = new TokenProf().DecodeToken(token);

                var tokenEntity = dbContext.Tokens.FirstOrDefault(t => t.token == tokenProf.token);

                if (tokenEntity == null || tokenProf.codProfessor != tokenEntity.codProfessor)
                    return HttpStatusCodeCustom.Unauthorized; //Se token não existe

                if (tokenEntity.expiraEm < DateTime.Now)
                    return HttpStatusCodeCustom.TokenExpired; //Se token existe mas expirou
                    
                // Adiciona 15 minutos ao tempo de expiração
                tokenEntity.expiraEm = DateTime.Now.AddMinutes(60);
                dbContext.Entry(tokenEntity).State = EntityState.Modified;
                dbContext.SaveChanges();

                return HttpStatusCodeCustom.OK; //Se token existe e é válido
            }
        }
    }
}