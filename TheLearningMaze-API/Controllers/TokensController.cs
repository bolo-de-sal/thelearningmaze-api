using System;
using System.Linq;
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
    }
}