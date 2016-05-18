﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using TheLearningMaze_API.Custom;
using TheLearningMaze_API.Models;

namespace TheLearningMaze_API.Filters
{
    public class ProfAuthFilter : AuthorizationFilterAttribute
    {
        //private bool v = true;
        private ApplicationDbContext db = new ApplicationDbContext();

        public ProfAuthFilter()
        {
            /*this.v = v*/;
        }

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
            return;
        }

        private HttpStatusCodeCustom AuthorizeRequest(HttpActionContext actionContext)
        {
            // Verifica se o cabeçalho contém "Authorization"
            if (actionContext.Request.Headers.Authorization != null)
            {
                string token = actionContext.Request.Headers.Authorization.ToString();

                // Faz decode do Token para extrair codProfessor e token original
                TokenProf tokenProf = new TokenProf().DecodeToken(token);

                Token tokenEntity = db.Tokens
                                        .Where(t => t.token == tokenProf.token)
                                        .FirstOrDefault();


                if (tokenEntity != null || tokenProf.codProfessor == tokenEntity.codProfessor)
                {
                    if (tokenEntity.expiraEm >= DateTime.Now)
                    {
                        // Adiciona 15 minutos ao tempo de expiração
                        tokenEntity.expiraEm = DateTime.Now.AddMinutes(15);
                        db.Entry(tokenEntity).State = EntityState.Modified;
                        db.SaveChanges();

                        return HttpStatusCodeCustom.OK; //Se token existe e é válido
                    }
                    return HttpStatusCodeCustom.TokenExpired; //Se token existe mas expirou
                }
            }
            return HttpStatusCodeCustom.Unauthorized; //Se token não existe
        }
    }
}