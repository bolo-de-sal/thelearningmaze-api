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
using TheLearningMaze_API.Filters;
using TheLearningMaze_API.Models;

namespace TheLearningMaze_API.Controllers
{
    //[ProfAuthFilter]
    public class EventosController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: api/Eventos/5/10
        [Route("api/Eventos/Paged/{page}/{perPage}")]
        public IHttpActionResult GetEventosPaginated(int page = 0, int perPage = 10)
        {
            var eventos = db.Eventos.OrderByDescending(d => d.data);
            int totalEventos = eventos.Count();
            int totalPaginas = (int)Math.Ceiling((double)totalEventos / perPage);

            var retorno = eventos
                .Skip(perPage * page)
                .Take(perPage)
                .ToList();


            return Ok(new
            {
                TotalEventos = totalEventos,
                TotalPaginas = totalPaginas,
                Eventos = retorno
            });
        }

        // GET: api/Eventos/5
        [ResponseType(typeof(Evento))]
        public IHttpActionResult GetEvento(int id)
        {
            Evento evento = db.Eventos.Find(id);
            if (evento == null)
            {
                return NotFound();
            }

            return Ok(evento);
        }

        // GET: api/Eventos/5/Grupos
        [ResponseType(typeof(Grupo))]
        [Route("api/Eventos/{id}/Grupos")]
        public IHttpActionResult GetGruposEvento(int id)
        {
            Evento evento = db.Eventos.Find(id);
            if (evento == null)
            {
                return NotFound();
            }

            IEnumerable<Grupo> grupos = db.Grupos
                .Where(g => g.codEvento == evento.codEvento);
            if (grupos == null)
            {
                return NotFound();
            }

            return Ok(grupos);

        }

        // POST: /api/Eventos/Iniciar
        [HttpPost]
        [Route("api/Eventos/Iniciar")]
        public IHttpActionResult IniciarEvento(Evento ev)
        {
            // Seleciona evento e altera status
            Evento evento = db.Eventos.Find(ev.codEvento);
            if (evento == null) return NotFound();
            evento.codStatus = "E";
            db.Entry(evento).State = EntityState.Modified;

            // Sorteia ordem
            IEnumerable<Grupo> grupos = db.Grupos
                .Where(g => g.codEvento == evento.codEvento)
                .OrderBy(x => Guid.NewGuid());

            List<MasterEventosOrdem> retorno = new List<MasterEventosOrdem>();
            Byte i = 0;
            foreach(Grupo grupo in grupos)
            {
                MasterEventosOrdem ordem = new MasterEventosOrdem();
                ordem.codGrupo = grupo.codGrupo;
                ordem.ordem = i;
                retorno.Add(ordem);
                db.MasterEventosOrdem.Add(ordem);
                i++;
            }
            
            db.SaveChanges();

            return Ok(retorno);
        }

        //// GET: api/Eventos
        //public IQueryable<Evento> GetEventos()
        //{
        //    return db.Eventos;
        //}

        //// PUT: api/Eventos/5
        //[ResponseType(typeof(void))]
        //public IHttpActionResult PutEvento(int id, Evento evento)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    if (id != evento.codEvento)
        //    {
        //        return BadRequest();
        //    }

        //    db.Entry(evento).State = EntityState.Modified;

        //    try
        //    {
        //        db.SaveChanges();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!EventoExists(id))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }

        //    return StatusCode(HttpStatusCode.NoContent);
        //}

        //// POST: api/Eventos
        //[ResponseType(typeof(Evento))]
        //public IHttpActionResult PostEvento(Evento evento)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    db.Eventos.Add(evento);
        //    db.SaveChanges();

        //    return CreatedAtRoute("DefaultApi", new { id = evento.codEvento }, evento);
        //}

        //// DELETE: api/Eventos/5
        //[ResponseType(typeof(Evento))]
        //public IHttpActionResult DeleteEvento(int id)
        //{
        //    Evento evento = db.Eventos.Find(id);
        //    if (evento == null)
        //    {
        //        return NotFound();
        //    }

        //    db.Eventos.Remove(evento);
        //    db.SaveChanges();

        //    return Ok(evento);
        //}

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool EventoExists(int id)
        {
            return db.Eventos.Count(e => e.codEvento == id) > 0;
        }
    }
}