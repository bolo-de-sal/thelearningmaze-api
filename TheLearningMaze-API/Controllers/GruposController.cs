using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using TheLearningMaze_API.Filters;
using TheLearningMaze_API.Models;

namespace TheLearningMaze_API.Controllers
{
    [ProfAuthFilter]
    public class GruposController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: api/Grupos
        public IQueryable<Grupo> GetGrupos()
        {
            return db.Grupos;
        }

        // GET: api/Grupos/5
        [ResponseType(typeof(Grupo))]
        public IHttpActionResult GetGrupo(int id)
        {
            Grupo grupo = db.Grupos.Find(id);
            if (grupo == null)
            {
                return NotFound();
            }

            return Ok(grupo);
        }

        // GET: api/Grupos/5/Assunto
        [HttpGet]
        [ResponseType(typeof(Assunto))]
        [Route("api/Grupos/{id}/Assunto")]
        public IHttpActionResult GetGrupoAssunto(int id)
        {
            Grupo grupo = db.Grupos.Find(id);
            Assunto assunto = db.Assuntoes.Find(grupo.codAssunto);
            if (assunto == null)
                return NotFound();
            return Ok(assunto);
        }

        // PUT: api/Grupos/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutGrupo(int id, Grupo grupo)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != grupo.codGrupo)
            {
                return BadRequest();
            }

            db.Entry(grupo).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GrupoExists(id))
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

        // POST: api/Grupos
        [ResponseType(typeof(Grupo))]
        public IHttpActionResult PostGrupo(Grupo grupo)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Grupos.Add(grupo);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = grupo.codGrupo }, grupo);
        }

        // DELETE: api/Grupos/5
        [ResponseType(typeof(Grupo))]
        public IHttpActionResult DeleteGrupo(int id)
        {
            Grupo grupo = db.Grupos.Find(id);
            if (grupo == null)
            {
                return NotFound();
            }

            db.Grupos.Remove(grupo);
            db.SaveChanges();

            return Ok(grupo);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool GrupoExists(int id)
        {
            return db.Grupos.Count(e => e.codGrupo == id) > 0;
        }
    }
}