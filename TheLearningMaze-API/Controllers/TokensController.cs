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
    public class TokensController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        [HttpGet]
        [ResponseType(typeof(Token))]
        [Route("api/Tokens/generateToken")]
        public IHttpActionResult GenerateToken()
        {
            Token token = new Token();

            token.token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            token.codProfessor = null;
            token.codEvento = null;
            token.concedidoEm = DateTime.Now;
            token.expiraEm = DateTime.Now.AddMinutes(15);

            db.Tokens.Add(token);
            db.SaveChanges();

            return Ok(token);
        }

        //// GET: api/Tokens
        //public IQueryable<Token> GetTokens()
        //{
        //    return db.Tokens;
        //}

        //// GET: api/Tokens/5
        //[ResponseType(typeof(Token))]
        //public IHttpActionResult GetToken(int id)
        //{
        //    Token token = db.Tokens.Find(id);
        //    if (token == null)
        //    {
        //        return NotFound();
        //    }

        //    return Ok(token);
        //}

        // GET: api/Tokens/generateToken
        //// PUT: api/Tokens/5
        //[ResponseType(typeof(void))]
        //public IHttpActionResult PutToken(int id, Token token)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    if (id != token.codToken)
        //    {
        //        return BadRequest();
        //    }

        //    db.Entry(token).State = EntityState.Modified;

        //    try
        //    {
        //        db.SaveChanges();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!TokenExists(id))
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

        //// POST: api/Tokens
        //[ResponseType(typeof(Token))]
        //public IHttpActionResult PostToken(Token token)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    db.Tokens.Add(token);
        //    db.SaveChanges();

        //    return CreatedAtRoute("DefaultApi", new { id = token.codToken }, token);
        //}

        //// DELETE: api/Tokens/5
        //[ResponseType(typeof(Token))]
        //public IHttpActionResult DeleteToken(int id)
        //{
        //    Token token = db.Tokens.Find(id);
        //    if (token == null)
        //    {
        //        return NotFound();
        //    }

        //    db.Tokens.Remove(token);
        //    db.SaveChanges();

        //    return Ok(token);
        //}

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool TokenExists(int id)
        {
            return db.Tokens.Count(e => e.codToken == id) > 0;
        }


    }
}