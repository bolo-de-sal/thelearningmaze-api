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
    [ProfAuthFilter]
    public class EventosController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();




        // GET: api/Eventos/5/10
        [Route("api/Eventos/Paged/{page}/{perPage}")]
        public Object GetEventosPaginated(int page = 0, int perPage = 10)
        {
            var eventos = db.Eventos.OrderByDescending(d => d.data);
            int totalEventos = eventos.Count();
            int totalPaginas = (int)Math.Ceiling((double)totalEventos / perPage);

            var retorno = eventos
                .Skip(perPage * page)
                .Take(perPage)
                .ToList();


            return new
            {
                TotalEventos = totalEventos,
                TotalPaginas = totalPaginas,
                Eventos = retorno
            };
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