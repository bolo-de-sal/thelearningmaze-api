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