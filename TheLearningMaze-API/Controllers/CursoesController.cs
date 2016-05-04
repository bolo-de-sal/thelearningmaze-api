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
    public class CursoesController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: api/Cursoes
        public IQueryable<Curso> GetCursoes()
        {
            return db.Cursoes;
        }

        // GET: api/Cursoes/5
        [ResponseType(typeof(Curso))]
        public IHttpActionResult GetCurso(int id)
        {
            Curso curso = db.Cursoes.Find(id);
            if (curso == null)
            {
                return NotFound();
            }

            return Ok(curso);
        }

        // PUT: api/Cursoes/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutCurso(int id, Curso curso)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != curso.codCurso)
            {
                return BadRequest();
            }

            db.Entry(curso).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CursoExists(id))
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

        // POST: api/Cursoes
        [ResponseType(typeof(Curso))]
        public IHttpActionResult PostCurso(Curso curso)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Cursoes.Add(curso);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = curso.codCurso }, curso);
        }

        // DELETE: api/Cursoes/5
        [ResponseType(typeof(Curso))]
        public IHttpActionResult DeleteCurso(int id)
        {
            Curso curso = db.Cursoes.Find(id);
            if (curso == null)
            {
                return NotFound();
            }

            db.Cursoes.Remove(curso);
            db.SaveChanges();

            return Ok(curso);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool CursoExists(int id)
        {
            return db.Cursoes.Count(e => e.codCurso == id) > 0;
        }
    }
}