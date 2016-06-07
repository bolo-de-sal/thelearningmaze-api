using System.Collections.Generic;
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
            if (grupo == null) return Content(HttpStatusCode.NotFound, new { message = "Não foi encontrado grupo especificado" });
            Assunto assunto = db.Assuntos.Find(grupo.codAssunto);
            if (assunto == null) return Content(HttpStatusCode.NotFound, new { message = "Não foi encontrado assunto especificado" });
            return Ok(assunto);
        }

        // GET: api/Grupos/5/Acertos
        [HttpGet]
        [Route("api/Grupos/{id}/Acertos")]
        public IHttpActionResult GetAcertosGrupo(int id)
        {
            List<QuestaoGrupo> qg = db.QuestaoGrupos
                                .Where(q => q.codGrupo == id)
                                .OrderBy(q => q.tempo)
                                .ToList();
            if (qg == null) return Content(HttpStatusCode.NotFound, new { message = "Grupo não tem questões" });

            return Ok(qg);
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