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
    public class TipoQuestaosController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: api/TipoQuestaos
        public IQueryable<TipoQuestao> GetTipoQuestaos()
        {
            return db.TipoQuestaos;
        }

        // GET: api/TipoQuestaos/5
        [ResponseType(typeof(TipoQuestao))]
        public IHttpActionResult GetTipoQuestao(string id)
        {
            TipoQuestao tipoQuestao = db.TipoQuestaos.Find(id);
            if (tipoQuestao == null)
            {
                return NotFound();
            }

            return Ok(tipoQuestao);
        }

        // PUT: api/TipoQuestaos/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutTipoQuestao(string id, TipoQuestao tipoQuestao)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != tipoQuestao.codTipoQuestao)
            {
                return BadRequest();
            }

            db.Entry(tipoQuestao).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TipoQuestaoExists(id))
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

        // POST: api/TipoQuestaos
        [ResponseType(typeof(TipoQuestao))]
        public IHttpActionResult PostTipoQuestao(TipoQuestao tipoQuestao)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.TipoQuestaos.Add(tipoQuestao);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateException)
            {
                if (TipoQuestaoExists(tipoQuestao.codTipoQuestao))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = tipoQuestao.codTipoQuestao }, tipoQuestao);
        }

        // DELETE: api/TipoQuestaos/5
        [ResponseType(typeof(TipoQuestao))]
        public IHttpActionResult DeleteTipoQuestao(string id)
        {
            TipoQuestao tipoQuestao = db.TipoQuestaos.Find(id);
            if (tipoQuestao == null)
            {
                return NotFound();
            }

            db.TipoQuestaos.Remove(tipoQuestao);
            db.SaveChanges();

            return Ok(tipoQuestao);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool TipoQuestaoExists(string id)
        {
            return db.TipoQuestaos.Count(e => e.codTipoQuestao == id) > 0;
        }
    }
}