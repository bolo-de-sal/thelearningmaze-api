using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Description;
using TheLearningMaze_API.Filters;
using TheLearningMaze_API.Models;

namespace TheLearningMaze_API.Controllers
{
    
    public class ProfessorsController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: api/Professors/5
        [ApiAuthFilter(true)]
        [ResponseType(typeof(Professor))]
        public IHttpActionResult GetProfessor(int id)
        {
            Professor professor = db.Professors.Find(id);
            if (professor == null)
            {
                return NotFound();
            }

            return Ok(professor);
        }

        // POST: api/Professors/login
        [ApiAuthFilter(true)]
        [HttpPost]
        [Route("api/Professors/login")]
        public IHttpActionResult LoginProfessor(Professor professor)
        {
            if (professor.email == null || professor.senhaText == null)
            {
                return BadRequest();
            }
            else
            {
                Professor _professor = db.Professors
                                        .Where(p => p.email == professor.email)
                                        .FirstOrDefault();
                byte[] pwProfessor = new SHA1CryptoServiceProvider().ComputeHash(Encoding.ASCII.GetBytes(professor.senhaText));
                if (pwProfessor.SequenceEqual(_professor.senha))
                {
                    // Pega token do Header
                    string token = Request.Headers.Authorization.ToString().Substring(6);
                    Token tokenEntity = db.Tokens
                                        .Where(t => t.token == token)
                                        .FirstOrDefault();

                    // Gera novo token com codProfessor embutido
                    //byte[] newToken = System.Text.Encoding.ASCII.GetBytes(@"{""codProfessor"": """ + _professor.codProfessor + """, ""token"": """ + token + """}");
                    TokenProf tokenProf = new TokenProf(_professor.codProfessor, token);
                    byte[] newToken = tokenProf.GenerateToken(tokenProf);

                    tokenEntity.codProfessor = _professor.codProfessor;
                    db.Entry(tokenEntity).State = EntityState.Modified;
                    db.SaveChanges();

                    return Content(HttpStatusCode.OK, new { _professor, newToken });
                }
                else
                {
                    return Unauthorized();
                }
            }

        }

        [ProfAuthFilter] // Utilizar filtro com token de professor
        [HttpPost]
        [Route("api/Professors/logout")]
        public IHttpActionResult LogoutProfessor()
        {
            string token = Request.Headers.Authorization.ToString();
            TokenProf tokenProf = new TokenProf().DecodeToken(token);
            Token tokenEntity = db.Tokens
                                .Where(t => t.token == tokenProf.token)
                                .FirstOrDefault();

            tokenEntity.expiraEm = DateTime.Now;
            db.Entry(tokenEntity).State = EntityState.Modified;
            db.SaveChanges();

            return Ok();
        }

        //// GET: api/Professors
        //public IQueryable<Professor> GetProfessors()
        //{
        //    return db.Professors;
        //}

        //// PUT: api/Professors/5
        //[ResponseType(typeof(void))]
        //public IHttpActionResult PutProfessor(int id, Professor professor)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    if (id != professor.codProfessor)
        //    {
        //        return BadRequest();
        //    }

        //    db.Entry(professor).State = EntityState.Modified;

        //    try
        //    {
        //        db.SaveChanges();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!ProfessorExists(id))
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

        //// POST: api/Professors
        //[ResponseType(typeof(Professor))]
        //public IHttpActionResult PostProfessor(Professor professor)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    db.Professors.Add(professor);
        //    db.SaveChanges();

        //    return CreatedAtRoute("DefaultApi", new { id = professor.codProfessor }, professor);
        //}

        //// DELETE: api/Professors/5
        //[ResponseType(typeof(Professor))]
        //public IHttpActionResult DeleteProfessor(int id)
        //{
        //    Professor professor = db.Professors.Find(id);
        //    if (professor == null)
        //    {
        //        return NotFound();
        //    }

        //    db.Professors.Remove(professor);
        //    db.SaveChanges();

        //    return Ok(professor);
        //}

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ProfessorExists(int id)
        {
            return db.Professors.Count(e => e.codProfessor == id) > 0;
        }
    }
}