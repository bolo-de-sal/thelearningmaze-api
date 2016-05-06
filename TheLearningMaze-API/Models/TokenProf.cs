using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace TheLearningMaze_API.Models
{
    public class TokenProf
    {
        public int codProfessor { get; set; }
        public string token { get; set; }

        public TokenProf()
        {
            // Construtor vazio
        }

        public TokenProf(int codProfessor, string token)
        {
            this.codProfessor = codProfessor;
            this.token = token;
        }

        public byte[] GenerateToken(TokenProf tokenProf)
        {
            string objJson = JsonConvert.SerializeObject(tokenProf);
            byte[] newToken = System.Text.Encoding.ASCII.GetBytes(objJson);
            return newToken;
        }

        public TokenProf DecodeToken(string token)
        {
            token = token.Substring(6); //Retira "Token "

            // Faz decode do Token para extrair codProfessor e token original
            byte[] data = Convert.FromBase64String(token);
            string decoded = Encoding.ASCII.GetString(data);
            TokenProf tokenProf = JsonConvert.DeserializeObject<TokenProf>(decoded);

            return tokenProf;
        }
    }
}