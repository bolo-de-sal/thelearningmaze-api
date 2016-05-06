using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;

namespace TheLearningMaze_API.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager, string authenticationType)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, authenticationType);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("SenaQuiz", throwIfV1Schema: false)
        {
        }
        
        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        public System.Data.Entity.DbSet<TheLearningMaze_API.Models.Professor> Professors { get; set; }

        public System.Data.Entity.DbSet<TheLearningMaze_API.Models.Token> Tokens { get; set; }

        public System.Data.Entity.DbSet<TheLearningMaze_API.Models.Area> Areas { get; set; }

        public System.Data.Entity.DbSet<TheLearningMaze_API.Models.TipoQuestao> TipoQuestaos { get; set; }

        public System.Data.Entity.DbSet<TheLearningMaze_API.Models.Status> Status { get; set; }

        public System.Data.Entity.DbSet<TheLearningMaze_API.Models.TipoEvento> TipoEventoes { get; set; }

        public System.Data.Entity.DbSet<TheLearningMaze_API.Models.Curso> Cursoes { get; set; }

        public System.Data.Entity.DbSet<TheLearningMaze_API.Models.Assunto> Assuntoes { get; set; }

        public System.Data.Entity.DbSet<TheLearningMaze_API.Models.Questao> Questaos { get; set; }

        public System.Data.Entity.DbSet<TheLearningMaze_API.Models.Imagem> Imagems { get; set; }

        public System.Data.Entity.DbSet<TheLearningMaze_API.Models.Alternativa> Alternativas { get; set; }

        public System.Data.Entity.DbSet<TheLearningMaze_API.Models.Participante> Participantes { get; set; }

        public System.Data.Entity.DbSet<TheLearningMaze_API.Models.Evento> Eventoes { get; set; }

        public System.Data.Entity.DbSet<TheLearningMaze_API.Models.QuestaoEvento> QuestaoEventoes { get; set; }

        public System.Data.Entity.DbSet<TheLearningMaze_API.Models.Grupo> Grupos { get; set; }

        public System.Data.Entity.DbSet<TheLearningMaze_API.Models.QuestaoGrupo> QuestaoGrupos { get; set; }

        public System.Data.Entity.DbSet<TheLearningMaze_API.Models.ParticipanteGrupo> ParticipanteGrupos { get; set; }

        public System.Data.Entity.DbSet<TheLearningMaze_API.Models.EventoAssunto> EventoAssuntoes { get; set; }
    }
}