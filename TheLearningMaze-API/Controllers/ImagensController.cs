using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using TheLearningMaze_API.Filters;
using TheLearningMaze_API.Models;

namespace TheLearningMaze_API.Controllers
{
    [ApiAuthFilter(true)]
    public class ImagensController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: api/Imagens/5
        [ResponseType(typeof(Imagem))]
        public HttpResponseMessage GetImagem(int id)
        {
            Imagem imagem = db.Imagems.Find(id);

            HttpResponseMessage response = new HttpResponseMessage();

            if (imagem == null)
            {
                response.StatusCode = HttpStatusCode.NotFound;
                return response;
            }

            MemoryStream ms = new MemoryStream(imagem.bitmapImagem);
            StreamContent sc = new StreamContent(ms);

            response.StatusCode = HttpStatusCode.OK;
            response.Content = sc;
            response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");


            return response;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}