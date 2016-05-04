using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using TheLearningMaze_API.Models;

namespace TheLearningMaze_API.Controllers
{
    public class ImagemsController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: api/Imagems
        public IQueryable<Imagem> GetImagems()
        {
            return db.Imagems;
        }

        // GET: api/Imagems/5
        [ResponseType(typeof(Imagem))]
        public IHttpActionResult GetImagem(int id)
        {
            Imagem imagem = db.Imagems.Find(id);
            if (imagem == null)
            {
                return NotFound();
            }

            return Ok(imagem);
        }

        // PUT: api/Imagems/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutImagem(int id, Imagem imagem)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != imagem.codImagem)
            {
                return BadRequest();
            }

            db.Entry(imagem).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ImagemExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/Imagems
        [ResponseType(typeof(Imagem))]
        public IHttpActionResult PostImagem(Imagem imagem)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Imagems.Add(imagem);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = imagem.codImagem }, imagem);
        }

        // DELETE: api/Imagems/5
        [ResponseType(typeof(Imagem))]
        public IHttpActionResult DeleteImagem(int id)
        {
            Imagem imagem = db.Imagems.Find(id);
            if (imagem == null)
            {
                return NotFound();
            }

            db.Imagems.Remove(imagem);
            db.SaveChanges();

            return Ok(imagem);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ImagemExists(int id)
        {
            return db.Imagems.Count(e => e.codImagem == id) > 0;
        }
    }
}